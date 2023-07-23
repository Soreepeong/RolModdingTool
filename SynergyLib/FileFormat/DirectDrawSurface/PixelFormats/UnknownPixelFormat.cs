using System;
using SynergyLib.FileFormat.DirectDrawSurface.PixelFormats.Channels;

namespace SynergyLib.FileFormat.DirectDrawSurface.PixelFormats;

public class UnknownPixelFormat : PixelFormat, IEquatable<UnknownPixelFormat> {
    public static readonly UnknownPixelFormat Instance = new();

    private UnknownPixelFormat() {
        Alpha = AlphaType.None;
        Bpp = 0;
    }

    public override DxgiFormat DxgiFormat => DxgiFormat.Unknown;
    public override DdsFourCc FourCc => DdsFourCc.Unknown;

    public override void ToB8G8R8A8(
        Span<byte> target,
        int targetStride,
        ReadOnlySpan<byte> source,
        int sourceStride,
        int width,
        int height) {
        throw new NotImplementedException();
    }

    public override bool Equals(object? obj) => ReferenceEquals(obj, this);

    public bool Equals(UnknownPixelFormat? other) => ReferenceEquals(other, this);

    public override int GetHashCode() => 0x4df85ea8;
}
