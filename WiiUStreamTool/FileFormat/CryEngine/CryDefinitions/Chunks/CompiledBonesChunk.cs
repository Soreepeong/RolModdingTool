using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Structs;
using WiiUStreamTool.Util.BinaryRW;

namespace WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Chunks;

public struct CompiledBonesChunk : ICryReadWrite {
    public ChunkHeader Header;
    public readonly List<CompiledBone> Bones = new();

    public CompiledBonesChunk() { }

    public void ReadFrom(NativeReader reader, int expectedSize) {
        var expectedEnd = reader.BaseStream.Position + expectedSize;
        Header.ReadFrom(reader, Unsafe.SizeOf<ChunkHeader>());
        using (reader.ScopedBigEndian(Header.IsBigEndian)) {
            reader.EnsureZeroesOrThrow(32);

            var bone = new CompiledBone();
            var boneCount = 1;
            Bones.Clear();
            for (var i = 0; i < boneCount; i++) {
                bone.ReadFrom(reader, 584);
                boneCount = Math.Max(boneCount, i + bone.ChildOffset + bone.ChildCount);
                Bones.Add(bone);
            }
        }

        reader.EnsurePositionOrThrow(expectedEnd);
    }

    public void WriteTo(NativeWriter writer, bool useBigEndian) {
        throw new NotImplementedException();
    }

    public override string ToString() => $"{nameof(CompiledBonesChunk)}: {Header}";
}