﻿using System.IO;
using System.Numerics;
using SynergyLib.Util.BinaryRW;
using SynergyLib.Util.MathExtras;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;

public struct IntSkinVertex : ICryReadWrite {
    public Vector3 Position0;
    public Vector3 Position1;
    public Vector3 Position2;
    public Vector4<ushort> BoneIds;
    public Vector4<float> Weights;
    public Rgba32 Color;

    public void ReadFrom(NativeReader reader, int expectedSize) {
        if (expectedSize != 64)
            throw new IOException();
        Position0 = reader.ReadVector3();
        Position1 = reader.ReadVector3();
        Position2 = reader.ReadVector3();
        BoneIds[0] = reader.ReadUInt16();
        BoneIds[1] = reader.ReadUInt16();
        BoneIds[2] = reader.ReadUInt16();
        BoneIds[3] = reader.ReadUInt16();
        Weights[0] = reader.ReadSingle();
        Weights[1] = reader.ReadSingle();
        Weights[2] = reader.ReadSingle();
        Weights[3] = reader.ReadSingle();

        Color.ReadFrom(reader, 4);
    }

    public readonly void WriteTo(NativeWriter writer, bool useBigEndian) {
        using (writer.ScopedBigEndian(useBigEndian)) {
            writer.Write(Position0);
            writer.Write(Position1);
            writer.Write(Position2);
            writer.Write(BoneIds[0]);
            writer.Write(BoneIds[1]);
            writer.Write(BoneIds[2]);
            writer.Write(BoneIds[3]);
            writer.Write(Weights[0]);
            writer.Write(Weights[1]);
            writer.Write(Weights[2]);
            writer.Write(Weights[3]);

            Color.WriteTo(writer, useBigEndian);
        }
    }

    public int WrittenSize => 64;
}
