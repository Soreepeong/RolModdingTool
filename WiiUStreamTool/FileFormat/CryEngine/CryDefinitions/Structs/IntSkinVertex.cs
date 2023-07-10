using System.IO;
using System.Numerics;
using WiiUStreamTool.Util.BinaryRW;
using WiiUStreamTool.Util.MathExtras;

namespace WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Structs;

public struct IntSkinVertex : ICryReadWrite {
    public Vector3 Position0;
    public Vector3 Position1;
    public Vector3 Position2;
    public unsafe fixed ushort BoneIds[4];
    public unsafe fixed float Weights[4];
    public Rgba32 Color;

    public void ReadFrom(NativeReader reader, int expectedSize) {
        if (expectedSize != 64)
            throw new IOException();
        Position0 = reader.ReadVector3();
        Position1 = reader.ReadVector3();
        Position2 = reader.ReadVector3();
        unsafe {
            reader.ReadInto(out BoneIds[0]);
            reader.ReadInto(out BoneIds[1]);
            reader.ReadInto(out BoneIds[2]);
            reader.ReadInto(out BoneIds[3]);
            reader.ReadInto(out Weights[0]);
            reader.ReadInto(out Weights[1]);
            reader.ReadInto(out Weights[2]);
            reader.ReadInto(out Weights[3]);
        }

        Color.ReadFrom(reader, 4);
    }

    public readonly void WriteTo(NativeWriter writer, bool useBigEndian) {
        using (writer.ScopedBigEndian(useBigEndian)) {
            writer.Write(Position0);
            writer.Write(Position1);
            writer.Write(Position2);
            unsafe {
                writer.Write(BoneIds[0]);
                writer.Write(BoneIds[1]);
                writer.Write(BoneIds[2]);
                writer.Write(BoneIds[3]);
                writer.Write(Weights[0]);
                writer.Write(Weights[1]);
                writer.Write(Weights[2]);
                writer.Write(Weights[3]);
            }

            Color.WriteTo(writer, useBigEndian);
        }
    }

    public int WrittenSize => 64;
}
