using System;

namespace SynergyLib.FileFormat.DotSquish; 

// From DotSquish
internal static class Alpha {
    public static int FloatToInt(float a, int limit) {
        // Use ANSI round-to-zero behaviour to get round-to-nearest.
        var i = (int) (a + .5f);

        if (i < 0)
            i = 0;
        else if (i > limit)
            i = limit;

        return i;
    }

    public static void CompressAlphaDxt3(ReadOnlySpan<byte> bgra, int mask, Span<byte> block) {
        // Quantise and pack alpha values pairwise.
        for (var i = 0; i < 8; ++i) {
            // Qnatise down to 4 bits.
            var alpha1 = bgra[8 * i + 3] * (15f / 255f);
            var alpha2 = bgra[8 * i + 7] * (15f / 255f);
            var quant1 = FloatToInt(alpha1, 15);
            var quant2 = FloatToInt(alpha2, 15);

            // Set alpha to zero where masked.
            var bit1 = 1 << (2 * i);
            var bit2 = 1 << (2 * i + 1);
            if ((mask & bit1) == 0)
                quant1 = 0;
            if ((mask & bit2) == 0)
                quant2 = 0;

            // Pack into the byte.
            block[i] = (byte) (quant1 | (quant2 << 4));
        }
    }

    public static void DecompressAlphaDxt3(ReadOnlySpan<byte> block, Span<byte> target) {
        // Unpack the alpha values pairwise.
        for (var i = 0; i < 8; ++i) {
            // Quantise down to 4 bits.
            var quant = block[i];

            // Unpack the values.
            var lo = quant & 0x0f;
            var hi = quant & 0xf0;

            // Convert back up to bytes.
            target[8 * i + 3] = (byte) (lo | (lo << 4));
            target[8 * i + 7] = (byte) (hi | (hi >> 4));
        }
    }

    private static void FixRange(ref byte min, ref byte max, int steps) {
        if (max - min < steps)
            max = (byte) Math.Min(min + steps, 255);
        if (max - min < steps)
            min = (byte) Math.Max(0, max - steps);
    }

    private static int FitCodes(ReadOnlySpan<byte> bgra, int mask, ReadOnlySpan<byte> codes, Span<byte> indices) {
        // Fit each alpha value to the codebook.
        var err = 0;
        for (var i = 0; i < 16; ++i) {
            // Check this pixel is valid.
            var bit = 1 << i;
            if ((mask & bit) == 0) {
                // Use the first code.
                indices[i] = 0;
                continue;
            }

            // Find the least error and corresponding index.
            var value = bgra[4 * i + 3];
            var least = int.MaxValue;
            var index = 0;
            for (var j = 0; j < 8; ++j) {
                // Get the squared error from this code.
                var dist = value - codes[j];
                dist *= dist;

                // Compare with the best so far.
                if (dist < least) {
                    least = dist;
                    index = j;
                }
            }

            // Save this index and accumulate the error.
            indices[i] = (byte) index;
            err += least;
        }

        // Return the total error.
        return err;
    }

    private static void WriteAlphaBlock(int alpha0, int alpha1, ReadOnlySpan<byte> indices, Span<byte> target) {
        // Write the first two bytes.
        target[0] = (byte) alpha0;
        target[1] = (byte) alpha1;

        var indOff = 0;
        var retOff = 2;
        for (var i = 0; i < 2; ++i) {
            // Pack 8 3-bit values.
            var value = 0;
            for (var j = 0; j < 8; ++j) {
                var index = indices[indOff++];
                value |= index << 3 * j;
            }

            // Store in 3 bytes
            for (var j = 0; j < 3; ++j) {
                var b = (value >> (8 * j)) & 0xFF;
                target[retOff++] = (byte) b;
            }
        }
    }

    private static void WriteAlphaBlock5(int alpha0, int alpha1, ReadOnlySpan<byte> indices, Span<byte> block) {
        // Check the relative values of the endpoints.
        if (alpha0 > alpha1) {
            Span<byte> swapped = stackalloc byte[16];
            for (var i = 0; i < 16; ++i) {
                var index = indices[i];
                swapped[i] = index switch {
                    0 => 1,
                    1 => 0,
                    <= 5 => (byte) (7 - index),
                    _ => index,
                };
            }

            // Write the block.
            WriteAlphaBlock(alpha1, alpha0, swapped, block);
        } else {
            // Write the block.
            WriteAlphaBlock(alpha0, alpha1, indices, block);
        }
    }

