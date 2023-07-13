using SynergyLib.FileFormat.CryEngine.CryDefinitions.Enums;
using SynergyLib.Util.BinaryRW;
using SynergyLib.Util.MathExtras;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Chunks;

public struct MeshChunk : ICryChunk {
    public ChunkHeader Header { get; set; }

    public MeshChunkFlags Flags;
    public int Flags2;
    public int VertexCount;
    public int IndexCount;
    public int SubsetsCount;
    public int SubsetsChunkId;
    public int VertAnimId;
    public int PositionsChunkId;
    public int NormalsChunkId;
    public int TexCoordsChunkId;
    public int ColorsChunkId;
    public int Colors2ChunkId;
    public int IndicesChunkId;
    public int TangentsChunkId;
    public int ShCoeffsChunkId;
    public int ShapeDeformationChunkId;
    public int BoneMappingChunkId;
    public int FaceMapChunkId;
    public int VertMatsChunkId;
    public int QTangentsChunkId;
    public int SkinDataChunkId;
    public int Ps3EdgeDataChunkId;
    public int Reserved15ChunkId;
    public unsafe fixed int PhysicsDataChunkId[4];
    public AaBb Bbox;
    public float TexMappingDensity;

    public MeshChunk() { }

    public void ReadFrom(NativeReader reader, int expectedSize) {
        var expectedEnd = reader.BaseStream.Position + expectedSize;
        Header = new(reader);
        using (reader.ScopedBigEndian(Header.IsBigEndian)) {
            reader.ReadInto(out Flags);
            reader.ReadInto(out Flags2);
            reader.ReadInto(out VertexCount);
            reader.ReadInto(out IndexCount);
            reader.ReadInto(out SubsetsCount);
            reader.ReadInto(out SubsetsChunkId);
            reader.ReadInto(out VertAnimId);
            reader.ReadInto(out PositionsChunkId);
            reader.ReadInto(out NormalsChunkId);
            reader.ReadInto(out TexCoordsChunkId);
            reader.ReadInto(out ColorsChunkId);
            reader.ReadInto(out Colors2ChunkId);
            reader.ReadInto(out IndicesChunkId);
            reader.ReadInto(out TangentsChunkId);
            reader.ReadInto(out ShCoeffsChunkId);
            reader.ReadInto(out ShapeDeformationChunkId);
            reader.ReadInto(out BoneMappingChunkId);
            reader.ReadInto(out FaceMapChunkId);
            reader.ReadInto(out VertMatsChunkId);
            reader.ReadInto(out QTangentsChunkId);
            reader.ReadInto(out SkinDataChunkId);
            reader.ReadInto(out Ps3EdgeDataChunkId);
            reader.ReadInto(out Reserved15ChunkId);
            unsafe {
                reader.ReadInto(out PhysicsDataChunkId[0]);
                reader.ReadInto(out PhysicsDataChunkId[1]);
                reader.ReadInto(out PhysicsDataChunkId[2]);
                reader.ReadInto(out PhysicsDataChunkId[3]);
            }

            Bbox = reader.ReadAaBb();
            reader.ReadInto(out TexMappingDensity);

            reader.EnsureZeroesOrThrow(31 * 4);
        }

        reader.EnsurePositionOrThrow(expectedEnd);
    }

    public readonly void WriteTo(NativeWriter writer, bool useBigEndian) {
        Header.WriteTo(writer, false);
        using (writer.ScopedBigEndian(useBigEndian)) {
            writer.WriteEnum(Flags);
            writer.Write(Flags2);
            writer.Write(VertexCount);
            writer.Write(IndexCount);
            writer.Write(SubsetsCount);
            writer.Write(SubsetsChunkId);
            writer.Write(VertAnimId);
            writer.Write(PositionsChunkId);
            writer.Write(NormalsChunkId);
            writer.Write(TexCoordsChunkId);
            writer.Write(ColorsChunkId);
            writer.Write(Colors2ChunkId);
            writer.Write(IndicesChunkId);
            writer.Write(TangentsChunkId);
            writer.Write(ShCoeffsChunkId);
            writer.Write(ShapeDeformationChunkId);
            writer.Write(BoneMappingChunkId);
            writer.Write(FaceMapChunkId);
            writer.Write(VertMatsChunkId);
            writer.Write(QTangentsChunkId);
            writer.Write(SkinDataChunkId);
            writer.Write(Ps3EdgeDataChunkId);
            writer.Write(Reserved15ChunkId);
            unsafe {
                writer.Write(PhysicsDataChunkId[0]);
                writer.Write(PhysicsDataChunkId[1]);
                writer.Write(PhysicsDataChunkId[2]);
                writer.Write(PhysicsDataChunkId[3]);
            }

            writer.Write(Bbox);
            writer.Write(TexMappingDensity);

            writer.FillZeroes(31 * 4);
        }
    }

    public int WrittenSize => Header.WrittenSize + 260;

    public override string ToString() => $"{nameof(MeshChunk)}: {Header}";
}
