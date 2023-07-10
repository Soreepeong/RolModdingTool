using System;
using System.IO;
using System.Numerics;
using WiiUStreamTool.Util.BinaryRW;
using WiiUStreamTool.Util.MathExtras;

namespace WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Chunks;

public struct IntSkinVertex : ICryReadWrite {
    public Vector3 Position0;
    public Vector3 Position1;
    public Vector3 Position2;
    public ushort[] BoneIds;
    public float[] Weights;
    public Rgba32 Color;
    
    public void ReadFrom(NativeReader reader, int expectedSize) {
        if (expectedSize != 64)
            throw new IOException();
        Position0 = reader.ReadVector3();
        Position1 = reader.ReadVector3();
        Position2 = reader.ReadVector3();
        BoneIds = new[] {reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16(), reader.ReadUInt16()};
        Weights = new[] {reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()};
        Color.ReadFrom(reader, 4);
    }

    public void WriteTo(NativeWriter writer, bool useBigEndian) {
        throw new NotImplementedException();
    }
}