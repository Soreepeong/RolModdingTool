using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SynergyLib.FileFormat.DirectDrawSurface;

public static class DdsFileExtensions {
    public static unsafe Image<Bgra32> ToImageBgra32(
        this DdsFile dds,
        int imageIndex,
        int mipmapIndex,
        int sliceIndex) {
        dds.SliceOrFaceData(imageIndex, mipmapIndex, sliceIndex);

        var output = new Image<Bgra32>(
            new() {PreferContiguousImageBuffers = true},
            dds.Width(mipmapIndex),
            dds.Height(mipmapIndex));
        if (!output.DangerousTryGetSinglePixelMemory(out var memory))
            throw new();
        fixed (void* p = memory.Span)
            dds.PixelFormat.ToB8G8R8A8(
                p,
                memory.Length * 4,
                output.Width * 4,
                dds.SliceOrFaceData(imageIndex, mipmapIndex, sliceIndex),
                dds.Pitch(mipmapIndex),
                output.Width,
                output.Height
            );
        return output;
    }
}
