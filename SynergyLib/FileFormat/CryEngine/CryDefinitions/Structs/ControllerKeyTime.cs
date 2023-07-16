using System;
using System.IO;
using System.Linq;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Enums;
using SynergyLib.Util.BinaryRW;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;

public class ControllerKeyTime {
    public KeyTimesFormat Format;
    public float[] Data = Array.Empty<float>();

    public void ReadFrom(NativeReader b, KeyTimesFormat format, int length) {
        float[] data;
        switch (format) {
            case KeyTimesFormat.F32:
                data = new float[length];
                b.ReadIntoSpan(data.AsSpan());
                break;
            case KeyTimesFormat.UInt16:
                data = new float[length];
                for (var i = 0; i < length; i++)
                    data[i] = b.ReadUInt16();
                break;
            case KeyTimesFormat.Byte:
                data = new float[length];
                for (var i = 0; i < length; i++)
                    data[i] = b.ReadByte();
                break;
            case KeyTimesFormat.F32StartStop:
            case KeyTimesFormat.UInt16StartStop:
            case KeyTimesFormat.ByteStartStop:
                throw new InvalidDataException();
            case KeyTimesFormat.Bitset: {
                var start = b.ReadUInt16();
                var end = b.ReadUInt16();
                var size = b.ReadUInt16();

                data = new float[size];
                var ptr = 0;
                var keyValue = start;
                for (var i = 3; i < length; i++) {
                    var curr = b.ReadUInt16();
                    for (var j = 0; j < 16; ++j) {
                        if (((curr >> j) & 1) != 0 && ptr++ < data.Length)
                            data[ptr - 1] = keyValue;
                        ++keyValue;
                    }
                }

                if (ptr != size)
                    throw new InvalidDataException($"eBitset: Expected {size} items, got {ptr} items");
                if (data.Any() && Math.Abs(data[^1] - end) > float.Epsilon)
                    throw new InvalidDataException($"eBitset: Expected last as {end}, got {data[^1]}");
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(format), format, null);
        }

        for (var i = 1; i < data.Length; i++)
            if (data[i - 1] > data[i])
                throw new InvalidDataException();

        Format = format;
        Data = data;
    }

    public void WriteTo(NativeWriter w) {
        switch (Format) {
            case KeyTimesFormat.F32:
                foreach (var f in Data)
                    w.Write(f);
                break;
            case KeyTimesFormat.UInt16:
                foreach (var f in Data)
                    w.Write((ushort) f);
                break;
            case KeyTimesFormat.Byte:
                foreach (var f in Data)
                    w.Write((byte) f);
                break;
            case KeyTimesFormat.F32StartStop:
            case KeyTimesFormat.UInt16StartStop:
            case KeyTimesFormat.ByteStartStop:
                throw new NotSupportedException();
            case KeyTimesFormat.Bitset: {
                w.Write((ushort) Data[0]);
                w.Write((ushort) Data[^1]);
                w.Write(checked((ushort) Data.Length));

                Span<ushort> tmp = stackalloc ushort[1 + ((ushort) Data[^1] - (ushort) Data[0]) / 16];
                foreach (var f in Data) {
                    var index = Math.DivRem((int) f - (int) Data[0], 16, out var shift);
                    tmp[index] |= (ushort) (1 << shift);
                }

                foreach (var t in tmp)
                    w.Write(t);

                break;
            }
            default:
                throw new InvalidOperationException();
        }
    }

    public int WrittenSize => Format switch {
        KeyTimesFormat.F32 => LengthMarker * 4,
        KeyTimesFormat.UInt16 => LengthMarker * 2,
        KeyTimesFormat.Byte => LengthMarker * 1,
        KeyTimesFormat.F32StartStop => throw new NotSupportedException(),
        KeyTimesFormat.UInt16StartStop => throw new NotSupportedException(),
        KeyTimesFormat.ByteStartStop => throw new NotSupportedException(),
        KeyTimesFormat.Bitset => LengthMarker * 2,
        _ => throw new ArgumentOutOfRangeException(nameof(Format), Format, null),
    };

    public int LengthMarker => Format switch {
        KeyTimesFormat.F32 => Data.Length,
        KeyTimesFormat.UInt16 => Data.Length,
        KeyTimesFormat.Byte => Data.Length,
        KeyTimesFormat.F32StartStop => throw new NotSupportedException(),
        KeyTimesFormat.UInt16StartStop => throw new NotSupportedException(),
        KeyTimesFormat.ByteStartStop => throw new NotSupportedException(),
        KeyTimesFormat.Bitset when Data.Length >= 2 => 4 + ((ushort) Data[^1] - (ushort) Data[0]) / 16,
        _ => throw new ArgumentOutOfRangeException(nameof(Format), Format, null),
    };

    public override string ToString() => Data.Length < 2
        ? $"{nameof(ControllerKeyTime)}<{Format}>: empty"
        : $"{nameof(ControllerKeyTime)}<{Format}>: {Data[0]}..{Data[^1]} ({Data.Length} frames)";
}
