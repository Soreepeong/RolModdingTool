using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using WiiUStreamTool.Util.BinaryRW;
using WiiUStreamTool.Util.MathExtras;

namespace WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Chunks;

public struct BonesBoxesChunk : ICryReadWrite {
    public ChunkHeader Header;
    public uint BoneId;
    public AaBb AaBb;
    public List<ushort> Indices = new();
    
    public BonesBoxesChunk() { }

    public void ReadFrom(NativeReader reader, int expectedSize) {
        var expectedEnd = reader.BaseStream.Position + expectedSize;
        Header.ReadFrom(reader, Unsafe.SizeOf<ChunkHeader>());
        using (reader.ScopedBigEndian(Header.IsBigEndian)) {
            reader.ReadInto(out BoneId);
            AaBb = reader.ReadAaBb();
            var count = reader.ReadInt32();
            Indices.Clear();
            Indices.EnsureCapacity(count);
            for (var i = 0; i < count; i++)
                Indices.Add(reader.ReadUInt16());
        }

        reader.EnsurePositionOrThrow(expectedEnd);
    }

    public void WriteTo(NativeWriter writer, bool useBigEndian) {
        throw new NotImplementedException();
    }

    public override string ToString() => $"{nameof(BonesBoxesChunk)}: {Header}";
}