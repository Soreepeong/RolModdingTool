namespace WiiUStreamTool.Util.BinaryRW; 

public interface ICryReadWrite {
    public void ReadFrom(NativeReader reader, int expectedSize);
    public void WriteTo(NativeWriter writer, bool useBigEndian);
}
