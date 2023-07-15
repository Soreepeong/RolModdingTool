using System;
using System.Numerics;
using SynergyLib.Util.BinaryRW;
using SynergyLib.Util.MathExtras;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;

public struct FoliageSpineSubChunk : ICryReadWrite {
    public byte VertexCount;
    public float Length;
    public Vector3 Navigation;
    public byte AttachSpine;
    public byte AttachSegment;

    public void ReadFrom(NativeReader reader, int expectedSize) {
        if (expectedSize != 24)
            throw new ArgumentOutOfRangeException(nameof(expectedSize), expectedSize, null);

        reader.ReadInto(out VertexCount);
        reader.EnsureZeroesOrThrow(3);
        reader.ReadInto(out Length);
        Navigation = reader.ReadVector3();
        reader.ReadInto(out AttachSpine);
        reader.ReadInto(out AttachSegment);
        reader.EnsureZeroesOrThrow(2);
    }

    public void WriteTo(NativeWriter writer, bool useBigEndian) {
        using (writer.ScopedBigEndian(useBigEndian)) {
            writer.Write(VertexCount);
            writer.FillZeroes(3);
            writer.Write(Length);
            writer.Write(Navigation);
            writer.Write(AttachSpine);
            writer.Write(AttachSegment);
            writer.FillZeroes(2);
        }
    }

    public int WrittenSize => 24;
}
