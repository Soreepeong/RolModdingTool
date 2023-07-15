using System.Collections.Generic;
using System.Linq;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;
using SynergyLib.Util.BinaryRW;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Chunks;

public struct CompiledIntFacesChunk : ICryChunk {
    public ChunkHeader Header { get; set; }
    public readonly List<CompiledIntFace> Faces = new();

    public CompiledIntFacesChunk() { }

    public void ReadFrom(NativeReader reader, int expectedSize) {
        var expectedEnd = reader.BaseStream.Position + expectedSize;
        Header = new(reader);
        using (reader.ScopedBigEndian(Header.IsBigEndian)) {
            var count = (int) ((expectedEnd - reader.BaseStream.Position) / 6);
            Faces.Clear();
            Faces.EnsureCapacity(count);

            var vertex = new CompiledIntFace();
            for (var i = 0; i < count; i++) {
                vertex.ReadFrom(reader, 6);
                Faces.Add(vertex);
            }
        }

        reader.EnsurePositionOrThrow(expectedEnd);
    }

    public readonly void WriteTo(NativeWriter writer, bool useBigEndian) {
        Header.WriteTo(writer, false);
        using (writer.ScopedBigEndian(useBigEndian)) {
            foreach (var v in Faces)
                v.WriteTo(writer, useBigEndian);
        }
    }

    public int WrittenSize => Header.WrittenSize + Faces.Sum(x => x.WrittenSize);

    public override string ToString() => $"{nameof(CompiledIntFacesChunk)}: {Header}";
}
