using System.Collections.Generic;
using SynergyLib.Util.BinaryRW;
using SynergyLib.Util.MathExtras;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Chunks;

public struct BonesBoxesChunk : ICryChunk {
    public ChunkHeader Header { get; set; }
    public uint BoneId;
    public AaBb AaBb;
    public readonly List<ushort> Indices = new();

    public BonesBoxesChunk() { }

    public void ReadFrom(NativeReader reader, int expectedSize) {
        var expectedEnd = reader.BaseStream.Position + expectedSize;
        Header = new(reader);
        using (reader.ScopedBigEndian(Header.IsBigEndian)) {
            reader.ReadInto(out BoneId);
            AaBb = reader.ReadAaBb();
            var count = reader.ReadInt32();
            Indices.Clear();
            Indices.EnsureCapacity(count);
            for (var i = 0; i < count; i++)
                Indices.Add(reader.ReadUInt16());
        }

        reader.EnsurePositionOrThrow(expectedEnd);
    }

    public readonly void WriteTo(NativeWriter writer, bool useBigEndian) {
        Header.WriteTo(writer, false);
        using (writer.ScopedBigEndian(useBigEndian)) {
            writer.Write(BoneId);
            writer.Write(AaBb);
            writer.Write(Indices.Count);
            foreach (var x in Indices)
                writer.Write(x);
        }
    }

    public int WrittenSize => Header.WrittenSize + 32 + Indices.Count * 2;

    public override string ToString() => $"{nameof(BonesBoxesChunk)}: {Header}";
}
