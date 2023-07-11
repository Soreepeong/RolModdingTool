using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace WiiUStreamTool.Util.BinaryRW;

public static class BinaryWriterExtensions {
    private static readonly byte[] Zeroes = new byte[4096];

    public static unsafe void WriteEnum<T>(this BinaryWriter writer, T value) where T : unmanaged, Enum {
        switch (Marshal.SizeOf(Enum.GetUnderlyingType(typeof(T)))) {
            case 1:
                writer.Write(*(byte*) &value);
                break;
            case 2:
                writer.Write(*(ushort*) &value);
                break;
            case 4:
                writer.Write(*(uint*) &value);
                break;
            case 8:
                writer.Write(*(ulong*) &value);
                break;
            default:
                throw new ArgumentException("Enum is not of size 1, 2, 4, or 8.", nameof(T), null);
        }
    }

    public static void WriteFString(this BinaryWriter writer, string str, int length, Encoding encoding) {
        var span = encoding.GetBytes(str).AsSpan();
        if (span.Length > length)
            throw new ArgumentOutOfRangeException(nameof(str), str, "String length exceeding length");
        writer.Write(span);
        for (var i = span.Length; i < length; i++)
            writer.Write((byte) 0);
    }

    public static void WriteCString(this BinaryWriter writer, string str) {
        writer.Write(str.AsSpan());
        writer.Write((char) 0);
    }

    public static void WritePadding(this BinaryWriter writer, byte alignment, byte padWith = 0) {
        while (writer.BaseStream.Position % alignment != 0)
            writer.Write(padWith);
    }

    public static void FillZeroes(this BinaryWriter writer, int length) {
        for (var i = 0; i < length; i += Zeroes.Length)
            writer.Write(Zeroes.AsSpan(0, Math.Min(Zeroes.Length, length - i)));
    }
}
