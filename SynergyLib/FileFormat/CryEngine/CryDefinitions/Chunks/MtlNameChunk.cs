using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Enums;
using SynergyLib.Util.BinaryRW;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Chunks;

public class MtlNameChunk : ICryChunk {
    public ChunkHeader Header { get; set; } = new();
    public MtlNameFlags Flags;
    public uint Flags2;
    public string Name = string.Empty;
    public MtlNamePhysicsType PhysicsType = MtlNamePhysicsType.None;
    public List<int> SubMaterialChunkIds = new();
    public int AdvancedDataChunkId;
    public float ShOpacity = 1f;

    public MtlNameChunk() { }

    public void ReadFrom(NativeReader reader, int expectedSize) {
        var expectedEnd = reader.BaseStream.Position + expectedSize;
        Header = new(reader);
        using (reader.ScopedBigEndian(Header.IsBigEndian)) {
            reader.ReadInto(out Flags);
            reader.ReadInto(out Flags2);
            Name = reader.ReadFString(128, Encoding.UTF8);
            reader.ReadInto(out PhysicsType);

            reader.ReadInto(out int childCount);
            if (childCount > 32)
                throw new InvalidDataException();
            SubMaterialChunkIds.EnsureCapacity(childCount);
            for (var i = 0; i < childCount; i++)
                SubMaterialChunkIds.Add(reader.ReadInt32());
            reader.BaseStream.Position += (32 - childCount) * 4;

            reader.ReadInto(out AdvancedDataChunkId);
            reader.ReadInto(out ShOpacity);
            reader.EnsureZeroesOrThrow(128);
        }

        reader.EnsurePositionOrThrow(expectedEnd);
    }

    public void WriteTo(NativeWriter writer, bool useBigEndian) {
        Header.WriteTo(writer, false);
        using (writer.ScopedBigEndian(useBigEndian)) {
            writer.WriteEnum(Flags);
            writer.Write(Flags2);
            writer.WriteFString(Name, 128, Encoding.UTF8);
            writer.WriteEnum(PhysicsType);

            if (SubMaterialChunkIds.Count > 32)
                throw new InvalidDataException();

            writer.Write(SubMaterialChunkIds.Count);
            foreach (var t in SubMaterialChunkIds)
                writer.Write(t);
            for (var i = SubMaterialChunkIds.Count; i < 32; i++)
                writer.Write(0);

            writer.Write(AdvancedDataChunkId);
            writer.Write(ShOpacity);
            writer.FillZeroes(128);
        }
    }

    public int WrittenSize => Header.WrittenSize + 408;

    public override string ToString() => $"{nameof(MtlNameChunk)}: {Header}: {Name}";

    public static MtlNameChunk FindChunkForMainMesh(CryChunks chunks) {
        var nodeChunk = chunks.Values.OfType<NodeChunk>().Single(
            x => x.ParentId == -1 && !((MeshChunk) chunks[x.ObjectId]).Flags.HasFlag(MeshChunkFlags.MeshIsEmpty));
        return (MtlNameChunk) chunks[nodeChunk.MaterialId];
    }
}
