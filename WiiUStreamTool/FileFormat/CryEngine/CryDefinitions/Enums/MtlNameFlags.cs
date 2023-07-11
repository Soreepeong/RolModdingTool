using System;

namespace WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Enums;

[Flags]
public enum MtlNameFlags : uint {
    MultiMaterial = 0x01,
    SubMaterial = 0x02,
    ShCoeff = 0x04,
    DoubleSided = 0x08,
    Ambient = 0x10,
}
