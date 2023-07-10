using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using WiiUStreamTool.Util.BinaryRW;

namespace WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Chunks;

public struct MeshSubsetsChunk : ICryReadWrite {
    public ChunkHeader Header;
    public MeshSubsetsFlags Flags;
    public List<MeshSubset> Subsets = new();
    public List<ushort[]> BoneIds = new();

    public MeshSubsetsChunk() { }

    public void ReadFrom(NativeReader reader, int expectedSize) {
        var expectedEnd = reader.BaseStream.Position + expectedSize;
        Header.ReadFrom(reader, Unsafe.SizeOf<ChunkHeader>());
        using (reader.ScopedBigEndian(Header.IsBigEndian)) {
            reader.ReadInto(out Flags);
            var count = reader.ReadInt32();
            reader.EnsureZeroesOrThrow(8);

            Subsets.Clear();
            BoneIds.Clear();
            Subsets.EnsureCapacity(count);
            BoneIds.EnsureCapacity(count);
            for (var i = 0; i < count; i++) {
                var v = new MeshSubset();
                v.ReadFrom(reader, 36);
                Subsets.Add(v);
            }

            for (var i = 0; i < count; i++) {
                reader.ReadInto(out int count2);
                if (count2 > 0x80)
                    throw new InvalidDataException();
                var ids = new ushort[count2];
                for (var j = 0; j < ids.Length; j++)
                    ids[j] = reader.ReadUInt16();
                reader.BaseStream.Position += (0x80 - ids.Length) * 2;
                BoneIds.Add(ids);
            }
        }

        reader.EnsurePositionOrThrow(expectedEnd);
    }

    public void WriteTo(NativeWriter writer, bool useBigEndian) {
        throw new NotImplementedException();
    }

    public override string ToString() => $"{nameof(MeshSubsetsChunk)}: {Header}";
}
