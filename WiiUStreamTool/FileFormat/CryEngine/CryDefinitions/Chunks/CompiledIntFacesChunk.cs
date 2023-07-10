using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using WiiUStreamTool.Util.BinaryRW;

namespace WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Chunks;

public struct CompiledIntFacesChunk : ICryReadWrite {
    public ChunkHeader Header;
    public readonly List<CompiledIntFace> Vertices = new();

    public CompiledIntFacesChunk() { }

    public void ReadFrom(NativeReader reader, int expectedSize) {
        var expectedEnd = reader.BaseStream.Position + expectedSize;
        Header.ReadFrom(reader, Unsafe.SizeOf<ChunkHeader>());
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

    public void WriteTo(NativeWriter writer, bool useBigEndian) {
        throw new NotImplementedException();
    }

    public override string ToString() => $"{nameof(CompiledIntFacesChunk)}: {Header}";
}