using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Enums;
using SynergyLib.Util.BinaryRW;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Chunks;

public class DataChunk : ICryChunk {
    public ChunkHeader Header { get; set; } = new();
    public uint Flags;
    public CgfStreamType Type;
    public int ElementSize;
    public byte[] NativeData = Array.Empty<byte>();

    public DataChunk() { }

    public void ReadFrom(NativeReader reader, int expectedSize) {
        var expectedEnd = reader.BaseStream.Position + expectedSize;
        Header = new(reader);
        using (reader.ScopedBigEndian(Header.IsBigEndian)) {
            reader.ReadInto(out Flags);
            reader.ReadInto(out Type);
            reader.ReadInto(out int elementCount);
            reader.ReadInto(out ElementSize);
            reader.EnsureZeroesOrThrow(8);
            NativeData = reader.ReadBytes(ElementSize * elementCount);
            if (BitConverter.IsLittleEndian == Header.IsBigEndian) {
                var dataSpan = NativeData.AsSpan();
                int flipUnit;
                switch (Type) {
                    // byte
                    case CgfStreamType.Indices when ElementSize == 1:
                    case CgfStreamType.Tangents when ElementSize == 8 * 1:
                    case CgfStreamType.Colors when ElementSize == 3:
                    case CgfStreamType.Colors when ElementSize == 4:
                    case CgfStreamType.Colors2 when ElementSize == 3:
                    case CgfStreamType.Colors2 when ElementSize == 4:
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

                    case CgfStreamType.BoneMapping when ElementSize == 4 * 2 + 4 * 1:
                        flipUnit = 0;
                        for (var i = 0; i < dataSpan.Length; i += 12) {
                            dataSpan[i..(i + 2)].Reverse();
                            dataSpan[(i + 2)..(i + 4)].Reverse();
                            dataSpan[(i + 4)..(i + 6)].Reverse();
                            dataSpan[(i + 6)..(i + 8)].Reverse();
                        }

                        break;

                    case CgfStreamType.ShapeDeformation when ElementSize == 28:
                        flipUnit = 0;
                        for (var i = 0; i < dataSpan.Length; i += 28) {
                            dataSpan[i..(i + 4)].Reverse();
                            dataSpan[(i + 4)..(i + 8)].Reverse();
                            dataSpan[(i + 8)..(i + 12)].Reverse();
                            dataSpan[(i + 12)..(i + 16)].Reverse();
                            dataSpan[(i + 16)..(i + 20)].Reverse();
                            dataSpan[(i + 20)..(i + 24)].Reverse();
                        }

                        break;

                    // unsupported
                    case CgfStreamType.ShCoeffs:
                    case CgfStreamType.FaceMap:
                    case CgfStreamType.VertMats:
                    case CgfStreamType.QTangents:
                    case CgfStreamType.SkinData:
                    case CgfStreamType.Ps3EdgeData:
                    default:
                        throw new NotSupportedException($"Type={Type} ElementSize={ElementSize}");
                }

                if (flipUnit >= 2) {
                    for (var i = 0; i < dataSpan.Length; i += flipUnit)
                        dataSpan[i..(i + flipUnit)].Reverse();
                }
            }
        }

        reader.EnsurePositionOrThrow(expectedEnd);
    }

