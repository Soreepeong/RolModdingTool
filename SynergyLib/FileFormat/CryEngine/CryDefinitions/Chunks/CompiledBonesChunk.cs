using System;
using System.Collections.Generic;
using System.Linq;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;
using SynergyLib.Util.BinaryRW;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Chunks;

public class CompiledBonesChunk : ICryChunk {
    public ChunkHeader Header { get; set; } = new();
    public List<CompiledBone> Bones = new();

    public CompiledBonesChunk() { }

    public void ReadFrom(NativeReader reader, int expectedSize) {
        var expectedEnd = reader.BaseStream.Position + expectedSize;
        Header = new(reader);
        using (reader.ScopedBigEndian(Header.IsBigEndian)) {
            reader.EnsureZeroesOrThrow(32);

            var boneCount = 1;
            Bones.Clear();
            for (var i = 0; i < boneCount; i++) {
                var bone = new CompiledBone();
                bone.ReadFrom(reader, 584);
                boneCount = Math.Max(boneCount, i + bone.ChildOffset + bone.ChildCount);
                Bones.Add(bone);
            }
        }

        reader.EnsurePositionOrThrow(expectedEnd);
    }

    public void WriteTo(NativeWriter writer, bool useBigEndian) {
        Header.WriteTo(writer, false);
        using (writer.ScopedBigEndian(useBigEndian)) {
            writer.FillZeroes(32);
            foreach (var b in Bones)
                b.WriteTo(writer, useBigEndian);
        }
    }

    public int WrittenSize => Header.WrittenSize + 32 + Bones.Sum(x => x.WrittenSize);

    public override string ToString() => $"{nameof(CompiledBonesChunk)}: {Header}";
}
