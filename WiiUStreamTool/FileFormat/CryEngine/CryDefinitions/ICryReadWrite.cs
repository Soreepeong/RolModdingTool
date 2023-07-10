using WiiUStreamTool.Util.BinaryRW;

namespace WiiUStreamTool.FileFormat.CryEngine.CryDefinitions; 

public interface ICryReadWrite {
    public void ReadFrom(NativeReader reader, int expectedSize);
    public void WriteTo(NativeWriter writer, bool useBigEndian);
    public int WrittenSize { get; }
}
