using System;
using System.Linq;
using SynergyLib.FileFormat.DirectDrawSurface.PixelFormats.Channels;

namespace SynergyLib.FileFormat.DirectDrawSurface.PixelFormats;

public class LumiPixelFormat : IPixelFormat, IEquatable<LumiPixelFormat> {
    public LumiPixelFormat(
        AlphaType alphaType,
        ChannelDefinition? l = null,
        ChannelDefinition? a = null,
        ChannelDefinition? x = null) {
        L = l ?? new();
        A = a ?? new();
        X = x ?? new();
        Alpha = alphaType;

        Bpp = new[] {L.Bits + L.Shift, A.Bits + A.Shift, X.Bits + X.Shift}.Max();
    }

    public ChannelDefinition L {get;}
    
    public ChannelDefinition A {get;}
    
    public ChannelDefinition X {get;}

    public AlphaType Alpha { get; }
    
    public int Bpp { get; }

    public void ToB8G8R8A8(Span<byte> target, int targetStride, ReadOnlySpan<byte> source, int sourceStride, int width,
        int height) {
        var bits = 0ul;
        var availBits = 0;
        var outOffset = 0;

        for (var y = 0; y < height; y++) {
            var inOffset = y * sourceStride;
            var inOffsetTo = inOffset + (width * Bpp + 7) / 8;
            
            for (var x = 0; x < width && inOffset < inOffsetTo; inOffset++) {
                bits = (bits << 8) | source[inOffset];
                availBits += 8;
                
                for (; availBits >= Bpp && x < width; x++, availBits -= Bpp) {
                    var l = (byte) L.DecodeValueAsUnorm(bits, 8);
                    var a = (byte) (A.Bits == 0 ? 255 : A.DecodeValueAsUnorm(bits, 8));
                    target[outOffset++] = a;
                    target[outOffset++] = l;
                    target[outOffset++] = l;
                    target[outOffset++] = l;
                }
            }
        }
    }

    public bool Equals(LumiPixelFormat? other) {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return L.Equals(other.L) && A.Equals(other.A) && X.Equals(other.X) && Alpha == other.Alpha;
    }

    public override bool Equals(object? obj) => Equals(obj as LumiPixelFormat);

    public override int GetHashCode() => HashCode.Combine(L, A, X, (int) Alpha);
}
