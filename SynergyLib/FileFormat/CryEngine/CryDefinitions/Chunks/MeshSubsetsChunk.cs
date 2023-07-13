using System.Collections.Generic;
using System.IO;
using System.Linq;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Enums;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;
using SynergyLib.Util.BinaryRW;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Chunks;

public struct MeshSubsetsChunk : ICryChunk {
    public ChunkHeader Header { get; set; }
    public MeshSubsetsFlags Flags;
    public List<MeshSubset> Subsets = new();
    public List<ushort[]> BoneIds = new();

    public MeshSubsetsChunk() { }

    public void ReadFrom(NativeReader reader, int expectedSize) {
        var expectedEnd = reader.BaseStream.Position + expectedSize;
        Header = new(reader);
        using (reader.ScopedBigEndian(Header.IsBigEndian)) {
            reader.ReadInto(out Flags);
            var count = reader.ReadInt32();
            reader.EnsureZeroesOrThrow(8);

            Subsets.Clear();
            Subsets.EnsureCapacity(count);
            for (var i = 0; i < count; i++) {
                var v = new MeshSubset();
                v.ReadFrom(reader, 36);
                Subsets.Add(v);
            }

            BoneIds.Clear();
            if (Flags.HasFlag(MeshSubsetsFlags.BoneIndices)) {
                BoneIds.EnsureCapacity(count);
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
        }

        reader.EnsurePositionOrThrow(expectedEnd);
    }

    public readonly void WriteTo(NativeWriter writer, bool useBigEndian) {
        Header.WriteTo(writer, false);
        using (writer.ScopedBigEndian(useBigEndian)) {
            if (Flags.HasFlag(MeshSubsetsFlags.BoneIndices)) {
                if (BoneIds.Count == 0)
                    throw new InvalidDataException("Flags has BoneIndices but BoneIds.Count == 0");
                if (BoneIds.Count != Subsets.Count)
                    throw new InvalidDataException("BoneIds.Count != Subsets.Count");
            }

            if (!Flags.HasFlag(MeshSubsetsFlags.BoneIndices) && BoneIds.Count != 0)
                throw new InvalidDataException("Flags does not have BoneIndices but BoneIds.Count != 0");

            writer.WriteEnum(Flags);
            writer.Write(Subsets.Count);
            writer.FillZeroes(8);
            foreach (var b in Subsets)
                b.WriteTo(writer, useBigEndian);
            if (Flags.HasFlag(MeshSubsetsFlags.BoneIndices)) {
                foreach (var b in BoneIds) {
                    if (b.Length > 128)
                        throw new InvalidDataException("BoneIds[..].Length > 0x80");
                    writer.Write(b.Length);
                    foreach (var c in b)
                        writer.Write(c);
                    for (var i = b.Length; i < 128; i++)
                        writer.Write((short) 0);
                }
            }
        }
    }

    public int WrittenSize => Header.WrittenSize + 16 + Subsets.Sum(x => x.WrittenSize) + BoneIds.Count * 260;

    public override string ToString() => $"{nameof(MeshSubsetsChunk)}: {Header}";
}
