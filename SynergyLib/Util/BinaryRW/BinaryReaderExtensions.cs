using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.ObjectPool;

namespace SynergyLib.Util.BinaryRW;

public static class BinaryReaderExtensions {
    private static readonly ObjectPool<StringBuilder> StringBuilderPool =
        ObjectPool.Create(new StringBuilderPooledObjectPolicy());

    public static void EnsureMagicOrThrow(
        this BinaryReader reader,
        ReadOnlySpan<byte> magic,
        string? errorMessage = null) {
        Span<byte> buf = stackalloc byte[magic.Length];
        buf = buf[..reader.Read(buf)];
        if (buf.Length != magic.Length)
            throw new IOException($"Magic is {magic.Length} b; read {buf.Length} b");

        if (!buf.SequenceEqual(magic))
            throw new IOException(errorMessage ?? "Invalid magic");
    }

    public static void EnsurePositionOrThrow(this BinaryReader reader, long offset) {
        if (reader.BaseStream.Position != offset)
            throw new IOException($"Expected position {offset:N0}; currently at {reader.BaseStream.Position:N0}");
    }

    public static void EnsureZeroesOrThrow(this BinaryReader reader, int length, string? errorMessage = null) {
        Span<byte> buf = stackalloc byte[length];
        buf = buf[..reader.Read(buf)];
        if (buf.Length != length)
            throw new IOException($"Padding is {length} b; read {buf.Length} b");

        if (buf.IndexOfAnyExcept((byte) 0) != -1)
            throw new IOException(errorMessage ?? "Padding is not empty");
    }

    public static string ReadFString(this BinaryReader reader, int length, Encoding encoding) {
        var buf = length < 4096 ? stackalloc byte[length] : new byte[length];
        buf = buf[..reader.Read(buf)];
        if (length != buf.Length)
            throw new IOException($"Incomplete read; expected {length} b, read {buf.Length}");

        var i = buf.IndexOf((byte) 0);
        if (i >= 0)
            buf = buf[..i];

        return encoding.GetString(buf);
    }

    public static string ReadCString(this BinaryReader reader) {
        var sb = StringBuilderPool.Get();
        try {
            while (true) {
                var c = reader.ReadChar();
                if (c == 0)
                    break;
                sb.Append(c);
            }

            return sb.ToString();
        } finally {
            StringBuilderPool.Return(sb);
        }
    }

    public static unsafe T ReadEnum<T>(this BinaryReader reader) where T : unmanaged, Enum {
        switch (Marshal.SizeOf(Enum.GetUnderlyingType(typeof(T)))) {
            case 1:
                var b1 = reader.ReadByte();
                return *(T*) &b1;
            case 2:
                var b2 = reader.ReadUInt16();
                return *(T*) &b2;
            case 4:
                var b4 = reader.ReadUInt32();
                return *(T*) &b4;
            case 8:
                var b8 = reader.ReadUInt64();
                return *(T*) &b8;
            default:
                throw new ArgumentException("Enum is not of size 1, 2, 4, or 8.", nameof(T), null);
        }
    }

    public static void ReadInto(this BinaryReader reader, out byte value) => value = reader.ReadByte();
    public static void ReadInto(this BinaryReader reader, out sbyte value) => value = reader.ReadSByte();
    public static void ReadInto(this BinaryReader reader, out ushort value) => value = reader.ReadUInt16();
    public static void ReadInto(this BinaryReader reader, out short value) => value = reader.ReadInt16();
    public static void ReadInto(this BinaryReader reader, out uint value) => value = reader.ReadUInt32();
    public static void ReadInto(this BinaryReader reader, out int value) => value = reader.ReadInt32();
    public static void ReadInto(this BinaryReader reader, out ulong value) => value = reader.ReadUInt64();
    public static void ReadInto(this BinaryReader reader, out long value) => value = reader.ReadInt64();
    public static void ReadInto(this BinaryReader reader, out float value) => value = reader.ReadSingle();
    public static void ReadInto(this BinaryReader reader, out double value) => value = reader.ReadDouble();

    public static void ReadInto<T>(this BinaryReader reader, out T value) where T : unmanaged, Enum
        => value = reader.ReadEnum<T>();

    public static void ReadIntoSpan(this BinaryReader reader, Span<byte> value) {
        for (var i = 0; i < value.Length; i++) reader.ReadInto(out value[i]);
    }

    public static void ReadIntoSpan(this BinaryReader reader, Span<sbyte> value) {
        for (var i = 0; i < value.Length; i++) reader.ReadInto(out value[i]);
    }

    public static void ReadIntoSpan(this BinaryReader reader, Span<ushort> value) {
        for (var i = 0; i < value.Length; i++) reader.ReadInto(out value[i]);
    }

    public static void ReadIntoSpan(this BinaryReader reader, Span<short> value) {
        for (var i = 0; i < value.Length; i++) reader.ReadInto(out value[i]);
    }

    public static void ReadIntoSpan(this BinaryReader reader, Span<uint> value) {
        for (var i = 0; i < value.Length; i++) reader.ReadInto(out value[i]);
    }

    public static void ReadIntoSpan(this BinaryReader reader, Span<int> value) {
        for (var i = 0; i < value.Length; i++) reader.ReadInto(out value[i]);
    }

    public static void ReadIntoSpan(this BinaryReader reader, Span<ulong> value) {
        for (var i = 0; i < value.Length; i++) reader.ReadInto(out value[i]);
    }

    public static void ReadIntoSpan(this BinaryReader reader, Span<long> value) {
        for (var i = 0; i < value.Length; i++) reader.ReadInto(out value[i]);
    }

    public static void ReadIntoSpan(this BinaryReader reader, Span<float> value) {
        for (var i = 0; i < value.Length; i++) reader.ReadInto(out value[i]);
    }

    public static void ReadIntoSpan(this BinaryReader reader, Span<double> value) {
        for (var i = 0; i < value.Length; i++) reader.ReadInto(out value[i]);
    }

    public static void ReadIntoSpan<T>(this BinaryReader reader, Span<T> value) where T : unmanaged, Enum {
        for (var i = 0; i < value.Length; i++)
            reader.ReadInto(out value[i]);
    }
}
