using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using WiiUStreamTool.Util.BinaryRW;

namespace WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Chunks;

public struct MtlNameChunk : ICryReadWrite {
    public ChunkHeader Header;
    public MtlNameFlags Flags;
    public uint Flags2;
    public string Name;
    public MtlNamePhysicsType PhysicsType;
    public int[] SubMaterialChunkIds;
    public int AdvancedDataChunkId;
    public float ShOpacity;

    public void ReadFrom(NativeReader reader, int expectedSize) {
        var expectedEnd = reader.BaseStream.Position + expectedSize;
        Header.ReadFrom(reader, Unsafe.SizeOf<ChunkHeader>());
        using (reader.ScopedBigEndian(Header.IsBigEndian)) {
            reader.ReadInto(out Flags);
            reader.ReadInto(out Flags2);
            Name = reader.ReadFString(128, Encoding.UTF8);
            reader.ReadInto(out PhysicsType);

            reader.ReadInto(out int childCount);
            if (childCount > 32)
                throw new InvalidDataException();
            SubMaterialChunkIds = new int[childCount];
            for (var i = 0; i < childCount; i++)
                reader.ReadInto(out SubMaterialChunkIds[i]);
            reader.BaseStream.Position += (32 - childCount) * 4;

            reader.ReadInto(out AdvancedDataChunkId);
            reader.ReadInto(out ShOpacity);
            reader.EnsureZeroesOrThrow(128);
        }

        reader.EnsurePositionOrThrow(expectedEnd);
    }

    public void WriteTo(NativeWriter writer, bool useBigEndian) {
        throw new NotImplementedException();
    }

    public override string ToString() => $"{nameof(MtlNameChunk)}: {Header}: {Name}";
}
