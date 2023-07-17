using System;
using SynergyLib.FileFormat.DirectDrawSurface.PixelFormats.Channels;

namespace SynergyLib.FileFormat.DirectDrawSurface.PixelFormats;

public interface IPixelFormat {
    public AlphaType Alpha { get; }
    public int Bpp { get; }
    public DxgiFormat DxgiFormat => PixelFormatResolver.GetDxgiFormat(this);
    public DdsFourCc FourCc => PixelFormatResolver.GetFourCc(this);

    public void ToB8G8R8A8(
        Span<byte> target,
        int targetStride,
        ReadOnlySpan<byte> source,
        int sourceStride,
        int width,
        int height);

    public unsafe void ToB8G8R8A8(
        void* targetAddress,
        int targetSize,
        int targetStride,
        ReadOnlySpan<byte> source,
        int sourceStride,
        int width,
        int height) {
        ToB8G8R8A8(new(targetAddress, targetSize), targetStride, source, sourceStride, width, height);
    }
}
