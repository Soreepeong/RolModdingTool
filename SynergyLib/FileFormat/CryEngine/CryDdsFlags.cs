using System;

namespace SynergyLib.FileFormat.CryEngine;

[Flags]
public enum CryDdsFlags : uint {
    CubeMap = 0x01,
    VolumeTexture = 0x02,
    Decal = 0x04,
    Greyscale = 0x08,
    SupressEngineReduce = 0x10,
    FileSingle = 0x40,
    Compressed = 0x200,
    AttachedAlpha = 0x400,
    SrgbRead = 0x800,
    XBox360Native = 0x1000,
    Ps3Native = 0x2000,
    X360NotPretiled = 0x4000,
    DontResize = 0x8000,
    RenormalizedTexture = 0x10000,
    CafeNative = 0x20000,
}