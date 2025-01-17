﻿using System.Collections.Generic;
using System.Linq;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;
using SynergyLib.Util.BinaryRW;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Chunks;

public class CompiledIntSkinVerticesChunk : ICryChunk {
    public ChunkHeader Header { get; set; } = new();
    public List<IntSkinVertex> Vertices = new();

    public CompiledIntSkinVerticesChunk() { }

    public void ReadFrom(NativeReader reader, int expectedSize) {
        var expectedEnd = reader.BaseStream.Position + expectedSize;
        Header = new(reader);
        using (reader.ScopedBigEndian(Header.IsBigEndian)) {
            reader.EnsureZeroesOrThrow(32);

            var count = (int) ((expectedEnd - reader.BaseStream.Position) / 64);
            Vertices.Clear();
            Vertices.EnsureCapacity(count);

            var vertex = new IntSkinVertex();
            for (var i = 0; i < count; i++) {
                vertex.ReadFrom(reader, 64);
                Vertices.Add(vertex);
            }
        }

        reader.EnsurePositionOrThrow(expectedEnd);
    }

    public void WriteTo(NativeWriter writer, bool useBigEndian) {
        Header.WriteTo(writer, false);
        using (writer.ScopedBigEndian(useBigEndian)) {
            writer.FillZeroes(32);
            foreach (var v in Vertices)
                v.WriteTo(writer, useBigEndian);
        }
    }

    public int WrittenSize => Header.WrittenSize + 32 + Vertices.Sum(x => x.WrittenSize);

    public override string ToString() => $"{nameof(CompiledIntSkinVerticesChunk)}: {Header}";
}
