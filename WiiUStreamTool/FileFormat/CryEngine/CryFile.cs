using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using WiiUStreamTool.FileFormat.CryEngine.CryDefinitions;
using WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Chunks;
using WiiUStreamTool.Util.BinaryRW;

namespace WiiUStreamTool.FileFormat.CryEngine;

public class CryFile {
    public static readonly ImmutableArray<byte> Magic = "CryTek\0\0"u8.ToArray().ToImmutableArray();

    public FileType Type;
    public FileVersion Version;
    public readonly Dictionary<int, object> Chunks = new();

    public CryFile() { }

    public void ReadFrom(NativeReader reader) {
        using (reader.ScopedLittleEndian()) {
            reader.EnsureMagicOrThrow(Magic.AsSpan());
            reader.ReadInto(out Type);
            if (!Enum.IsDefined(Type))
                throw new IOException("Bad FileType");
            reader.ReadInto(out Version);
            if (!Enum.IsDefined(Version))
                throw new IOException("Bad FileVersion");
            reader.ReadInto(out int chunkOffset);
            reader.ReadInto(out int chunkCount);
            reader.EnsurePositionOrThrow(chunkOffset + 4);

            Span<ChunkSizeChunk> headers = stackalloc ChunkSizeChunk[chunkCount];
            for (var i = 0; i < chunkCount; i++)
                headers[i].ReadFrom(reader, Unsafe.SizeOf<ChunkSizeChunk>());

            for (var i = 0; i < chunkCount; i++) {
                ICryReadWrite chunk = (headers[i].Header.Type, headers[i].Header.Version) switch {
                    (ChunkType.MtlName, 0x800) => new MtlNameChunk(),
                    (ChunkType.CompiledBones, 0x800) => new CompiledBonesChunk(),
                    (ChunkType.CompiledPhysicalBones, 0x800) => new CompiledPhysicalBonesChunk(),
                    (ChunkType.CompiledPhysicalProxies, 0x800) => new CompiledPhysicalProxyChunk(),
                    (ChunkType.CompiledMorphTargets, 0x800) => new CompiledMorphTargetsChunk(),
                    (ChunkType.CompiledIntSkinVertices, 0x800) => new CompiledIntSkinVerticesChunk(),
                    (ChunkType.CompiledIntFaces, 0x800) => new CompiledIntFacesChunk(),
                    (ChunkType.CompiledExt2IntMap, 0x800) => new CompiledExtToIntMapChunk(),
                    (ChunkType.BonesBoxes, 0x801) => new BonesBoxesChunk(),
                    (ChunkType.ExportFlags, 1) => new ExportFlagsChunk(),
                    (ChunkType.MeshSubsets, 0x800) => new MeshSubsetsChunk(),
                    (ChunkType.DataStream, 0x800) => new DataChunk(),
                    _ => throw new NotSupportedException(headers[i].ToString()),
                };
                reader.BaseStream.Position = headers[i].Header.Offset;
                chunk.ReadFrom(reader, headers[i].Size);
                Chunks.Add(headers[i].Header.Id, chunk);
            }
        }

        throw new NotImplementedException();
    }

    public void WriteTo(NativeWriter writer) {
        writer.Write(Magic.AsSpan());
        writer.WriteEnum(Type);
        writer.WriteEnum(Version);
        throw new NotImplementedException();
    }

    public CryFile(Stream stream, bool leaveOpen = false) {
        using var reader = new NativeReader(stream, Encoding.UTF8, leaveOpen);
    }

    public enum FileType : uint {
        Geometry = 0xFFFF0000,
        Animation = 0xFFFF0001,
    }

    public enum FileVersion : uint {
        CryTek3 = 0x745,
    }
}
