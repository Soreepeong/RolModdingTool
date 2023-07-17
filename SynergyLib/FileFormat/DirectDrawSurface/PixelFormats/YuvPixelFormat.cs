using System;
using System.Linq;
using SynergyLib.FileFormat.DirectDrawSurface.PixelFormats.Channels;

namespace SynergyLib.FileFormat.DirectDrawSurface.PixelFormats;

public class YuvPixelFormat : IPixelFormat, IEquatable<YuvPixelFormat> {
    public readonly ChannelDefinition Y;
    public readonly ChannelDefinition U;
    public readonly ChannelDefinition V;
    public readonly ChannelDefinition A;
    public readonly ChannelDefinition X;

    public YuvPixelFormat(
        AlphaType alphaType,
        ChannelDefinition? y = null,
        ChannelDefinition? u = null,
        ChannelDefinition? v = null,
        ChannelDefinition? a = null,
        ChannelDefinition? x = null) {
        Alpha = alphaType;
        Y = y ?? new();
        U = u ?? new();
        V = v ?? new();
        A = a ?? new();
        X = x ?? new();
        Bpp = new[] {
            Y.Bits + Y.Shift,
            U.Bits + U.Shift,
            V.Bits + V.Shift,
            A.Bits + A.Shift,
            X.Bits + X.Shift,
        }.Max();
    }

    public AlphaType Alpha { get; }

    public int Bpp { get; }

    public void ToB8G8R8A8(Span<byte> target, int targetStride, ReadOnlySpan<byte> source, int sourceStride, int width,
        int height) {
        throw new NotImplementedException();
    }

    public bool Equals(YuvPixelFormat? other) {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Y.Equals(other.Y) && U.Equals(other.U) && V.Equals(other.V) && A.Equals(other.A) && X.Equals(other.X) &&
            Alpha == other.Alpha;
    }

    public override bool Equals(object? obj) => Equals(obj as YuvPixelFormat);

    public override int GetHashCode() => HashCode.Combine(Y, U, V, A, X, (int) Alpha);
}
