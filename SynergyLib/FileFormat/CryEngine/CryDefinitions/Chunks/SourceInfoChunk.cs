using System.Text;
using SynergyLib.Util.BinaryRW;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Chunks;

public class SourceInfoChunk : ICryChunk {
    public ChunkHeader Header { get; set; } = new();
    public string Data = string.Empty;

    public void ReadFrom(NativeReader reader, int expectedSize) {
        var expectedEnd = reader.BaseStream.Position + expectedSize;
        Header = new(reader);
        using (reader.ScopedBigEndian(Header.IsBigEndian)) {
            var xd = reader.ReadBytes((int) (expectedEnd - reader.BaseStream.Position));
            Data = Encoding.UTF8.GetString(xd);
        }

        reader.EnsurePositionOrThrow(expectedEnd);
    }

    public void WriteTo(NativeWriter writer, bool useBigEndian) {
        Header.WriteTo(writer, false);
        using (writer.ScopedBigEndian(useBigEndian)) {
            writer.Write(Encoding.UTF8.GetBytes(Data));
        }
    }

    public int WrittenSize => Header.WrittenSize + Encoding.UTF8.GetBytes(Data).Length;

    public override string ToString() => $"{nameof(SourceInfoChunk)}: {Header}";
}