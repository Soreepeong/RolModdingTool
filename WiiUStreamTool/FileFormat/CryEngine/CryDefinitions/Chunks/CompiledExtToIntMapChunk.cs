using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using WiiUStreamTool.Util.BinaryRW;

namespace WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Chunks;

public struct CompiledExtToIntMapChunk : ICryReadWrite {
    public ChunkHeader Header;
    public readonly List<ushort> Map = new();

    public CompiledExtToIntMapChunk() { }

    public void ReadFrom(NativeReader reader, int expectedSize) {
        var expectedEnd = reader.BaseStream.Position + expectedSize;
        Header.ReadFrom(reader, Unsafe.SizeOf<ChunkHeader>());
        using (reader.ScopedBigEndian(Header.IsBigEndian)) {
            var count = (int) ((expectedEnd - reader.BaseStream.Position) / 2);
            Map.Clear();
            Map.EnsureCapacity(count);

            for (var i = 0; i < count; i++)
                Map.Add(reader.ReadUInt16());
        }

        reader.EnsurePositionOrThrow(expectedEnd);
    }

    public void WriteTo(NativeWriter writer, bool useBigEndian) {
        throw new NotImplementedException();
    }

    public override string ToString() => $"{nameof(CompiledExtToIntMapChunk)}: {Header}";
}