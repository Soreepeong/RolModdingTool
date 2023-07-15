using System;
using SynergyLib.Util.BinaryRW;
using SynergyLib.Util.MathExtras;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;

public struct MeshBoneMapping : ICryReadWrite {
    public Vector4<byte> BoneIds;
    public Vector4<byte> Weights;

    public void ReadFrom(NativeReader reader, int expectedSize) {
        if (expectedSize != 8)
            throw new ArgumentOutOfRangeException(nameof(expectedSize), expectedSize, null);

        BoneIds.X = reader.ReadByte();
        BoneIds.Y = reader.ReadByte();
        BoneIds.Z = reader.ReadByte();
        BoneIds.W = reader.ReadByte();
        Weights.X = reader.ReadByte();
        Weights.Y = reader.ReadByte();
        Weights.Z = reader.ReadByte();
        Weights.W = reader.ReadByte();
    }

    public void WriteTo(NativeWriter writer, bool useBigEndian) {
        writer.Write(BoneIds.X);
        writer.Write(BoneIds.Y);
        writer.Write(BoneIds.Z);
        writer.Write(BoneIds.W);
        writer.Write(Weights.X);
        writer.Write(Weights.Y);
        writer.Write(Weights.Z);
        writer.Write(Weights.W);
    }

    public int WrittenSize => 8;
}
