using System;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Enums;

[Flags]
public enum MtlNameFlags : uint {
    MultiMaterial = 0x01,
    SubMaterial = 0x02,
    ShCoeff = 0x04,
    ShDoubleSided = 0x08,
    ShAmbient = 0x10,
}
