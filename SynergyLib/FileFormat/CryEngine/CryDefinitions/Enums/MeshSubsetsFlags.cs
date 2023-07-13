using System;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Enums;

[Flags]
public enum MeshSubsetsFlags {
    ShHasDecomprMat = 0x1,
    BoneIndices = 0x2,
}
