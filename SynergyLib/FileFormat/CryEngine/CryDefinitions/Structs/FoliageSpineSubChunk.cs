using System;
using System.Numerics;
using SynergyLib.Util.BinaryRW;
using SynergyLib.Util.MathExtras;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;

public struct FoliageSpineSubChunk : ICryReadWrite {
    public byte VertexCount;
    public byte Trash1;
    public byte Trash2;
    public byte Trash3;
    public float Length;
    public Vector3 Navigation;
    public byte AttachSpine;
    public byte AttachSegment;
    public byte Trash4;
    public byte Trash5;

    public void ReadFrom(NativeReader reader, int expectedSize) {
        if (expectedSize != 24)
            throw new ArgumentOutOfRangeException(nameof(expectedSize), expectedSize, null);

        reader.ReadInto(out VertexCount);
        reader.ReadInto(out Trash1);
        reader.ReadInto(out Trash2);
        reader.ReadInto(out Trash3);
        reader.ReadInto(out Length);
        Navigation = reader.ReadVector3();
        reader.ReadInto(out AttachSpine);
        reader.ReadInto(out AttachSegment);
        reader.ReadInto(out Trash4);
        reader.ReadInto(out Trash5);
    }

    public void WriteTo(NativeWriter writer, bool useBigEndian) {
        using (writer.ScopedBigEndian(useBigEndian)) {
            writer.Write(VertexCount);
            writer.Write(Trash1);
            writer.Write(Trash2);
            writer.Write(Trash3);
            writer.Write(Length);
            writer.Write(Navigation);
            writer.Write(AttachSpine);
            writer.Write(AttachSegment);
            writer.Write(Trash4);
            writer.Write(Trash5);
        }
    }

    public int WrittenSize => 24;
}
