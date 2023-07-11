using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Enums;
using WiiUStreamTool.Util;
using WiiUStreamTool.Util.BinaryRW;

namespace WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Structs;

public struct ControllerKeyRotation : IReadOnlyList<Quaternion> {
    public CompressionFormat Format;
    public byte[] RawData = Array.Empty<byte>();

    static ControllerKeyRotation() {
        Debug.Assert(Unsafe.SizeOf<Quaternion>() == 16);
    }

    public ControllerKeyRotation() { }

    public Quaternion this[int index] {
        get => Format switch {
            CompressionFormat.NoCompressQuat => RawData.AsSpan().GetNativeStruct<Quaternion>(index),
            CompressionFormat.ShortInt3Quat => RawData.AsSpan().GetNativeStruct<ShortInt3Quat>(index),
            CompressionFormat.SmallTreeQuat32 => RawData.AsSpan().GetNativeStruct<SmallTreeQuat32>(index),
            CompressionFormat.SmallTreeQuat48 => RawData.AsSpan().GetNativeStruct<SmallTreeQuat48>(index),
            CompressionFormat.SmallTreeQuat64 => RawData.AsSpan().GetNativeStruct<SmallTreeQuat64>(index),
            CompressionFormat.PolarQuat => RawData.AsSpan().GetNativeStruct<PolarQuat>(index),
            CompressionFormat.SmallTreeQuat64Ext => RawData.AsSpan().GetNativeStruct<SmallTreeQuat64Ext>(index),
            CompressionFormat.NoCompress => throw new NotSupportedException(),
            CompressionFormat.NoCompressVec3 => throw new NotSupportedException(),
            _ => throw new NotSupportedException(),
        };
        set {
            switch (Format) {
                case CompressionFormat.NoCompressQuat:
                    RawData.AsSpan().SetNativeStruct(value, index);
                    break;
                case CompressionFormat.ShortInt3Quat:
                    RawData.AsSpan().SetNativeStruct((ShortInt3Quat) value, index);
                    break;
                case CompressionFormat.SmallTreeQuat32:
                    RawData.AsSpan().SetNativeStruct((SmallTreeQuat32) value, index);
                    break;
                case CompressionFormat.SmallTreeQuat48:
                    RawData.AsSpan().SetNativeStruct((SmallTreeQuat48) value, index);
                    break;
                case CompressionFormat.SmallTreeQuat64:
                    RawData.AsSpan().SetNativeStruct((SmallTreeQuat64) value, index);
                    break;
                case CompressionFormat.PolarQuat:
                    RawData.AsSpan().SetNativeStruct((PolarQuat) value, index);
                    break;
                case CompressionFormat.SmallTreeQuat64Ext:
                    RawData.AsSpan().SetNativeStruct((SmallTreeQuat64Ext) value, index);
                    break;
                case CompressionFormat.NoCompress:
                case CompressionFormat.NoCompressVec3:
                default:
                    throw new NotSupportedException();
            }
        }
    }

    public void ReadFrom(NativeReader b, CompressionFormat format, int length) {
        GetElementComponentSizes(format, out var cBytesPerNumber, out var cNumberPerElement);

        var data = b.ReadBytes(length * cBytesPerNumber * cNumberPerElement);
        if (BitConverter.IsLittleEndian == b.IsBigEndian) {
            for (var i = 0; i < data.Length; i += cBytesPerNumber)
                data.AsSpan(i, cBytesPerNumber).Reverse();
        }

        Format = format;
        RawData = data;
    }

    public unsafe void WriteTo(NativeWriter w) {
        if (BitConverter.IsLittleEndian != w.IsBigEndian) {
            w.Write(RawData);
        } else {
            GetElementComponentSizes(Format, out var cBytesPerNumber, out _);
            fixed (void* p = RawData)
                switch (cBytesPerNumber) {
                    case 1:
                        w.Write(RawData);
                        break;
                    case 2:
                        foreach (var s in new Span<ushort>(p, RawData.Length / cBytesPerNumber))
                            w.Write(s);
                        break;
                    case 4:
                        foreach (var s in new Span<uint>(p, RawData.Length / cBytesPerNumber))
                            w.Write(s);
                        break;
                    case 8:
                        foreach (var s in new Span<ulong>(p, RawData.Length / cBytesPerNumber))
                            w.Write(s);
                        break;
                    default:
                        throw new NotSupportedException();
                }
        }
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
        CompressionFormat.NoCompressQuat => RawData.Length / Unsafe.SizeOf<Quaternion>(),
        CompressionFormat.ShortInt3Quat => RawData.Length / Unsafe.SizeOf<ShortInt3Quat>(),
        CompressionFormat.SmallTreeQuat32 => RawData.Length / Unsafe.SizeOf<SmallTreeQuat32>(),
        CompressionFormat.SmallTreeQuat48 => RawData.Length / Unsafe.SizeOf<SmallTreeQuat48>(),
        CompressionFormat.SmallTreeQuat64 => RawData.Length / Unsafe.SizeOf<SmallTreeQuat64>(),
        CompressionFormat.PolarQuat => RawData.Length / Unsafe.SizeOf<PolarQuat>(),
        CompressionFormat.SmallTreeQuat64Ext => RawData.Length / Unsafe.SizeOf<SmallTreeQuat64Ext>(),
        _ => 0,
    };

    private static void GetElementComponentSizes(CompressionFormat format, out int intSize, out int intCount) =>
        (intSize, intCount) = format switch {
            CompressionFormat.NoCompressQuat => (4, 4),
            CompressionFormat.ShortInt3Quat => (2, 3),
            CompressionFormat.SmallTreeQuat32 => (4, 1),
            CompressionFormat.SmallTreeQuat48 => (2, 3),
            CompressionFormat.SmallTreeQuat64 => (4, 2),
            CompressionFormat.PolarQuat => (2, 3),
            CompressionFormat.SmallTreeQuat64Ext => (4, 2),
            CompressionFormat.NoCompress => throw new ArgumentOutOfRangeException(nameof(format), format, null),
            CompressionFormat.NoCompressVec3 => throw new ArgumentOutOfRangeException(nameof(format), format, null),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
        };
}
