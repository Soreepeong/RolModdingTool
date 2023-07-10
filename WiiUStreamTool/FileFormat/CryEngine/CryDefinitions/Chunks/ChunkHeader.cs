using System;
using System.IO;
using WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Enums;
using WiiUStreamTool.Util.BinaryRW;

namespace WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Chunks;

public struct ChunkHeader : ICryReadWrite {
    public ChunkType Type;
    public uint VersionRaw;
    public int Offset;
    public int Id;

    public ChunkHeader() { }

    public ChunkHeader(NativeReader reader) => ReadFrom(reader, 16);

    public void ReadFrom(NativeReader reader, int expectedSize) {
        if (expectedSize != 16)
            throw new IOException();

        using (reader.ScopedLittleEndian()) {
            reader.ReadInto(out Type);
            reader.ReadInto(out VersionRaw);
            reader.ReadInto(out Offset);
            reader.ReadInto(out Id);
        }
    }

    public readonly void WriteTo(NativeWriter writer, bool useBigEndian) {
        using (writer.ScopedLittleEndian()) {
            writer.WriteEnum(Type);
            writer.Write(VersionRaw);
            writer.Write(Offset);
            writer.Write(Id);
        }
    }

    public int WrittenSize => 16;

    public bool IsBigEndian {
        get => (VersionRaw & 0x80000000u) != 0;
        set => VersionRaw = (VersionRaw & 0x7ffffffu) | (value ? 0x80000000u : 0u);
    }

    public int Version {
        get => unchecked((int) (VersionRaw & 0x7fffffff));
        set => VersionRaw = 0 == (value & 0x80000000u)
            ? ((VersionRaw & 0x8000000u) | unchecked((uint) value))
            : throw new ArgumentOutOfRangeException(nameof(value), value, null);
    }

    public override string ToString() => $"#{Id} {Type} v{Version:X} {(IsBigEndian ? "BE" : "LE")}";
}
