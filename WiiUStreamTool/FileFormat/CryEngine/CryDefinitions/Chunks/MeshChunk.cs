using System;
using System.Runtime.CompilerServices;
using System.Text;
using WiiUStreamTool.Util.BinaryRW;

namespace WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Chunks;

public struct MeshChunk : ICryReadWrite {
    public ChunkHeader Header;
    public ExportFlags Flags;
    public uint[] RcVersion;
    public string RcVersionString;
    public int AssetAuthorTool;
    public int AuthorToolVersion;
    
    public MeshChunk() { }

    public void ReadFrom(NativeReader reader, int expectedSize) {
        var expectedEnd = reader.BaseStream.Position + expectedSize;
        Header.ReadFrom(reader, Unsafe.SizeOf<ChunkHeader>());
        using (reader.ScopedBigEndian(Header.IsBigEndian)) {
            reader.ReadInto(out Flags);
            RcVersion = new[] {reader.ReadUInt32(), reader.ReadUInt32(), reader.ReadUInt32(), reader.ReadUInt32()};
            RcVersionString = reader.ReadFString(16, Encoding.UTF8);
            reader.ReadInto(out AssetAuthorTool);
            reader.ReadInto(out AuthorToolVersion);
            reader.EnsureZeroesOrThrow(120);
        }

        reader.EnsurePositionOrThrow(expectedEnd);
    }

    public void WriteTo(NativeWriter writer, bool useBigEndian) {
        throw new NotImplementedException();
    }

    public override string ToString() => $"{nameof(MeshChunk)}: {Header}";
}