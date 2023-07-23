using System;
using System.Linq;
using System.Threading.Tasks;

namespace SynergyLib.FileFormat.DotSquish;

// From DotSquish
public static class Squish {
    private class BlockCompresser {
        private readonly SquishOptions _options;
        private readonly ColorSet _colors = new();
        private readonly SingleColorFit _singleColorFit;
        private readonly RangeFit _rangeFit;
        private readonly ClusterFit _clusterFit;

        public BlockCompresser(SquishOptions options) {
            _options = options;
            _singleColorFit = new(_colors, options);
            _rangeFit = new(_colors, options);
            _clusterFit = new(_colors, options);
        }

        public void CompressMasked(ReadOnlySpan<byte> bgra, int mask, Span<byte> block) {
            var colourBlock = _options.Method is SquishMethod.Dxt3 or SquishMethod.Dxt5 ? block[8..] : block;

            // create the minimal point set
            _colors.Reset(bgra, mask, _options);

            // check the compression type and compress colour
            if (_colors.Count == 1) {
                // always do a single colour fit
                _singleColorFit.Compress(colourBlock);
            } else if (_options.Fit == SquishFit.ColorRangeFit || _colors.Count == 0) {
                _rangeFit.Compress(colourBlock);
            } else {
                // default to a cluster fit (could be iterative or not)
                _clusterFit.Compress(colourBlock);
            }

            switch (_options.Method) {
                case SquishMethod.Dxt1:
                    break;
                case SquishMethod.Dxt3:
                    Alpha.CompressAlphaDxt3(bgra, mask, block);
                    break;
                case SquishMethod.Dxt5:
                    Alpha.CompressAlphaDxt5(bgra, mask, block);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }
    }

    internal static void Decompress(Span<byte> bgra, ReadOnlySpan<byte> block, SquishOptions options) {
        var colorBlock = options.Method is SquishMethod.Dxt3 or SquishMethod.Dxt5 ? block[8..] : block;

        switch (options.Method) {
            case SquishMethod.Dxt1:
                ColorBlock.DecompressColor(bgra, colorBlock, true);
                break;
            case SquishMethod.Dxt3:
                ColorBlock.DecompressColor(bgra, colorBlock, false);
                Alpha.DecompressAlphaDxt3(block, bgra);
                break;
            case SquishMethod.Dxt5:
                ColorBlock.DecompressColor(bgra, colorBlock, false);
                Alpha.DecompressAlphaDxt5(block, bgra);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(options), options, null);
        }
    }

    public static int GetStorageRequirements(int width, int height, SquishOptions options) {
        var blockCount = (width + 3) / 4 * ((height + 3) / 4);
        var blockSize = options.Method == SquishMethod.Dxt1 ? 8 : 16;
        return blockCount * blockSize;
    }

    public static unsafe void CompressImage<TBgra, TBlocks>(
        ReadOnlySpan<TBgra> bgra,
        int stride,
        int width,
        int height,
        Span<TBlocks> blocks,
        SquishOptions options)
        where TBgra : unmanaged
        where TBlocks : unmanaged {
        fixed (void* pBgra = bgra)
        fixed (void* pBlocks = blocks) {
            var bgraBytes = new Span<byte>(pBgra, bgra.Length * sizeof(TBgra));
            var blocksBytes = new Span<byte>(pBlocks, blocks.Length * sizeof(TBlocks));
            var strideBytes = sizeof(TBgra) * stride;

            // initialise the block output
            var bytesPerBlock = options.Method == SquishMethod.Dxt1 ? 8 : 16;
            var compresser = new BlockCompresser(options);

            // loop over blocks
            Span<byte> sourceBgra = stackalloc byte[16 * 4];
            for (var y = 0; y < height; y += 4) {
                for (var x = 0; x < width; x += 4) {
                    // build the 4x4 block of pixels
                    var targetPixel = sourceBgra;
                    var mask = 0;
                    for (var py = 0; py < 4; ++py) {
                        for (var px = 0; px < 4; ++px) {
                            // get the source pixel in the image
                            var sx = x + px;
                            var sy = y + py;

                            // enable if we're in the image
                            if (sx < width && sy < height) {
                                // copy the bgra value
                                bgraBytes.Slice(strideBytes * sy + 4 * sx, 4).CopyTo(targetPixel);

                                // enable this pixel
                                mask |= (1 << (4 * py + px));
                            }

                            // advance
                            targetPixel = targetPixel[4..];
                        }
                    }

                    // compress it into the output
                    compresser.CompressMasked(sourceBgra, mask, blocksBytes);

                    // advance
                    blocksBytes = blocksBytes[bytesPerBlock..];
                }
            }
        }
    }

