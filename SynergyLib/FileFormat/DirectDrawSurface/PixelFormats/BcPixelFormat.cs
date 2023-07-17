using System;
using BCnEncoder.Shared;
using SynergyLib.FileFormat.DirectDrawSurface.PixelFormats.Channels;

namespace SynergyLib.FileFormat.DirectDrawSurface.PixelFormats;

public class BcPixelFormat : IPixelFormat, IEquatable<BcPixelFormat> {
    public readonly ChannelType Type;
    public readonly byte Version;

    public BcPixelFormat(
        ChannelType type = ChannelType.Typeless,
        AlphaType alpha = AlphaType.Straight,
        byte version = 0) {
        if (version is < 1 or > 7)
            throw new ArgumentOutOfRangeException(nameof(version), version, null);

        Type = type;
        Alpha = alpha;
        Version = version;
    }

    public AlphaType Alpha { get; }
    
    public int Bpp => Version is 1 or 4 ? 4 : 8;

    public int BlockSize => Version is 1 or 4 ? 8 : 16;

    public void ToB8G8R8A8(Span<byte> target, int targetStride, ReadOnlySpan<byte> source, int sourceStride, int width,
        int height) {
        if (sourceStride * 2 != (width + 3) / 4 * 4 * Bpp)
            throw new ArgumentException("No padding is allowed for stride.", nameof(sourceStride));

        var blockSize = BlockSize;
        var decoder = new BCnEncoder.Decoder.BcDecoder();
        if (Version == 6) {
            var block = new ColorRgbFloat[4, 4];

            var isrc = 0;
            switch (Type) {
                case ChannelType.Sf16:
                    for (var y = 0; y < height; y += 4) {
                        for (var x = 0; x < width; x += 4) {
                            decoder.DecodeBlockHdr(source[isrc..(isrc + blockSize)], CompressionFormat.Bc6S, block);
                            isrc += blockSize;
                            var yn = Math.Min(4, height - y);
                            var xn = Math.Min(4, width - x);
                            for (var y1 = 0; y1 < yn; y1++) {
                                var offset = (y + y1) * targetStride + x * 4;
                                for (var x1 = 0; x1 < xn; x1++) {
                                    target[offset++] = (byte) Math.Round(127.5f + 127.5f * block[y1, x1].b);
                                    target[offset++] = (byte) Math.Round(127.5f + 127.5f * block[y1, x1].g);
                                    target[offset++] = (byte) Math.Round(127.5f + 127.5f * block[y1, x1].r);
                                    target[offset++] = 255;
                                }
                            }
                        }
                    }

                    break;
                case ChannelType.Uf16:
                    for (var y = 0; y < height; y += 4) {
                        for (var x = 0; x < width; x += 4) {
                            decoder.DecodeBlockHdr(source[isrc..(isrc + blockSize)], CompressionFormat.Bc6U, block);
                            isrc += blockSize;
                            var yn = Math.Min(4, height - y);
                            var xn = Math.Min(4, width - x);
                            for (var y1 = 0; y1 < yn; y1++) {
                                var offset = (y + y1) * targetStride + x * 4;
                                for (var x1 = 0; x1 < xn; x1++) {
                                    target[offset++] = (byte) Math.Round(255 * block[y1, x1].b);
                                    target[offset++] = (byte) Math.Round(255 * block[y1, x1].g);
                                    target[offset++] = (byte) Math.Round(255 * block[y1, x1].r);
                                    target[offset++] = 255;
                                }
                            }
                        }
                    }

                    break;
                default:
                    throw new NotSupportedException();
            }
        } else {
            var block = new ColorRgba32[4, 4];
            var fmt = Version switch {
                1 => CompressionFormat.Bc1,
                2 => CompressionFormat.Bc2,
                3 => CompressionFormat.Bc3,
                4 => CompressionFormat.Bc4,
                5 => CompressionFormat.Bc5,
                7 => CompressionFormat.Bc7,
                _ => throw new NotSupportedException(),
            };
            var isrc = 0;
            for (var y = 0; y < height; y += 4) {
                for (var x = 0; x < width; x += 4) {
                    decoder.DecodeBlock(source[isrc..(isrc + blockSize)], fmt, block);
                    isrc += blockSize;
                    var yn = Math.Min(4, height - y);
                    var xn = Math.Min(4, width - x);
                    for (var y1 = 0; y1 < yn; y1++) {
                        var offset = (y + y1) * targetStride + x * 4;
                        for (var x1 = 0; x1 < xn; x1++) {
                            target[offset++] = block[y1, x1].b;
                            target[offset++] = block[y1, x1].g;
                            target[offset++] = block[y1, x1].r;
                            target[offset++] = block[y1, x1].a;
                        }
                    }
                }
            }
        }
    }

    public bool Equals(BcPixelFormat? other) {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Type == other.Type && Version == other.Version && Alpha == other.Alpha;
    }

    public override bool Equals(object? obj) => Equals(obj as BcPixelFormat);

    public override int GetHashCode() => HashCode.Combine((int) Type, Version, (int) Alpha);
}
