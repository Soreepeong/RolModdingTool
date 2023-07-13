using SynergyLib.Util.BinaryRW;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Chunks;

public struct ChunkSizeChunk : ICryChunk {
    public ChunkHeader Header { get; set; }
    public int Size;

    public void ReadFrom(NativeReader reader, int expectedSize) {
        var expectedEnd = reader.BaseStream.Position + expectedSize;
        using (reader.ScopedLittleEndian()) {
            Header = new(reader);
            reader.ReadInto(out Size);
        }

        reader.EnsurePositionOrThrow(expectedEnd);
    }

    public readonly void WriteTo(NativeWriter writer, bool useBigEndian) {
        Header.WriteTo(writer, useBigEndian);
        using (writer.ScopedLittleEndian())
            writer.Write(Size);
    }

    public readonly void WriteTo(NativeWriter writer) {
        Header.WriteTo(writer, false);
        using (writer.ScopedLittleEndian())
            writer.Write(Size);
    }

    public int WrittenSize => Header.WrittenSize + 4;

    public override string ToString() => $"Header: {Header}";
}