    public static unsafe Task CompressImageAsync<TBgra, TBlocks>(
        ReadOnlyMemory<TBgra> bgraMemory,
        int stride,
        int width,
        int height,
        Memory<TBlocks> blocksMemory,
        SquishOptions options)
        where TBgra : unmanaged
        where TBlocks : unmanaged {
        var horzBlockCount = (width + 3) / 4;
        var vertBlockCount = (height + 3) / 4;
        var totalBlockCount = horzBlockCount * vertBlockCount;
        var threads = Math.Min(options.Threads < 1 ? Environment.ProcessorCount : options.Threads, totalBlockCount);

        if (threads < 2) {
            CompressImage(bgraMemory.Span, stride, width, height, blocksMemory.Span, options);
            return Task.CompletedTask;
        }

        // initialise the block output
        var bytesPerBlock = options.Method == SquishMethod.Dxt1 ? 8 : 16;
        var strideBytes = sizeof(TBgra) * stride;

        return Task.WhenAll(
            Enumerable.Range(0, threads).Select(
                i => Task.Run(
                    () => {
                        var compresser = new BlockCompresser(options);
                        var blockIndex = totalBlockCount * i / threads;
                        var blockIndexTo = totalBlockCount * (i + 1) / threads;

                        fixed (void* pBgra = bgraMemory.Span)
                        fixed (void* pBlocks = blocksMemory.Span) {
                            var bgra = new Span<byte>(pBgra, bgraMemory.Length * sizeof(TBgra));
                            var blocks = new Span<byte>(pBlocks, blocksMemory.Length * sizeof(TBlocks));
                            blocks = blocks[(bytesPerBlock * blockIndex)..];

                            // loop over blocks
                            Span<byte> sourceBgra = stackalloc byte[16 * 4];
                            for (; blockIndex < blockIndexTo; blockIndex++) {
                                var y = blockIndex / horzBlockCount * 4;
                                var x = blockIndex % horzBlockCount * 4;

                                options.CancellationToken.ThrowIfCancellationRequested();

                                // build the 4x4 block of pixels
                                var targetPixel = sourceBgra;
                                var mask = 0;
                                for (var py = 0; py < 4; ++py) {
                                    for (var px = 0; px < 4; ++px) {
                                        // get the source pixel in the image
                                        var sx = x + px;
                                        var sy = y + py;

                                        // enable if we're in the image
                                        if (sx < width && sy < height) {
                                            // copy the bgra value
                                            bgra.Slice(strideBytes * sy + 4 * sx, 4).CopyTo(targetPixel);

                                            // enable this pixel
                                            mask |= (1 << (4 * py + px));
                                        }

                                        // advance
                                        targetPixel = targetPixel[4..];
                                    }
                                }

                                // compress it into the output
                                compresser.CompressMasked(sourceBgra, mask, blocks);

                                // advance
                                blocks = blocks[bytesPerBlock..];
                            }
                        }
                    },
                    options.CancellationToken)));
    }

    public static unsafe void DecompressImage<TBgra, TBlocks>(
        Span<TBgra> bgra,
        int stride,
        int width,
        int height,
        ReadOnlySpan<TBlocks> blocks,
        SquishOptions options)
        where TBgra : unmanaged
        where TBlocks : unmanaged {
        fixed (void* pBgra = bgra)
        fixed (void* pBlocks = blocks) {
            var bgraBytes = new Span<byte>(pBgra, bgra.Length * sizeof(TBgra));
            var blocksBytes = new Span<byte>(pBlocks, blocks.Length * sizeof(TBlocks));
            var strideBytes = sizeof(TBgra) * stride;

            // initialise the block output
            var bytesPerBlock = options.Method == SquishMethod.Dxt1 ? 8 : 16;

            // loop over blocks
            Span<byte> targetBgra = stackalloc byte[16 * 4];
            for (var y = 0; y < height; y += 4) {
                for (var x = 0; x < width; x += 4) {
                    // decompress the block
                    Decompress(targetBgra, blocksBytes, options);

                    // write the decompressed pixels to the correct image locations
                    var sourcePixel = targetBgra;
                    for (var py = 0; py < 4; ++py) {
                        for (var px = 0; px < 4; ++px) {
                            // get the target location
                            var sx = x + px;
                            var sy = y + py;

                            // enable if we're in the image
                            if (sx < width && sy < height)
                                sourcePixel[..4].CopyTo(bgraBytes.Slice(strideBytes * sy + 4 * sx, 4));

                            // advance
                            sourcePixel = sourcePixel[4..];
                        }
                    }

                    // advance
                    blocksBytes = blocksBytes[bytesPerBlock..];
                }
            }
        }
    }
}
