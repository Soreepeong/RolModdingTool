using System;
using System.Collections.Generic;
using System.Linq;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;
using SynergyLib.Util.BinaryRW;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Chunks;

public class CompiledPhysicalBonesChunk : ICryChunk {
    public ChunkHeader Header { get; set; } = new();
    public List<BoneEntity> Bones = new();

    public CompiledPhysicalBonesChunk() { }

    public void ReadFrom(NativeReader reader, int expectedSize) {
        var expectedEnd = reader.BaseStream.Position + expectedSize;
        Header = new(reader);
        using (reader.ScopedBigEndian(Header.IsBigEndian)) {
            reader.EnsureZeroesOrThrow(32);

            var boneCount = (int) ((expectedEnd - reader.BaseStream.Position) / 152);
            Bones.EnsureCapacity(boneCount);
            Bones.Clear();
            var bone = new BoneEntity();
            for (var i = 0; i < boneCount; i++) {
                bone.ReadFrom(reader, 152);
                boneCount = Math.Max(boneCount, i + bone.ChildCount);
                Bones.Add(bone);
            }
        }

        reader.EnsurePositionOrThrow(expectedEnd);
    }

    public void WriteTo(NativeWriter writer, bool useBigEndian) {
        Header.WriteTo(writer, false);
        using (writer.ScopedBigEndian(useBigEndian)) {
            writer.FillZeroes(32);
            foreach (var bone in Bones)
                bone.WriteTo(writer, useBigEndian);
        }
    }

    public int WrittenSize => Header.WrittenSize + 32 + Bones.Sum(x => x.WrittenSize);

    public override string ToString() => $"{nameof(CompiledPhysicalBonesChunk)}: {Header}";
}
