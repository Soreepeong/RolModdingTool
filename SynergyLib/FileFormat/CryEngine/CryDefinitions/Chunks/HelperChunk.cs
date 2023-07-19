using System.Numerics;
using SynergyLib.Util.BinaryRW;
using SynergyLib.Util.MathExtras;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Chunks;

public class HelperChunk : ICryChunk {
    public ChunkHeader Header { get; set; } = new();
    public HelperType Type;
    public Vector3 Size;

    public void ReadFrom(NativeReader reader, int expectedSize) {
        var expectedEnd = reader.BaseStream.Position + expectedSize;
        Header = new(reader);
        using (reader.ScopedBigEndian(Header.IsBigEndian)) {
            reader.ReadInto(out Type);
            Size = reader.ReadVector3();
        }

        reader.EnsurePositionOrThrow(expectedEnd);
    }

    public void WriteTo(NativeWriter writer, bool useBigEndian) {
        Header.WriteTo(writer, false);
        using (writer.ScopedBigEndian(useBigEndian)) {
            writer.Write((int) Type);
            writer.Write(Size);
        }
    }

    public int WrittenSize => Header.WrittenSize + 16;

    public override string ToString() => $"{nameof(HelperChunk)}: {Header}";
}