    private static void WriteAlphaBlock7(int alpha0, int alpha1, ReadOnlySpan<byte> indices, Span<byte> block) {
        // Check the relative values of the endpoints.
        if (alpha0 < alpha1) {
            Span<byte> swapped = stackalloc byte[16];
            for (var i = 0; i < 16; ++i) {
                var index = indices[i];
                swapped[i] = index switch {
                    0 => 1,
                    1 => 0,
                    _ => (byte) (9 - index)
                };
            }

            // Write the block.
            WriteAlphaBlock(alpha1, alpha0, swapped, block);
        } else {
            // Write the block.
            WriteAlphaBlock(alpha0, alpha1, indices, block);
        }
    }

    public static void CompressAlphaDxt5(ReadOnlySpan<byte> bgra, int mask, Span<byte> block) {
        // Get the range for 5-alpha and 7-alpha interpolation.
        byte min5 = 255, max5 = 0;
        byte min7 = 255, max7 = 0;
        for (var i = 0; i < 16; ++i) {
            // Check this pixel is valid.
            var bit = 1 << i;
            if ((mask & bit) == 0)
                continue;

            // Incorporate into the min/max.
            var value = bgra[4 * i + 3];
            if (value < min7)
                min7 = value;
            if (value > max7)
                max7 = value;
            if (value != 0 && value < min5)
                min5 = value;
            if (value != 255 && value > max5)
                max5 = value;
        }

        // Handle the case that no valid range was found.
        if (min5 > max5)
            min5 = max5;
        if (min7 > max7)
            min7 = max7;

        // Fix the range to be the minimum in each case.
        FixRange(ref min5, ref max5, 5);
        FixRange(ref min7, ref max7, 7);

        // Set up the 5-alpha code book.
        Span<byte> codes5 = stackalloc byte[8];
        codes5[0] = min5;
        codes5[1] = max5;
        for (var i = 1; i < 5; ++i)
            codes5[i + 1] = (byte) (((5 - i) * min5 + i * max5) / 5);
        codes5[6] = 0;
        codes5[7] = 255;

        // Set up the 7-alpha code book.
        Span<byte> codes7 = stackalloc byte[8];
        codes7[0] = min7;
        codes7[1] = max7;
        for (var i = 1; i < 7; ++i)
            codes7[i + 1] = (byte) (((7 - i) * min7 + i * max7) / 7);

        // Fit the data to both code books.
        Span<byte> indices5 = stackalloc byte[16];
        Span<byte> indices7 = stackalloc byte[16];
        var err5 = FitCodes(bgra, mask, codes5, indices5);
        var err7 = FitCodes(bgra, mask, codes7, indices7);

        // Save the block with least error.
        if (err5 <= err7)
            WriteAlphaBlock5(min5, max5, indices5, block);
        else
            WriteAlphaBlock7(min7, max7, indices7, block);
    }

    public static void DecompressAlphaDxt5(ReadOnlySpan<byte> block, Span<byte> target) {
        // Get the two alpha values.
        var alpha0 = block[0];
        var alpha1 = block[1];

        // Compare the values to build the codebook.
        Span<byte> codes = stackalloc byte[8];
        codes[0] = alpha0;
        codes[1] = alpha1;
        if (alpha0 <= alpha1) {
            // Use the 5-alpha codebook.
            for (var i = 1; i < 5; ++i)
                codes[1 + i] = (byte) (((5 - i) * alpha0 + i * alpha1) / 5);
            codes[6] = 0;
            codes[7] = 255;
        } else {
            // Use the 7-alpha codebook.
            for (var i = 1; i < 7; ++i)
                codes[1 + i] = (byte) (((7 - i) * alpha0 + i * alpha1) / 7);
        }

        // Decode the incdices
        Span<byte> indices = stackalloc byte[16];
        var blOff = 2;
        var indOff = 0;
        for (var i = 0; i < 2; ++i) {
            // Grab 3 bytes
            var value = 0;
            for (var j = 0; j < 3; ++j) {
                var b = block[blOff++];
                value |= b << 8 * j;
            }

            // Unpack 8 3-bit values from it
            for (var j = 0; j < 8; ++j) {
                var index = (value >> 3 * j) & 0x7;
                indices[indOff++] = (byte) index;
            }
        }

        // Write out the index codebook values.
        for (var i = 0; i < 16; ++i)
            target[4 * i + 3] = codes[indices[i]];
    }
}