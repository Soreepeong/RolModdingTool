using System.IO;
using System.Text;
using WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Enums;
using WiiUStreamTool.Util.BinaryRW;

namespace WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Chunks;

public struct MtlNameChunk : ICryChunk {
    public ChunkHeader Header { get; set; }
    public MtlNameFlags Flags;
    public uint Flags2;
    public string Name;
    public MtlNamePhysicsType PhysicsType;
    public int[] SubMaterialChunkIds;
    public int AdvancedDataChunkId;
    public float ShOpacity;

    public void ReadFrom(NativeReader reader, int expectedSize) {
        var expectedEnd = reader.BaseStream.Position + expectedSize;
        Header = new(reader);
        using (reader.ScopedBigEndian(Header.IsBigEndian)) {
            reader.ReadInto(out Flags);
            reader.ReadInto(out Flags2);
            Name = reader.ReadFString(128, Encoding.UTF8);
            reader.ReadInto(out PhysicsType);

            reader.ReadInto(out int childCount);
            if (childCount > 32)
                throw new InvalidDataException();
            SubMaterialChunkIds = new int[childCount];
            for (var i = 0; i < childCount; i++)
                reader.ReadInto(out SubMaterialChunkIds[i]);
            reader.BaseStream.Position += (32 - childCount) * 4;

            reader.ReadInto(out AdvancedDataChunkId);
            reader.ReadInto(out ShOpacity);
            reader.EnsureZeroesOrThrow(128);
        }

        reader.EnsurePositionOrThrow(expectedEnd);
    }

    public readonly void WriteTo(NativeWriter writer, bool useBigEndian) {
        Header.WriteTo(writer, false);
        using (writer.ScopedBigEndian(useBigEndian)) {
            writer.WriteEnum(Flags);
            writer.Write(Flags2);
            writer.WriteFString(Name, 128, Encoding.UTF8);
            writer.WriteEnum(PhysicsType);

            if (SubMaterialChunkIds.Length > 32)
                throw new InvalidDataException();

            writer.Write(SubMaterialChunkIds.Length);
            foreach (var t in SubMaterialChunkIds)
                writer.Write(t);
            for (var i = SubMaterialChunkIds.Length; i < 32; i++)
                writer.Write(0);

            writer.Write(AdvancedDataChunkId);
            writer.Write(ShOpacity);
            writer.FillZeroes(128);
        }
    }

    public int WrittenSize => Header.WrittenSize + 408;

    public override string ToString() => $"{nameof(MtlNameChunk)}: {Header}: {Name}";
}
