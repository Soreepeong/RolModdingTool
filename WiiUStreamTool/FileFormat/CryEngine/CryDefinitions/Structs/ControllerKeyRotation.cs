using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Enums;
using WiiUStreamTool.Util.BinaryRW;

namespace WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Structs;

public struct ControllerKeyRotation : IReadOnlyList<Quaternion> {
    public CompressionFormat Format;
    public byte[] RawData = Array.Empty<byte>();

    public ControllerKeyRotation() { }

    public Quaternion this[int index] {
        get => Format switch {
            CompressionFormat.NoCompressQuat => new(
                BitConverter.ToSingle(RawData, index * 16 + 0),
                BitConverter.ToSingle(RawData, index * 16 + 4),
                BitConverter.ToSingle(RawData, index * 16 + 8),
                BitConverter.ToSingle(RawData, index * 16 + 12)),
            CompressionFormat.ShortInt3Quat => new ShortInt3Quat {
                X = BitConverter.ToInt16(RawData, index * 6 + 0),
                Y = BitConverter.ToInt16(RawData, index * 6 + 2),
                Z = BitConverter.ToInt16(RawData, index * 6 + 4),
            },
            CompressionFormat.SmallTreeQuat32 => new SmallTreeQuat32 {
                Value = BitConverter.ToUInt32(RawData, index * 4 + 0),
            },
            CompressionFormat.SmallTreeQuat48 => new SmallTreeQuat48 {
                M1 = BitConverter.ToUInt16(RawData, index * 6 + 0),
                M2 = BitConverter.ToUInt16(RawData, index * 6 + 2),
                M3 = BitConverter.ToUInt16(RawData, index * 6 + 4),
            },
            CompressionFormat.SmallTreeQuat64 => new SmallTreeQuat64 {
                M1 = BitConverter.ToUInt32(RawData, index * 8 + 0),
                M2 = BitConverter.ToUInt32(RawData, index * 8 + 4),
            },
            CompressionFormat.PolarQuat => new PolarQuat {
                Yaw = BitConverter.ToInt16(RawData, index * 6 + 0),
                Pitch = BitConverter.ToInt16(RawData, index * 6 + 2),
                W = BitConverter.ToInt16(RawData, index * 6 + 4),
            },
            CompressionFormat.SmallTreeQuat64Ext => new SmallTreeQuat64Ext {
                M1 = BitConverter.ToUInt32(RawData, index * 8 + 0),
                M2 = BitConverter.ToUInt32(RawData, index * 8 + 4),
            },
            CompressionFormat.NoCompress => throw new NotSupportedException(),
            CompressionFormat.NoCompressVec3 => throw new NotSupportedException(),
            _ => throw new NotSupportedException(),
        };
        set => throw new NotImplementedException();
    }

    public void ReadFrom(NativeReader b, CompressionFormat format, int length) {
        var (cBytesPerNumber, cNumberPerElement) = format switch {
            CompressionFormat.NoCompressQuat => (4, 4),
            CompressionFormat.ShortInt3Quat => (2, 3),
            CompressionFormat.SmallTreeQuat32 => (4, 1),
            CompressionFormat.SmallTreeQuat48 => (2, 3),
            CompressionFormat.SmallTreeQuat64 => (4, 2),
            CompressionFormat.PolarQuat => (2, 3),
            CompressionFormat.SmallTreeQuat64Ext => (4, 2),
            CompressionFormat.NoCompress => throw new ArgumentOutOfRangeException(nameof(format), format, null),
            CompressionFormat.NoCompressVec3 => throw new ArgumentOutOfRangeException(nameof(format), format, null),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
        
        var data = b.ReadBytes(length * cBytesPerNumber * cNumberPerElement);
        if (BitConverter.IsLittleEndian == b.IsBigEndian) {
            for (var i = 0; i < data.Length; i += cBytesPerNumber)
                data.AsSpan(i, cBytesPerNumber).Reverse();
        }

        Format = format;
        RawData = data;
    }

    public void WriteTo(NativeWriter w) {
        throw new NotImplementedException();
    }

    public int WrittenSize => RawData.Length;

    public IEnumerator<Quaternion> GetEnumerator() {
        for (var i = 0; i < Count; i++)
            yield return this[i];
    }

    public override string ToString() => $"{nameof(ControllerKeyRotation)}<{Format}>: {Count} items";

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public int Count => Format switch {
        CompressionFormat.NoCompressVec3 => 0,
        CompressionFormat.NoCompress => 0,
        CompressionFormat.NoCompressQuat => RawData.Length / 16,
        CompressionFormat.ShortInt3Quat => RawData.Length / 6,
        CompressionFormat.SmallTreeQuat32 => RawData.Length / 4,
        CompressionFormat.SmallTreeQuat48 => RawData.Length / 6,
        CompressionFormat.SmallTreeQuat64 => RawData.Length / 8,
        CompressionFormat.PolarQuat => RawData.Length / 6,
        CompressionFormat.SmallTreeQuat64Ext => RawData.Length / 8,
        _ => 0,
    };
}