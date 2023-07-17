using System.Runtime.InteropServices;

namespace SynergyLib.FileFormat.DirectDrawSurface;

[StructLayout(LayoutKind.Sequential)]
public struct DdsHeaderLegacy {
    public const uint MagicValue = 0x20534444;

    public uint Magic;
    public DdsHeader Header;
}