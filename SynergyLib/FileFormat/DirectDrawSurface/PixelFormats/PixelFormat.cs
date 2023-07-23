using System;
using SynergyLib.FileFormat.DirectDrawSurface.PixelFormats.Channels;

namespace SynergyLib.FileFormat.DirectDrawSurface.PixelFormats;

public abstract class PixelFormat {
    public AlphaType Alpha { get; protected init; }
    public int Bpp { get; protected init; }
    public virtual DxgiFormat DxgiFormat => PixelFormatResolver.GetDxgiFormat(this);
    public virtual DdsFourCc FourCc => PixelFormatResolver.GetFourCc(this);

    public abstract void ToB8G8R8A8(
        Span<byte> target,
        int targetStride,
        ReadOnlySpan<byte> source,
        int sourceStride,
        int width,
        int height);

    public void ToB8G8R8A8<TTarget, TSource>(
        Span<TTarget> target,
        int targetStride,
        ReadOnlySpan<TSource> source,
        int sourceStride,
        int width,
        int height)
        where TTarget : unmanaged
        where TSource : unmanaged {
        unsafe {
            fixed (void* pTarget = target)
            fixed (void* pSource = source) {
                ToB8G8R8A8(
                    new(pTarget, target.Length * sizeof(TTarget)),
                    targetStride * sizeof(TTarget),
                    new(pSource, source.Length * sizeof(TSource)),
                    sourceStride * sizeof(TSource),
                    width,
                    height);
            }
        }
    }
}
