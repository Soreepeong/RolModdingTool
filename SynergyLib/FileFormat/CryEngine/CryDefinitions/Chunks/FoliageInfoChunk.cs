using System;
using System.Numerics;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;
using SynergyLib.Util.BinaryRW;
using SynergyLib.Util.MathExtras;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Chunks;

public class FoliageInfoChunk : ICryChunk {
    public ChunkHeader Header { get; set; } = new();
    public FoliageSpineSubChunk[] Spines = Array.Empty<FoliageSpineSubChunk>();
    public Vector3[] SpineVertices = Array.Empty<Vector3>();
    public Vector4[] SpineVertexSegDim = Array.Empty<Vector4>();
    public MeshBoneMapping[] BoneMappings = Array.Empty<MeshBoneMapping>();
    public ushort[] BoneIds = Array.Empty<ushort>();

    public FoliageInfoChunk() { }

    public void ReadFrom(NativeReader reader, int expectedSize) {
        var expectedEnd = reader.BaseStream.Position + expectedSize;
        Header = new(reader);
        using (reader.ScopedBigEndian(Header.IsBigEndian)) {
            reader.ReadInto(out int spineCount);
            reader.ReadInto(out int spineVertexCount);
            reader.ReadInto(out int skinnedVertexCount);
            reader.ReadInto(out int boneIdCount);

            Spines = new FoliageSpineSubChunk[spineCount];
            for (var i = 0; i < spineCount; i++)
                Spines[i].ReadFrom(reader, 24);

            SpineVertices = new Vector3[spineVertexCount];
            for (var i = 0; i < spineVertexCount; i++)
                SpineVertices[i] = reader.ReadVector3();

            SpineVertexSegDim = new Vector4[spineVertexCount];
            for (var i = 0; i < spineVertexCount; i++)
                SpineVertexSegDim[i] = reader.ReadVector4();

            BoneMappings = new MeshBoneMapping[skinnedVertexCount];
            for (var i = 0; i < skinnedVertexCount; i++)
                BoneMappings[i].ReadFrom(reader, 8);

            BoneIds = new ushort[boneIdCount];
            for (var i = 0; i < boneIdCount; i++)
                BoneIds[i] = reader.ReadUInt16();
        }

        reader.EnsurePositionOrThrow(expectedEnd);
    }

    public void WriteTo(NativeWriter writer, bool useBigEndian) {
        Header.WriteTo(writer, false);
        using (writer.ScopedBigEndian(useBigEndian)) {
            writer.Write(Spines.Length);
            writer.Write(SpineVertices.Length);
            writer.Write(BoneMappings.Length);
            writer.Write(BoneIds.Length);

            foreach (var s in Spines)
                s.WriteTo(writer, useBigEndian);

            foreach (var s in SpineVertices)
                writer.Write(s);

            foreach (var s in SpineVertexSegDim)
                writer.Write(s);

            foreach (var s in BoneMappings)
                s.WriteTo(writer, useBigEndian);

            foreach (var s in BoneIds)
                writer.Write(s);
        }
    }

    public int WrittenSize => Header.WrittenSize + 16 + 24 * Spines.Length + 12 * SpineVertices.Length +
        16 * SpineVertexSegDim.Length + 8 * BoneMappings.Length + 2 * BoneIds.Length;

    public override string ToString() => $"{nameof(FoliageInfoChunk)}: {Header}";
}
