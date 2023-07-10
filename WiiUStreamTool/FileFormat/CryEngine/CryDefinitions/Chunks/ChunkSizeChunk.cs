using System;
using System.Runtime.CompilerServices;
using WiiUStreamTool.Util.BinaryRW;

namespace WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Chunks;

public struct ChunkSizeChunk : ICryReadWrite {
    public ChunkHeader Header;
    public int Size;

    public void ReadFrom(NativeReader reader, int expectedSize) {
        var expectedEnd = reader.BaseStream.Position + expectedSize;
        using (reader.ScopedLittleEndian()) {
            Header.ReadFrom(reader, Unsafe.SizeOf<ChunkHeader>());
            reader.ReadInto(out Size);
        }
        
        reader.EnsurePositionOrThrow(expectedEnd);
    }

    public void WriteTo(NativeWriter writer, bool useBigEndian) {
        Header.WriteTo(writer, useBigEndian);
        writer.Write(Size);
        throw new NotImplementedException();
    }

    public override string ToString() => $"Header: {Header}";
}
