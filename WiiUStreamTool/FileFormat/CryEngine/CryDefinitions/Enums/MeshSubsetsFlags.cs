using System;

namespace WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Chunks;

[Flags]
public enum MeshSubsetsFlags {
    ShHasDecomprMat = 0x1,
    BoneIndices = 0x2,
}