using System;
using System.Runtime.CompilerServices;
using System.Text;
using WiiUStreamTool.Util.BinaryRW;

namespace WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Chunks;

public enum CgfStreamType {
    Positions,
    Normals,
    TexCoords,
    Colors,
    Colors2,
    Indices,
    Tangents,
    ShCoeffs,
    ShapeDeformation,
    BoneMapping,
    FaceMap,
    VertMats,
    QTangents,
    SkinData,
    Ps3EdgeData,
}

public struct DataChunk : ICryReadWrite {
    public ChunkHeader Header;
    public uint Flags;
    public CgfStreamType Type;
    public int ElementSize;
    public byte[] Data;

    public DataChunk() { }

    public void ReadFrom(NativeReader reader, int expectedSize) {
        var expectedEnd = reader.BaseStream.Position + expectedSize;
        Header.ReadFrom(reader, Unsafe.SizeOf<ChunkHeader>());
        using (reader.ScopedBigEndian(Header.IsBigEndian)) {
            reader.ReadInto(out Flags);
            reader.ReadInto(out Type);
            reader.ReadInto(out int elementCount);
            reader.ReadInto(out ElementSize);
            reader.EnsureZeroesOrThrow(8);
            Data = reader.ReadBytes(ElementSize * elementCount);
            if (BitConverter.IsLittleEndian != Header.IsBigEndian) {
                var dataSpan = Data.AsSpan();
                var flipUnit = 1;
                switch (Type) {
                    // byte
                    case CgfStreamType.Indices when ElementSize == 1:
                    case CgfStreamType.Tangents when ElementSize == 8 * 1:
                    case CgfStreamType.BoneMapping when ElementSize == 4 * 1 + 4 * 1:
                        flipUnit = 1;
                        break;

                    // half
                    case CgfStreamType.Positions when ElementSize == 3 * 2:
                    case CgfStreamType.Normals when ElementSize == 3 * 2:
                    case CgfStreamType.TexCoords when ElementSize == 2 * 2:
                        flipUnit = 2;
                        break;

                    // single
                    case CgfStreamType.Positions when ElementSize == 3 * 4:
                    case CgfStreamType.Normals when ElementSize == 3 * 4:
                    case CgfStreamType.TexCoords when ElementSize == 2 * 4:
                        flipUnit = 4;
                        break;

                    // double
                    case CgfStreamType.Positions when ElementSize == 3 * 8:
                    case CgfStreamType.Normals when ElementSize == 3 * 8:
                    case CgfStreamType.TexCoords when ElementSize == 2 * 8:
                        flipUnit = 8;
                        break;

                    // short
                    case CgfStreamType.Indices when ElementSize == 2:
                    case CgfStreamType.Tangents when ElementSize == 8 * 2:
                        flipUnit = 2;
                        break;

                    // int
                    case CgfStreamType.Indices when ElementSize == 4:
                        flipUnit = 4;
                        break;

                    // mixed short+byte
                    case CgfStreamType.BoneMapping when ElementSize == 4 * 2 + 4 * 1:
                        for (var i = 0; i < dataSpan.Length; i += 12) {
                            dataSpan[i..(i + 2)].Reverse();
                            dataSpan[(i + 2)..(i + 4)].Reverse();
                            dataSpan[(i + 4)..(i + 6)].Reverse();
                            dataSpan[(i + 6)..(i + 8)].Reverse();
                        }

                        break;

                    // unsupported
                    case CgfStreamType.ShCoeffs:
                    case CgfStreamType.ShapeDeformation:
                    case CgfStreamType.FaceMap:
                    case CgfStreamType.VertMats:
                    case CgfStreamType.QTangents:
                    case CgfStreamType.SkinData:
                    case CgfStreamType.Ps3EdgeData:
                    default:
                        throw new NotSupportedException($"Type={Type} ElementSize={ElementSize}");
                }

                for (var i = 0; i < dataSpan.Length; i += flipUnit)
                    dataSpan[i..(i + flipUnit)].Reverse();
            }
        }

        reader.EnsurePositionOrThrow(expectedEnd);
    }

    public void WriteTo(NativeWriter writer, bool useBigEndian) {
        throw new NotImplementedException();
    }

    public override string ToString() => $"{nameof(DataChunk)}: {Header}";
}
