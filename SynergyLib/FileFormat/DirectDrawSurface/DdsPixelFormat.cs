using System.Runtime.InteropServices;

namespace SynergyLib.FileFormat.DirectDrawSurface;

[StructLayout(LayoutKind.Sequential)]
public struct DdsPixelFormat {
    public int Size;

    public DdsPixelFormatFlags Flags;

    public DdsFourCc FourCc;
    public int RgbBitCount;
    public uint RBitMask;
    public uint GBitMask;
    public uint BBitMask;
    public uint ABitMask;
}