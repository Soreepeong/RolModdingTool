using System.Collections.Generic;
using SynergyLib.Util.BinaryRW;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Chunks;

public struct CompiledExtToIntMapChunk : ICryChunk {
    public ChunkHeader Header { get; set; }
    public readonly List<ushort> Map = new();

    public CompiledExtToIntMapChunk() { }

    public void ReadFrom(NativeReader reader, int expectedSize) {
        var expectedEnd = reader.BaseStream.Position + expectedSize;
        Header = new(reader);
        using (reader.ScopedBigEndian(Header.IsBigEndian)) {
            var count = (int) ((expectedEnd - reader.BaseStream.Position) / 2);
            Map.Clear();
            Map.EnsureCapacity(count);

            for (var i = 0; i < count; i++)
                Map.Add(reader.ReadUInt16());
        }

        reader.EnsurePositionOrThrow(expectedEnd);
    }

    public readonly void WriteTo(NativeWriter writer, bool useBigEndian) {
        Header.WriteTo(writer, false);
        using (writer.ScopedBigEndian(useBigEndian)) {
            foreach (var r in Map)
                writer.Write(r);
        }
    }

    public int WrittenSize => Header.WrittenSize + 2 * Map.Count;

    public override string ToString() => $"{nameof(CompiledExtToIntMapChunk)}: {Header}";
}
