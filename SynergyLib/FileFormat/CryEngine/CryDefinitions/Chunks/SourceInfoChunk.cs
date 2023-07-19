using System;
using SynergyLib.Util.BinaryRW;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Chunks;

public class SourceInfoChunk : ICryChunk {
    public ChunkHeader Header { get; set; } = new();
    public byte[] Data = Array.Empty<byte>();

    public void ReadFrom(NativeReader reader, int expectedSize) {
        using (reader.ScopedBigEndian(Header.IsBigEndian)) {
            Data = reader.ReadBytes(expectedSize);
        }
    }

    public void WriteTo(NativeWriter writer, bool useBigEndian) {
        using (writer.ScopedBigEndian(useBigEndian)) {
            writer.Write(Data);
        }
    }

    public int WrittenSize => Data.Length;

    public override string ToString() => $"{nameof(SourceInfoChunk)}: {Header}";
}