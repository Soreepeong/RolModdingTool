using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using WiiUStreamTool.Util.BinaryRW;
using WiiUStreamTool.Util.MathExtras;

namespace WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Chunks;

public struct CompiledPhysicalProxyChunk : ICryReadWrite {
    public ChunkHeader Header;
    public readonly List<CompiledPhysicalProxy> Proxies = new();

    public CompiledPhysicalProxyChunk() { }

    public void ReadFrom(NativeReader reader, int expectedSize) {
        var expectedEnd = reader.BaseStream.Position + expectedSize;
        Header.ReadFrom(reader, Unsafe.SizeOf<ChunkHeader>());
        using (reader.ScopedBigEndian(Header.IsBigEndian)) {
            reader.ReadInto(out int count);
            Proxies.Clear();
            Proxies.EnsureCapacity(count);

            var proxy = new CompiledPhysicalProxy();
            for (var i = 0; i < count; i++) {
                proxy.ChunkId = reader.ReadUInt32();
                proxy.Vertices = new Vector3[reader.ReadInt32()];
                proxy.Indices = new ushort[reader.ReadInt32()];
                proxy.Materials = new byte[reader.ReadInt32()];
                for (var j = 0; j < proxy.Vertices.Length; j++)
                    proxy.Vertices[j] = reader.ReadVector3();
                for (var j = 0; j < proxy.Indices.Length; j++)
                    proxy.Indices[j] = reader.ReadUInt16();
                for (var j = 0; j < proxy.Materials.Length; j++)
                    proxy.Materials[j] = reader.ReadByte();
                Proxies.Add(proxy);
            }
        }

        reader.EnsurePositionOrThrow(expectedEnd);
    }

    public void WriteTo(NativeWriter writer, bool useBigEndian) {
        throw new NotImplementedException();
    }

    public override string ToString() => $"{nameof(CompiledPhysicalProxyChunk)}: {Header}";
}