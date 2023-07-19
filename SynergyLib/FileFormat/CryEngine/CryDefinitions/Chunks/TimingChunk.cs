using System.Text;
using SynergyLib.Util.BinaryRW;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Chunks;

public class TimingChunk : ICryChunk {
    public ChunkHeader Header { get; set; } = new();
    public float SecsPerTick;
    public int TicksPerFrame;
    public string RangeName = string.Empty;
    public int RangeStart;
    public int RangeEnd;

    public void ReadFrom(NativeReader reader, int expectedSize) {
        var expectedEnd = reader.BaseStream.Position + expectedSize;
        Header = new(reader);
        using (reader.ScopedBigEndian(Header.IsBigEndian)) {
            reader.ReadInto(out SecsPerTick);
            reader.ReadInto(out TicksPerFrame);
            RangeName = reader.ReadFString(32, Encoding.UTF8);
            reader.ReadInto(out RangeStart);
            reader.ReadInto(out RangeEnd);
            reader.EnsureZeroesOrThrow(4);
        }

        reader.EnsurePositionOrThrow(expectedEnd);
    }

    public void WriteTo(NativeWriter writer, bool useBigEndian) {
        Header.WriteTo(writer, false);
        using (writer.ScopedBigEndian(useBigEndian)) {
            writer.Write(SecsPerTick);
            writer.Write(TicksPerFrame);
            writer.WriteFString(RangeName, 32, Encoding.UTF8);
            writer.Write(RangeStart);
            writer.Write(RangeEnd);
            writer.FillZeroes(4);
        }
    }

    public int WrittenSize => Header.WrittenSize + 52;

    public override string ToString() => $"{nameof(TimingChunk)}: {Header}";
}
