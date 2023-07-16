using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Enums;
using SynergyLib.Util;
using SynergyLib.Util.BinaryRW;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;

public class ControllerKeyRotation : IReadOnlyList<Quaternion> {
    public VectorCompressionFormat Format;
    public byte[] RawData = Array.Empty<byte>();

    static ControllerKeyRotation() {
        Debug.Assert(Unsafe.SizeOf<Quaternion>() == 16);
    }

    public Quaternion this[int index] {
        get => Format switch {
            VectorCompressionFormat.NoCompressQuat => RawData.AsSpan().GetNativeStruct<Quaternion>(index),
            VectorCompressionFormat.ShortInt3Quat => RawData.AsSpan().GetNativeStruct<ShortInt3Quat>(index),
            VectorCompressionFormat.SmallTreeQuat32 => RawData.AsSpan().GetNativeStruct<SmallTreeQuat32>(index),
            VectorCompressionFormat.SmallTreeQuat48 => RawData.AsSpan().GetNativeStruct<SmallTreeQuat48>(index),
            VectorCompressionFormat.SmallTreeQuat64 => RawData.AsSpan().GetNativeStruct<SmallTreeQuat64>(index),
            VectorCompressionFormat.PolarQuat => RawData.AsSpan().GetNativeStruct<PolarQuat>(index),
            VectorCompressionFormat.SmallTreeQuat64Ext => RawData.AsSpan().GetNativeStruct<SmallTreeQuat64Ext>(index),
            VectorCompressionFormat.NoCompress => throw new NotSupportedException(),
            VectorCompressionFormat.NoCompressVec3 => throw new NotSupportedException(),
            _ => throw new NotSupportedException(),
        };
        set {
            switch (Format) {
                case VectorCompressionFormat.NoCompressQuat:
                    RawData.AsSpan().SetNativeStruct(value, index);
                    break;
                case VectorCompressionFormat.ShortInt3Quat:
                    RawData.AsSpan().SetNativeStruct((ShortInt3Quat) value, index);
                    break;
                case VectorCompressionFormat.SmallTreeQuat32:
                    RawData.AsSpan().SetNativeStruct((SmallTreeQuat32) value, index);
                    break;
                case VectorCompressionFormat.SmallTreeQuat48:
                    RawData.AsSpan().SetNativeStruct((SmallTreeQuat48) value, index);
                    break;
                case VectorCompressionFormat.SmallTreeQuat64:
                    RawData.AsSpan().SetNativeStruct((SmallTreeQuat64) value, index);
                    break;
                case VectorCompressionFormat.PolarQuat:
                    RawData.AsSpan().SetNativeStruct((PolarQuat) value, index);
                    break;
                case VectorCompressionFormat.SmallTreeQuat64Ext:
                    RawData.AsSpan().SetNativeStruct((SmallTreeQuat64Ext) value, index);
                    break;
                case VectorCompressionFormat.NoCompress:
                case VectorCompressionFormat.NoCompressVec3:
                default:
                    throw new NotSupportedException();
            }
        }
    }

    public void ReadFrom(NativeReader b, VectorCompressionFormat format, int length) {
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
        VectorCompressionFormat.NoCompressVec3 => 0,
        VectorCompressionFormat.NoCompress => 0,
        VectorCompressionFormat.NoCompressQuat => RawData.Length / Unsafe.SizeOf<Quaternion>(),
        VectorCompressionFormat.ShortInt3Quat => RawData.Length / Unsafe.SizeOf<ShortInt3Quat>(),
        VectorCompressionFormat.SmallTreeQuat32 => RawData.Length / Unsafe.SizeOf<SmallTreeQuat32>(),
        VectorCompressionFormat.SmallTreeQuat48 => RawData.Length / Unsafe.SizeOf<SmallTreeQuat48>(),
        VectorCompressionFormat.SmallTreeQuat64 => RawData.Length / Unsafe.SizeOf<SmallTreeQuat64>(),
        VectorCompressionFormat.PolarQuat => RawData.Length / Unsafe.SizeOf<PolarQuat>(),
        VectorCompressionFormat.SmallTreeQuat64Ext => RawData.Length / Unsafe.SizeOf<SmallTreeQuat64Ext>(),
        _ => 0,
    };

    private static void GetElementComponentSizes(VectorCompressionFormat format, out int intSize, out int intCount) =>
        (intSize, intCount) = format switch {
            VectorCompressionFormat.NoCompressQuat => (4, 4),
            VectorCompressionFormat.ShortInt3Quat => (2, 3),
            VectorCompressionFormat.SmallTreeQuat32 => (4, 1),
            VectorCompressionFormat.SmallTreeQuat48 => (2, 3),
            VectorCompressionFormat.SmallTreeQuat64 => (4, 2),
            VectorCompressionFormat.PolarQuat => (2, 3),
            VectorCompressionFormat.SmallTreeQuat64Ext => (4, 2),
            VectorCompressionFormat.NoCompress => throw new ArgumentOutOfRangeException(nameof(format), format, null),
            VectorCompressionFormat.NoCompressVec3 => throw new ArgumentOutOfRangeException(nameof(format), format, null),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
        };

    public static ControllerKeyRotation FromArray(Quaternion[] data) {
        var r = new ControllerKeyRotation {
            RawData = new byte[6 * data.Length],
            Format = VectorCompressionFormat.SmallTreeQuat48
        };
        for (var i = 0; i< data.Length;i++)
            r.RawData.AsSpan().SetNativeStruct((SmallTreeQuat48) data[i], i);
        return r;
    }
}