    public void WriteTo(NativeWriter writer, bool useBigEndian) {
        Header.WriteTo(writer, false);
        using (writer.ScopedBigEndian(useBigEndian)) {
            var elementCount = NativeData.Length / ElementSize;
            writer.Write(Flags);
            writer.WriteEnum(Type);
            writer.Write(elementCount);
            writer.Write(ElementSize);
            writer.FillZeroes(8);
            if (BitConverter.IsLittleEndian != Header.IsBigEndian)
                writer.Write(NativeData);
            else {
                var dataSpan = NativeData.AsSpan();
                int flipUnit;
                switch (Type) {
                    // byte
                    case CgfStreamType.Indices when ElementSize == 1:
                    case CgfStreamType.Tangents when ElementSize == 8 * 1:
                    case CgfStreamType.Colors when ElementSize == 3:
                    case CgfStreamType.Colors when ElementSize == 4:
                    case CgfStreamType.Colors2 when ElementSize == 3:
                    case CgfStreamType.Colors2 when ElementSize == 4:
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

                    case CgfStreamType.BoneMapping when ElementSize == 4 * 2 + 4 * 1: {
                        flipUnit = 0;

                        Span<byte> rowBuffer = stackalloc byte[ElementSize];
                        for (var i = 0; i < dataSpan.Length; i += ElementSize) {
                            dataSpan[i..(i + ElementSize)].CopyTo(rowBuffer);
                            rowBuffer[..2].Reverse();
                            rowBuffer[2..4].Reverse();
                            rowBuffer[4..6].Reverse();
                            rowBuffer[6..8].Reverse();
                            writer.Write(rowBuffer);
                        }

                        break;
                    }

                    case CgfStreamType.ShapeDeformation when ElementSize == 28: {
                        flipUnit = 0;

                        Span<byte> rowBuffer = stackalloc byte[ElementSize];
                        for (var i = 0; i < dataSpan.Length; i += ElementSize) {
                            dataSpan[i..(i + ElementSize)].CopyTo(rowBuffer);
                            rowBuffer[..4].Reverse();
                            rowBuffer[4..8].Reverse();
                            rowBuffer[8..12].Reverse();
                            rowBuffer[12..16].Reverse();
                            rowBuffer[16..20].Reverse();
                            rowBuffer[20..24].Reverse();
                            writer.Write(rowBuffer);
                        }

                        break;
                    }

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

                switch (flipUnit) {
                    case 0:
                        break;
                    case 1:
                        writer.Write(NativeData);
                        break;
                    case 2:
                        unsafe {
                            fixed (byte* p = dataSpan) {
                                var span = new Span<short>(p, dataSpan.Length / 2);
                                foreach (var s in span)
                                    writer.Write(s);
                            }

                            break;
                        }
                    case 4:
                        unsafe {
                            fixed (byte* p = dataSpan) {
                                var span = new Span<int>(p, dataSpan.Length / 4);
                                foreach (var s in span)
                                    writer.Write(s);
                            }

                            break;
                        }
                    case 8:
                        unsafe {
                            fixed (byte* p = dataSpan) {
                                var span = new Span<long>(p, dataSpan.Length / 8);
                                foreach (var s in span)
                                    writer.Write(s);
                            }

                            break;
                        }
                    default:
                        throw new NotSupportedException();
                }
            }
        }
    }

    public int WrittenSize => Header.WrittenSize + 24 + NativeData.Length;

    public override string ToString() => $"{nameof(DataChunk)}: {Header}";

    public T GetItem<T>(int index) where T : unmanaged {
        if (Unsafe.SizeOf<T>() != ElementSize)
            throw new ArgumentException(null, nameof(T));
        if (index < 0 || index * ElementSize > NativeData.Length)
            throw new ArgumentOutOfRangeException(nameof(index), index, null);
        return GetItemUnchecked<T>(index);
    }

    public void SetItem<T>(int index, in T value) where T : unmanaged {
        if (Unsafe.SizeOf<T>() != ElementSize)
            throw new ArgumentException(null, nameof(T));
        if (index < 0 || index * ElementSize > NativeData.Length)
            throw new ArgumentOutOfRangeException(nameof(index), index, null);
        SetItemUnchecked(index, value);
    }

    public unsafe T[] AsArray<T>() where T : unmanaged {
        if (sizeof(T) != ElementSize)
            throw new ArgumentException(null, nameof(T));

        var res = new T[NativeData.Length / ElementSize];
        fixed (void* p = res)
            NativeData.CopyTo(new Span<byte>(p, NativeData.Length));
        return res;
    }

    public IEnumerable<T> AsEnumerable<T>() where T : unmanaged {
        var count = NativeData.Length / ElementSize;
        for (var i = 0; i < count; i++)
            yield return GetItemUnchecked<T>(i);
    }

    public void FromEnumerable<T>(IEnumerable<T> items, int count) where T : unmanaged {
        ElementSize = Unsafe.SizeOf<T>();
        NativeData = new byte[count * ElementSize];
        var i = 0;
        foreach (var item in items)
            SetItemUnchecked(i++, item);
        
        Debug.Assert(i == count);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe T GetItemUnchecked<T>(int index) where T : unmanaged {
        fixed (void* p = &NativeData[index * ElementSize])
            return *(T*) p;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private unsafe void SetItemUnchecked<T>(int index, in T value) where T : unmanaged {
        fixed (void* p = &NativeData[index * ElementSize])
            *(T*) p = value;
    }
}
