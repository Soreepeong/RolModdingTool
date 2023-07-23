using System;
using System.Linq;
using SynergyLib.FileFormat.DirectDrawSurface.PixelFormats.Channels;

namespace SynergyLib.FileFormat.DirectDrawSurface.PixelFormats;

public class RgbaPixelFormat : PixelFormat, IEquatable<RgbaPixelFormat> {
    public RgbaPixelFormat(
        AlphaType alphaType,
        ChannelDefinition? r = null,
        ChannelDefinition? g = null,
        ChannelDefinition? b = null,
        ChannelDefinition? a = null,
        ChannelDefinition? x1 = null,
        ChannelDefinition? x2 = null) {
        Alpha = alphaType;
        R = r ?? new();
        G = g ?? new();
        B = b ?? new();
        A = a ?? new();
        X1 = x1 ?? new();
        X2 = x2 ?? new();
        Bpp = new[] {
            R.Bits + R.Shift,
            G.Bits + G.Shift,
            B.Bits + B.Shift,
            A.Bits + A.Shift,
            X1.Bits + X1.Shift,
            X2.Bits + X2.Shift,
        }.Max();
    }

    public ChannelDefinition R { get; }
    public ChannelDefinition G { get; }
    public ChannelDefinition B { get; }
    public ChannelDefinition A { get; }
    public ChannelDefinition X1 { get; }
    public ChannelDefinition X2 { get; }

    public override void ToB8G8R8A8(
        Span<byte> target,
        int targetStride,
        ReadOnlySpan<byte> source,
        int sourceStride,
        int width,
        int height) {
        var bits = 0ul;
        var availBits = 0;

        for (var y = 0; y < height; y++) {
            var inOffset = y * sourceStride;
            var inOffsetTo = inOffset + (width * Bpp + 7) / 8;
            var outOffset = y * targetStride;

            for (var x = 0; x < width && inOffset < inOffsetTo; inOffset++) {
                bits = (bits << 8) | source[inOffset];
                availBits += 8;
                for (; availBits >= Bpp && x < width; x++, availBits -= Bpp) {
                    target[outOffset++] = (byte) (A.Bits == 0 ? 255 : A.DecodeValueAsUnorm(bits, 8));
                    target[outOffset++] = (byte) R.DecodeValueAsUnorm(bits, 8);
                    target[outOffset++] = (byte) G.DecodeValueAsUnorm(bits, 8);
                    target[outOffset++] = (byte) B.DecodeValueAsUnorm(bits, 8);
                }
            }
        }
    }

    // If colors are wrong, then it means that I got orders wrong, and it needs to be modified.

    public static RgbaPixelFormat NewR(
        int rbits,
        int xbits1 = 0,
        int xbits2 = 0,
        ChannelType channelType = ChannelType.Unorm,
        AlphaType alphaType = AlphaType.Straight) => new(
        alphaType: alphaType,
        r: new(channelType, 0, rbits),
        x1: new(ChannelType.Typeless, rbits, xbits1),
        x2: new(ChannelType.Typeless, rbits + xbits1, xbits2));

    public static RgbaPixelFormat NewA(
        int abits,
        int xbits1 = 0,
        int xbits2 = 0,
        ChannelType channelType = ChannelType.Unorm,
        AlphaType alphaType = AlphaType.Straight) => new(
        alphaType: alphaType,
        a: new(channelType, 0, abits),
        x1: new(ChannelType.Typeless, abits, xbits1),
        x2: new(ChannelType.Typeless, abits + xbits1, xbits2));

    public static RgbaPixelFormat NewRg(
        int rbits,
        int gbits,
        int xbits1 = 0,
        int xbits2 = 0,
        ChannelType channelType = ChannelType.Unorm,
        AlphaType alphaType = AlphaType.Straight) => new(
        alphaType: alphaType,
        r: new(channelType, 0, rbits),
        g: new(channelType, rbits, gbits),
        x1: new(ChannelType.Typeless, rbits + gbits, xbits1),
        x2: new(ChannelType.Typeless, rbits + gbits + xbits1, xbits2));

    public static RgbaPixelFormat NewRgb(
        int rbits,
        int gbits,
        int bbits,
        int xbits1 = 0,
        int xbits2 = 0,
        ChannelType channelType = ChannelType.Unorm,
        AlphaType alphaType = AlphaType.Straight) => new(
        alphaType: alphaType,
        r: new(channelType, 0, rbits),
        g: new(channelType, rbits, gbits),
        b: new(channelType, rbits + gbits, bbits),
        x1: new(ChannelType.Typeless, rbits + gbits + bbits, xbits1),
        x2: new(ChannelType.Typeless, rbits + gbits + bbits + xbits1, xbits2));

    public static RgbaPixelFormat NewRgba(
        int rbits,
        int gbits,
        int bbits,
        int abits,
        int xbits1 = 0,
        int xbits2 = 0,
        ChannelType channelType = ChannelType.Unorm,
        AlphaType alphaType = AlphaType.Straight) =>
        new(
            alphaType: alphaType,
            r: new(channelType, 0, rbits),
            g: new(channelType, rbits, gbits),
            b: new(channelType, rbits + gbits, bbits),
            a: new(channelType, rbits + bbits + bbits, abits),
            x1: new(ChannelType.Typeless, rbits + bbits + bbits + abits, xbits1),
            x2: new(ChannelType.Typeless, rbits + bbits + bbits + abits + xbits1, xbits2));

    public static RgbaPixelFormat NewBgr(
        int rbits,
        int gbits,
        int bbits,
        int xbits1 = 0,
        int xbits2 = 0,
        ChannelType channelType = ChannelType.Unorm,
        AlphaType alphaType = AlphaType.Straight) => new(
        alphaType: alphaType,
        b: new(channelType, 0, bbits),
        g: new(channelType, bbits, gbits),
        r: new(channelType, bbits + gbits, rbits),
        x1: new(ChannelType.Typeless, bbits + gbits + rbits, xbits1),
        x2: new(ChannelType.Typeless, bbits + gbits + rbits + xbits1, xbits2));

    public static RgbaPixelFormat NewBgra(
        int rbits,
        int gbits,
        int bbits,
        int abits,
        int xbits1 = 0,
        int xbits2 = 0,
        ChannelType channelType = ChannelType.Unorm,
        AlphaType alphaType = AlphaType.Straight) =>
        new(
            alphaType: alphaType,
            b: new(channelType, 0, bbits),
            g: new(channelType, bbits, gbits),
            r: new(channelType, bbits + gbits, rbits),
            a: new(channelType, bbits + gbits + rbits, abits, (1u << abits) - 1u),
            x1: new(ChannelType.Typeless, bbits + gbits + rbits + abits, xbits1),
            x2: new(ChannelType.Typeless, bbits + gbits + rbits + abits + xbits1, xbits2));

    public bool Equals(RgbaPixelFormat? other) {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return R.Equals(other.R) && G.Equals(other.G) && B.Equals(other.B) && A.Equals(other.A) &&
            X1.Equals(other.X1) && X2.Equals(other.X2) && Alpha == other.Alpha;
    }

    public override bool Equals(object? obj) => Equals(obj as RgbaPixelFormat);

    public override int GetHashCode() => HashCode.Combine(R, G, B, A, X1, X2, (int) Alpha);
}
