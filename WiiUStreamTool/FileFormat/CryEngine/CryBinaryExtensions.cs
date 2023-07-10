﻿using System;
using System.IO;
using WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Structs;
using WiiUStreamTool.Util;

namespace WiiUStreamTool.FileFormat.CryEngine;

public static class CryBinaryExtensions {
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

    public static ShortInt3Quat ReadShortInt3Quat(this BinaryReader r) =>
        new() {
            X = r.ReadInt16(),
            Y = r.ReadInt16(),
            Z = r.ReadInt16(),
        };

    public static SmallTreeQuat32 ReadSmallTreeQuat32(this BinaryReader r) => new() {
            Value = r.ReadUInt32(),
        };

    public static SmallTreeQuat48 ReadSmallTreeQuat48(this BinaryReader r) => new() {
            M1 = r.ReadUInt16(),
            M2 = r.ReadUInt16(),
            M3 = r.ReadUInt16(),
        };

    public static SmallTreeQuat64 ReadSmallTreeQuat64(this BinaryReader r) => new() {
            M1 = r.ReadUInt32(),
            M2 = r.ReadUInt32(),
        };

    public static SmallTreeQuat64Ext ReadSmallTreeQuat64Ext(this BinaryReader r) => new() {
            M1 = r.ReadUInt32(),
            M2 = r.ReadUInt32(),
        };

    public static PolarQuat ReadPolarQuat(this BinaryReader r) => new() {
            Yaw = r.ReadInt16(),
            Pitch = r.ReadInt16(),
            W = r.ReadInt16(),
        };
}
