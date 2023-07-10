using System;
using WiiUStreamTool.Util.BinaryRW;

namespace WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Chunks;

public struct ChunkHeader : ICryReadWrite {
    public ChunkType Type;
    public uint VersionRaw;
    public int Offset;
    public int Id;

    public void ReadFrom(NativeReader reader, int expectedSize) {
        var expectedEnd = reader.BaseStream.Position + expectedSize;
        using (reader.ScopedLittleEndian()) {
            reader.ReadInto(out Type);
            reader.ReadInto(out VersionRaw);
            reader.ReadInto(out Offset);
            reader.ReadInto(out Id);
        }

        reader.EnsurePositionOrThrow(expectedEnd);
    }

    public void WriteTo(NativeWriter writer, bool useBigEndian) {
        writer.WriteEnum(Type);
        writer.Write(VersionRaw);
        writer.Write(Offset);
        writer.Write(Id);
    }

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
