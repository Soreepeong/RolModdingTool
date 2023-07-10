using System.Collections.Generic;
using System.Linq;
using WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Structs;
using WiiUStreamTool.Util.BinaryRW;

namespace WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Chunks;

public struct CompiledPhysicalProxyChunk : ICryChunk {
    public ChunkHeader Header { get; set; }
    public readonly List<CompiledPhysicalProxy> Proxies = new();

    public CompiledPhysicalProxyChunk() { }

    public void ReadFrom(NativeReader reader, int expectedSize) {
        var expectedEnd = reader.BaseStream.Position + expectedSize;
        Header = new(reader);
        using (reader.ScopedBigEndian(Header.IsBigEndian)) {
            reader.ReadInto(out int count);
            Proxies.Clear();
            Proxies.EnsureCapacity(count);

            var proxy = new CompiledPhysicalProxy();
            for (var i = 0; i < count; i++) {
                proxy.ReadFrom(reader, -1);
                Proxies.Add(proxy);
            }
        }

        reader.EnsurePositionOrThrow(expectedEnd);
    }

    public readonly void WriteTo(NativeWriter writer, bool useBigEndian) {
        Header.WriteTo(writer, false);
        using (writer.ScopedBigEndian(useBigEndian)) {
            writer.Write(Proxies.Count);
            foreach (var proxy in Proxies)
                proxy.WriteTo(writer, useBigEndian);
        }
    }

    public int WrittenSize => Header.WrittenSize + 4 + Proxies.Sum(x => x.WrittenSize);

    public override string ToString() => $"{nameof(CompiledPhysicalProxyChunk)}: {Header}";
}
