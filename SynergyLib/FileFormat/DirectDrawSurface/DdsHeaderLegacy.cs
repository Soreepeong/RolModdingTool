using System;
using System.Runtime.InteropServices;

namespace SynergyLib.FileFormat.DirectDrawSurface;

[StructLayout(LayoutKind.Sequential)]
public struct DdsHeaderLegacy {
    public const uint MagicValue = 0x20534444;

    public uint Magic;
    public DdsHeader Header;

    public readonly unsafe void WriteTo<T>(Span<T> target) where T : unmanaged {
        fixed (void* pSource = &this)
        fixed (void* pTarget = target)
            new Span<byte>(pSource, sizeof(DdsHeaderLegacy)).CopyTo(
                new(pTarget, target.Length * sizeof(T)));
    }
}