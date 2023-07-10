using System.Collections.Generic;
using System.Linq;
using WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Structs;
using WiiUStreamTool.Util.BinaryRW;

namespace WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Chunks;

public struct CompiledIntFacesChunk : ICryChunk {
    public ChunkHeader Header { get; set; }
    public readonly List<CompiledIntFace> Vertices = new();

    public CompiledIntFacesChunk() { }

    public void ReadFrom(NativeReader reader, int expectedSize) {
        var expectedEnd = reader.BaseStream.Position + expectedSize;
        Header = new(reader);
        using (reader.ScopedBigEndian(Header.IsBigEndian)) {
            var count = (int) ((expectedEnd - reader.BaseStream.Position) / 6);
            Vertices.Clear();
            Vertices.EnsureCapacity(count);

            var vertex = new CompiledIntFace();
            for (var i = 0; i < count; i++) {
                vertex.ReadFrom(reader, 6);
                Vertices.Add(vertex);
            }
        }

        reader.EnsurePositionOrThrow(expectedEnd);
    }

    public readonly void WriteTo(NativeWriter writer, bool useBigEndian) {
        Header.WriteTo(writer, false);
        using (writer.ScopedBigEndian(useBigEndian)) {
            foreach (var v in Vertices)
                v.WriteTo(writer, useBigEndian);
        }
    }

    public int WrittenSize => Header.WrittenSize + Vertices.Sum(x => x.WrittenSize); 

    public override string ToString() => $"{nameof(CompiledIntFacesChunk)}: {Header}";
}