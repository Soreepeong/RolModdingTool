using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using WiiUStreamTool.Util.BinaryRW;
using WiiUStreamTool.Util.MathExtras;

namespace WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Chunks;

public struct CompiledMorphTargetsChunk : ICryReadWrite {
    public ChunkHeader Header;
    public readonly List<CompiledMorphTarget> Targets = new();

    public CompiledMorphTargetsChunk() { }

    public void ReadFrom(NativeReader reader, int expectedSize) {
        var expectedEnd = reader.BaseStream.Position + expectedSize;
        Header.ReadFrom(reader, Unsafe.SizeOf<ChunkHeader>());
        using (reader.ScopedBigEndian(Header.IsBigEndian)) {
            reader.ReadInto(out int count);
            Targets.Clear();
            Targets.EnsureCapacity(count);

            var target = new CompiledMorphTarget();
            for (var i = 0; i < count; i++) {
                target.VertexId = reader.ReadUInt32();
                target.Vertex = reader.ReadVector3();
                Targets.Add(target);
            }
        }

        reader.EnsurePositionOrThrow(expectedEnd);
    }

    public void WriteTo(NativeWriter writer, bool useBigEndian) {
        throw new NotImplementedException();
    }

    public override string ToString() => $"{nameof(CompiledMorphTargetsChunk)}: {Header}";
}