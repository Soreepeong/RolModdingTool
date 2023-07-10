using WiiUStreamTool.Util.BinaryRW;

namespace WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Chunks;

public interface ICryChunk : ICryReadWrite {
    public ChunkHeader Header { get; set; }

    public void WriteTo(NativeWriter writer) => WriteTo(writer, Header.IsBigEndian);
}
