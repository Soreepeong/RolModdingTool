using System;
using BCnEncoder.Shared;
using Microsoft.Toolkit.HighPerformance;
using SynergyLib.FileFormat.DirectDrawSurface.PixelFormats.Channels;
using SynergyLib.FileFormat.DotSquish;

namespace SynergyLib.FileFormat.DirectDrawSurface.PixelFormats;

public class BcPixelFormat : PixelFormat, IEquatable<BcPixelFormat> {
    public const int BlockSizeBc6 = 16;
    
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
        Bpp = version is 1 or 4 ? 4 : 8;
    }

    public int BlockSize => Version is 1 or 4 ? 8 : 16;

    public override unsafe void ToB8G8R8A8(
        Span<byte> target,
        int targetStride,
        ReadOnlySpan<byte> source,
        int sourceStride,
        int width,
        int height) {
        if (sourceStride * 2 != (width + 3) / 4 * 4 * Bpp)
            throw new ArgumentException("No padding is allowed for stride.", nameof(sourceStride));

        var blockSize = BlockSize;
        var decoder = new BCnEncoder.Decoder.BcDecoder();
        switch (Version) {
            case 6 when Type == ChannelType.Sf16:
                ToB8G8R8A8ForBc6S(target, targetStride, source, width, height);
                break;
            case 6 when Type == ChannelType.Uf16:
                ToB8G8R8A8ForBc6U(target, targetStride, source, width, height);
                break;
            case 1 or 2 or 3: {
                Squish.DecompressImage(
                    target,
                    targetStride,
                    width,
                    height,
                    source,
                    new() {
                        Method = Version switch {
                            1 => SquishMethod.Dxt1,
                            2 => SquishMethod.Dxt3,
                            3 => SquishMethod.Dxt5,
                            _ => throw new InvalidOperationException(),
                        }
                    });
                break;
            }
            default: {
                void* pBlock = stackalloc ColorRgba32[16];
                Span2D<ColorRgba32> block = new(pBlock, 4, 4, 0);

                var fmt = Version switch {
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

                break;
            }
        }
    }

    private static unsafe void ToB8G8R8A8ForBc6S(
        Span<byte> target,
        int targetStride,
        ReadOnlySpan<byte> source,
        int width,
        int height) {
        var decoder = new BCnEncoder.Decoder.BcDecoder();
        void* pBlock = stackalloc ColorRgbFloat[16];
        Span2D<ColorRgbFloat> block = new(pBlock, 4, 4, 0);

        for (var y = 0; y < height; y += 4) {
            for (var x = 0; x < width; x += 4) {
                decoder.DecodeBlockHdr(source[..BlockSizeBc6], CompressionFormat.Bc6S, block);
                source = source[BlockSizeBc6..];
                var yn = Math.Min(4, height - y);
                var xn = Math.Min(4, width - x);
                for (var y1 = 0; y1 < yn; y1++) {
                    var offset = (y + y1) * targetStride + x * 4;
                    for (var x1 = 0; x1 < xn; x1++) {
                        target[offset++] = (byte) MathF.Round(127.5f + 127.5f * block[y1, x1].b);
                        target[offset++] = (byte) MathF.Round(127.5f + 127.5f * block[y1, x1].g);
                        target[offset++] = (byte) MathF.Round(127.5f + 127.5f * block[y1, x1].r);
                        target[offset++] = 255;
                    }
                }
            }
        }
    }

    private static unsafe void ToB8G8R8A8ForBc6U(
        Span<byte> target,
        int targetStride,
        ReadOnlySpan<byte> source,
        int width,
        int height) {
        var decoder = new BCnEncoder.Decoder.BcDecoder();
        void* pBlock = stackalloc ColorRgbFloat[16];
        Span2D<ColorRgbFloat> block = new(pBlock, 4, 4, 0);

        for (var y = 0; y < height; y += 4) {
            for (var x = 0; x < width; x += 4) {
                decoder.DecodeBlockHdr(source[..BlockSizeBc6], CompressionFormat.Bc6U, block);
                source = source[BlockSizeBc6..];
                var yn = Math.Min(4, height - y);
                var xn = Math.Min(4, width - x);
                for (var y1 = 0; y1 < yn; y1++) {
                    var offset = (y + y1) * targetStride + x * 4;
                    for (var x1 = 0; x1 < xn; x1++) {
                        target[offset++] = (byte) MathF.Round(255 * block[y1, x1].b);
                        target[offset++] = (byte) MathF.Round(255 * block[y1, x1].g);
                        target[offset++] = (byte) MathF.Round(255 * block[y1, x1].r);
                        target[offset++] = 255;
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
