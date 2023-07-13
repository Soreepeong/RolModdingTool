using System;
using System.IO;
using System.Numerics;
using SynergyLib.Util;

namespace SynergyLib.FileFormat.CryEngine;

public static class CrySerializationExtensions {
    public static int CountCryIntBytes(int n, bool useFlag) {
        var res = 1;

        if (useFlag)
            for (; n >= 0x80; n >>= 7)
                res++;
        else
            for (; n >= 0x40; n >>= 7)
                res++;

        return res;
    }

    public static Span<byte> WriteCryInt(this Span<byte> bytes, int n) {
        var ptr = bytes.Length;
        while (n >= 0x80) {
            bytes[--ptr] = (byte) (n & 0x7F);
            n >>= 7;
        }

        bytes[--ptr] = (byte) n;
        for (var i = ptr; i < bytes.Length - 1; i++)
            bytes[i] |= 0x80;
        return bytes[ptr..];
    }

    public static Span<byte> WriteCryIntWithFlag(this Span<byte> bytes, int n, bool flag) {
        var ptr = bytes.Length;
        while (n >= 0x40) {
            bytes[--ptr] = (byte) (n & 0x7F);
            n >>= 7;
        }

        bytes[--ptr] = (byte) n;
        for (var i = ptr; i < bytes.Length - 1; i++)
            bytes[i] |= 0x80;
        if (flag)
            bytes[ptr] |= 0x40;
        return bytes[ptr..];
    }

    public static void WriteCryInt(this Stream stream, int n) {
        Span<byte> bytes = stackalloc byte[5];
        stream.Write(bytes.WriteCryInt(n));
    }

    public static void WriteCryIntWithFlag(this Stream stream, int n, bool flag) {
        Span<byte> bytes = stackalloc byte[5];
        stream.Write(bytes.WriteCryIntWithFlag(n, flag));
    }

    public static void WriteCryInt(this BinaryWriter writer, int n) => writer.BaseStream.WriteCryInt(n);

    public static void WriteCryIntWithFlag(this BinaryWriter writer, int n, bool flag) =>
        writer.BaseStream.WriteCryIntWithFlag(n, flag);

    public static int ReadCryInt(this Stream stream) {
        var current = stream.ReadByteOrThrow();
        var result = current & 0x7F;
        while ((current & 0x80) != 0) {
            current = stream.ReadByteOrThrow();
            result = (result << 7) | (current & 0x7F);
        }

        return result;
    }

    public static int ReadCryIntWithFlag(this Stream stream, out bool flag) {
        var current = stream.ReadByteOrThrow();
        var result = current & 0x3F;
        flag = (current & 0x40) != 0;
        while ((current & 0x80) != 0) {
            current = stream.ReadByteOrThrow();
            result = (result << 7) | (current & 0x7F);
        }

        return result;
    }

    public static int ReadCryInt(this BinaryReader reader) => reader.BaseStream.ReadCryInt();

    public static int ReadCryIntWithFlag(this BinaryReader reader, out bool flag) =>
        reader.BaseStream.ReadCryIntWithFlag(out flag);

    public static Vector3? XmlToVector3(this string? value) {
        if (string.IsNullOrWhiteSpace(value))
            return new();

        var parts = value.Split(',');
        if (parts.Length != 3)
            return new();

        return new(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
    }

    public static Quaternion? XmlToQuaternion(this string? value) {
        if (string.IsNullOrWhiteSpace(value))
            return new();

        var parts = value.Split(',');
        if (parts.Length != 4)
            return new();

        return new(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));
    }

    public static string? ToXmlValue(this Vector3? value) => value is { } v
        ? $"{v.X:0.########},{v.Y:0.########},{v.Z:0.########}"
        : null;

    public static string? ToXmlValue(this Quaternion? value) => value is { } v
        ? $"{v.X:0.########},{v.Y:0.########},{v.Z:0.########},{v.W:0.########}"
        : null;

    public static string ToXmlValue(this Vector3 value) =>
        $"{value.X:0.########},{value.Y:0.########},{value.Z:0.########}";

    public static string ToXmlValue(this Quaternion value) =>
        $"{value.X:0.########},{value.Y:0.########},{value.Z:0.########},{value.W:0.########}";
}
