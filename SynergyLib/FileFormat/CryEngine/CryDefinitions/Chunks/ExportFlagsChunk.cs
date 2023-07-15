using System.Text;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Enums;
using SynergyLib.Util.BinaryRW;
using SynergyLib.Util.MathExtras;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Chunks;

public class ExportFlagsChunk : ICryChunk {
    public ChunkHeader Header { get; set; } = new();
    public ExportFlags Flags;
    public Vector4<uint> RcVersion;
    public string RcVersionString = string.Empty;
    public int AssetAuthorTool;
    public int AuthorToolVersion;

    public ExportFlagsChunk() { }

    public void ReadFrom(NativeReader reader, int expectedSize) {
        var expectedEnd = reader.BaseStream.Position + expectedSize;
        Header = new(reader);
        using (reader.ScopedBigEndian(Header.IsBigEndian)) {
            reader.ReadInto(out Flags);
            RcVersion[0] = reader.ReadUInt32();
            RcVersion[1] = reader.ReadUInt32();
            RcVersion[2] = reader.ReadUInt32();
            RcVersion[3] = reader.ReadUInt32();

            RcVersionString = reader.ReadFString(16, Encoding.UTF8);
            reader.ReadInto(out AssetAuthorTool);
            reader.ReadInto(out AuthorToolVersion);
            reader.EnsureZeroesOrThrow(120);
        }

        reader.EnsurePositionOrThrow(expectedEnd);
    }

    public void WriteTo(NativeWriter writer, bool useBigEndian) {
        Header.WriteTo(writer, false);
        using (writer.ScopedBigEndian(useBigEndian)) {
            writer.WriteEnum(Flags);
            writer.Write(RcVersion[0]);
            writer.Write(RcVersion[1]);
            writer.Write(RcVersion[2]);
            writer.Write(RcVersion[3]);

            writer.WriteFString(RcVersionString, 16, Encoding.UTF8);
            writer.Write(AssetAuthorTool);
            writer.Write(AuthorToolVersion);
            writer.FillZeroes(120);
        }
    }

    public int WrittenSize => Header.WrittenSize + 164;

    public override string ToString() => $"{nameof(ExportFlagsChunk)}: {Header}";
}
