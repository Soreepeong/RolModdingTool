using System;
using WiiUStreamTool.Util.BinaryRW;

namespace WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Chunks;

public struct MeshPhysicsDataChunk : ICryChunk {
    public ChunkHeader Header { get; set; }
    public uint Flags;
    public int TetrahedraDataSize; // unused
    public int TetrahedraId; // unused

    public byte[] Data = Array.Empty<byte>();

    public MeshPhysicsDataChunk() { }

    public void ReadFrom(NativeReader reader, int expectedSize) {
        var expectedEnd = reader.BaseStream.Position + expectedSize;
        Header = new(reader);
        using (reader.ScopedBigEndian(Header.IsBigEndian)) {
            reader.ReadInto(out int dataSize);
            reader.ReadInto(out Flags);
            reader.ReadInto(out TetrahedraDataSize);
            reader.ReadInto(out TetrahedraId);
            reader.EnsureZeroesOrThrow(8);
            Data = reader.ReadBytes(dataSize);
        }

        reader.EnsurePositionOrThrow(expectedEnd);
    }

    public readonly void WriteTo(NativeWriter writer, bool useBigEndian) {
        Header.WriteTo(writer, false);
        using (writer.ScopedBigEndian(useBigEndian)) {
            writer.Write(Data.Length);
            writer.Write(Flags);
            writer.Write(TetrahedraDataSize);
            writer.Write(TetrahedraId);
            writer.FillZeroes(8);
            writer.Write(Data);
        }
    }

    public int WrittenSize => Header.WrittenSize + 24 + Data.Length;

    public override string ToString() => $"{nameof(MeshPhysicsDataChunk)}: {Header}";
}
