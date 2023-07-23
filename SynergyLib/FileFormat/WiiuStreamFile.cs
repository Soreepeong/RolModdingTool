using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;
using SynergyLib.FileFormat.CryEngine;
using SynergyLib.Util;
using SynergyLib.Util.BinaryRW;

namespace SynergyLib.FileFormat;

public class WiiuStreamFile {
    public const string MetadataFilename = "_WiiUStreamMetadata.txt";
    public static readonly ImmutableArray<byte> Magic = "strm"u8.ToArray().ToImmutableArray();

    public readonly List<FileEntry> Entries = new();

    public FileEntry GetEntry(string path, SkinFlag lookupSkinFlag) =>
        TryGetEntry(out var entry, path, lookupSkinFlag) ? entry : throw new FileNotFoundException();

    public bool TryGetEntry([NotNullWhen(true)] out FileEntry? entry, string path, SkinFlag lookupSkinFlag) {
        path = path.Replace('\\', '/');
        entry = Entries.FirstOrDefault(
            x => string.Compare(x.Header.InnerPath, path, StringComparison.OrdinalIgnoreCase) == 0
                && x.Header.SkinFlag.MatchesLookup(lookupSkinFlag));
        return entry is not null;
    }

    public FileEntry GetEntry(string path, bool useAltSkin) =>
        GetEntry(path, useAltSkin ? SkinFlag.LookupAlt : SkinFlag.LookupDefault);

    public bool TryGetEntry([NotNullWhen(true)] out FileEntry? entry, string path, bool useAltSkin) =>
        TryGetEntry(out entry, path, useAltSkin ? SkinFlag.LookupAlt : SkinFlag.LookupDefault);

    public Func<string, CancellationToken, Task<Stream>> AsFunc(SkinFlag lookupSkinFlag) =>
        (path, cancellationToken) => !TryGetEntry(out var entry, path, lookupSkinFlag)
            ? Task.FromException<Stream>(new FileNotFoundException())
            : Task.FromResult<Stream>(new MemoryStream(entry.Source.ReadRaw(cancellationToken)));

    public void PutEntry(int position, string path, FileEntrySource source, SkinFlag skinFlag = SkinFlag.Default) {
        path = path.Replace('\\', '/');
        var entry = Entries.SingleOrDefault(
            x => string.Compare(x.Header.InnerPath, path, StringComparison.OrdinalIgnoreCase) == 0
                && x.Header.SkinFlag == skinFlag);
        if (entry is not null) {
            entry.Source = source;
            return;
        }

        entry = new(
            new() {
                InnerPath = path,
                SkinFlag = skinFlag,
                Unknown = Entries.First().Header.Unknown,
            },
            source);

        if (position < 0)
            position += Entries.Count;
        Entries.Insert(position, entry);
    }

    public async Task ReadFromMetadata(Stream stream, string basePath, CancellationToken cancellationToken = default) {
        using var s = new StreamReader(stream, Encoding.UTF8, true, -1, true);
        var hdr = new FileEntryHeader();
        while (true) {
            var l = await s.ReadLineAsync(cancellationToken);
            if (string.IsNullOrEmpty(l))
                break;

            hdr.ReadFromLine(l);
            var path = Path.Join(basePath, hdr.LocalPath);
            Entries.Add(
                new(
                    hdr,
                    new() {
                        SourceType = FileEntrySourceType.RawFile,
                        Path = path,
                        Offset = 0,
                        RawLength = checked((int) new FileInfo(path).Length),
                    }));
        }
    }

    public async Task WriteMetadata(Stream stream) {
        await using var metadata = new StreamWriter(stream, new UTF8Encoding(), -1, true);
        foreach (var entry in Entries)
            await metadata.WriteLineAsync(entry.Header.ToLine());
    }

    public void ReadFrom(Stream? stream, string? path, CancellationToken cancellationToken = default) {
        Entries.Clear();

        using var reader = new NativeReader(
            stream ?? (path is null
                ? throw new ArgumentException("path or reader must be provided")
                : File.OpenRead(path)),
            Encoding.UTF8,
            stream is not null);

        using var readerBigEndian = reader.ScopedBigEndian();
        if (!Magic.SequenceEqual(reader.ReadBytes(4)))
            throw new InvalidDataException("Given file does not have the correct magic value.");

        while (reader.BaseStream.Position < reader.BaseStream.Length) {
            cancellationToken.ThrowIfCancellationRequested();

            Entries.Add(new(reader, path));
        }
    }

    public async IAsyncEnumerable<(int progress, int max, FileEntry entry, bool entryComplete)> WriteTo(
        Stream stream,
        SaveConfig saveConfig = default,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {
        saveConfig.CompressionLevel = saveConfig.CompressionLevel == SaveConfig.CompressionLevelAuto
            ? saveConfig.CompressionChunkSize == 0
                ? SaveConfig.CompressionLevelIfAuto
                : saveConfig.CompressionChunkSize
            : saveConfig.CompressionLevel;

        await using var writer = new NativeWriter(stream, Encoding.UTF8, true) {IsBigEndian = true};
        writer.Write(Magic.AsSpan());

        using var rawStream = new MemoryStream();
        using var compressionBuffer = new MemoryStream();
        await using var compressionBufferWriter = new BinaryWriter(compressionBuffer);
        for (var i = 0; i < Entries.Count; i++) {
            cancellationToken.ThrowIfCancellationRequested();

            var entry = Entries[i];
            yield return (progress: i, max: Entries.Count, entry, entryComplete: false);

            if (saveConfig.SkipAlreadyCompressed && entry.Source.IsCompressed) {
                entry.Header.DecompressedSize = entry.Source.RawLength;
                entry.Header.CompressedSize = entry.Source.StoredLength;
                entry.Header.Hash = entry.Source.Hash;

                entry.Header.WriteTo(writer);
                entry.Source.ReadCompressedInto(stream);
            } else {
                rawStream.SetLength(entry.Source.RawLength);
                rawStream.Position = 0;
                entry.Source.ReadRawInto(rawStream, cancellationToken);

                var raw = rawStream.GetBuffer().AsMemory(0, checked((int) rawStream.Length));

                if (!saveConfig.PreserveXml && raw.StartsWith("<?xml"u8)) {
                    compressionBuffer.SetLength(compressionBuffer.Position = 0);
                    rawStream.Position = 0;
                    PbxmlFile.FromStream(rawStream).WriteBinary(compressionBufferWriter, true);
                    rawStream.SetLength(compressionBuffer.Length);
                    rawStream.Position = 0;
                    compressionBuffer.Position = 0;
                    await compressionBuffer.CopyToAsync(rawStream, cancellationToken);
                }

                var useCompression = saveConfig.CompressionLevel > 0;
                if (saveConfig.CompressionLevel > 0) {
                    compressionBuffer.SetLength(0);
                    compressionBuffer.Position = 0;
                    await CompressOne(
                        raw,
                        compressionBuffer,
                        saveConfig.CompressionLevel,
                        saveConfig.CompressionChunkSize,
                        cancellationToken);
                    compressionBuffer.Position = 0;

                    useCompression = compressionBuffer.Length < raw.Length;
                }

                entry.Header.DecompressedSize = raw.Length;
                if (useCompression) {
                    entry.Header.CompressedSize = checked((int) compressionBuffer.Length);
                    entry.Header.Hash = Crc32.Brb.Get(compressionBuffer.GetBuffer(), 0, entry.Header.CompressedSize);
                } else {
                    entry.Header.CompressedSize = 0;
                    entry.Header.Hash = Crc32.Brb.Get(raw.Span);
                }

                entry.Header.WriteTo(writer);
                await writer.BaseStream.WriteAsync(
                    useCompression ? compressionBuffer.GetBuffer().AsMemory(0, entry.Header.CompressedSize) : raw,
                    cancellationToken);
            }

            yield return (progress: i, max: Entries.Count, entry, entryComplete: true);
        }
    }

    public static async Task CompressOne(
        Memory<byte> raw,
        Stream target,
        int level,
        int chunkSize,
        CancellationToken cancellationToken) {
        if (level == SaveConfig.CompressionLevelAuto)
            level = SaveConfig.CompressionLevelIfAuto;

        // Is chunking disabled, or is the file small enough that there is no point in multithreading?
        if (chunkSize <= 0 || raw.Length <= chunkSize) {
            CompressChunk(raw.Span, target, level, cancellationToken);
            return;
        }

        var pool = ObjectPool.Create(new DefaultPooledObjectPolicy<MemoryStream>());

        Task<MemoryStream> DoChunk(int offset, int length) => Task.Run(
            () => {
                var ms = pool.Get();
                ms.SetLength(ms.Position = 0);
                CompressChunk(raw.Span.Slice(offset, length), ms, level, cancellationToken);
                return ms;
            },
            cancellationToken);

        var concurrency = Environment.ProcessorCount;
        var tasks = new List<Task<MemoryStream>>(Math.Min((raw.Length + chunkSize - 1) / chunkSize, concurrency));

        var runningTasks = new HashSet<Task<MemoryStream>>();
        for (var i = 0; i < raw.Length; i += chunkSize) {
            if (runningTasks.Count >= concurrency) {
                await Task.WhenAny(runningTasks);
                runningTasks.RemoveWhere(x => x.IsCompleted);
            }

            while (tasks.FirstOrDefault()?.IsCompleted is true) {
                var result = tasks[0].Result;
                tasks.RemoveAt(0);
                result.Position = 0;
                await result.CopyToAsync(target, cancellationToken);
            }

            tasks.Add(DoChunk(i, Math.Min(chunkSize, raw.Length - i)));
            runningTasks.Add(tasks.Last());
        }

        foreach (var task in tasks) {
            var result = await task;
            result.Position = 0;
            await result.CopyToAsync(target, cancellationToken);
        }
    }

    public static void CompressChunk(Span<byte> source, Stream target, int level, CancellationToken cancellationToken) {
        if (level == SaveConfig.CompressionLevelAuto)
            level = SaveConfig.CompressionLevelIfAuto;

        var asisBegin = 0;
        var asisLen = 0;

        Span<int> lastByteIndex = stackalloc int[0x100];
        lastByteIndex.Fill(-1);
        var prevSameByteIndex = new int[source.Length];

        for (var i = 0; i < source.Length;) {
            cancellationToken.ThrowIfCancellationRequested();

            var lookbackOffset = 1;
            var maxRepeatedSequenceLength = 0;

            for (int index = lastByteIndex[source[i]], remaining = level;
                 index != -1 && remaining > 0;
                 index = prevSameByteIndex[index], remaining--) {
                var lookbackLength = i - index;
                var compareTo = source.Slice(index, lookbackLength);

                var repeatedSequenceLength = 0;
                for (var s = source[i..]; !s.IsEmpty; s = s[lookbackLength..]) {
                    var len = compareTo.CommonPrefixLength(s);
                    repeatedSequenceLength += len;
                    if (len < lookbackLength)
                        break;
                }

                if (repeatedSequenceLength >= maxRepeatedSequenceLength) {
                    maxRepeatedSequenceLength = repeatedSequenceLength;
                    lookbackOffset = lookbackLength;
                }
            }

            if (maxRepeatedSequenceLength >= 3 &&
                maxRepeatedSequenceLength >=
                (asisLen == 0 ? 0 : CrySerializationExtensions.CountCryIntBytes(asisLen, false)) +
                CrySerializationExtensions.CountCryIntBytes(maxRepeatedSequenceLength - 3, true) +
                CrySerializationExtensions.CountCryIntBytes(lookbackOffset, false)) {
                if (asisLen != 0) {
                    target.WriteCryIntWithFlag(asisLen, false);
                    target.Write(source.Slice(asisBegin, asisLen));
                }

                target.WriteCryIntWithFlag(maxRepeatedSequenceLength - 3, true);
                target.WriteCryInt(lookbackOffset);

                while (maxRepeatedSequenceLength-- > 0) {
                    prevSameByteIndex[i] = lastByteIndex[source[i]];
                    lastByteIndex[source[i]] = i;

                    i++;
                }

                asisBegin = i;
                asisLen = 0;
            } else {
                prevSameByteIndex[i] = lastByteIndex[source[i]];
                lastByteIndex[source[i]] = i;

                asisLen++;
                i++;
            }
        }

        if (asisLen != 0) {
            target.WriteCryIntWithFlag(asisLen, false);
            target.Write(source.Slice(asisBegin, asisLen));
        }
    }

    public static void DecompressOne(
        Stream source,
        Stream target,
        int decompressedSize,
        int compressedSize,
        CancellationToken cancellationToken) {
        if (compressedSize == 0) {
            source.CopyToLength(target, decompressedSize, cancellationToken);
        } else {
            var endOffset = source.Position + compressedSize;
            var buffer = new byte[decompressedSize];
            using var ms = new MemoryStream(buffer, true);
            while (ms.Position < buffer.Length && source.Position < endOffset) {
                cancellationToken.ThrowIfCancellationRequested();

                var size = source.ReadCryIntWithFlag(out var backFlag);

                if (backFlag) {
                    var copyLength = source.ReadCryInt();
                    var copyBaseOffset = (int) ms.Position - copyLength;

                    for (var remaining = size + 3; remaining > 0; remaining -= copyLength)
                        ms.Write(buffer, copyBaseOffset, Math.Min(copyLength, remaining));
                } else {
                    if (source.Read(buffer, (int) ms.Position, size) != size)
                        throw new EndOfStreamException();
                    ms.Position += size;
                }
            }

            if (ms.Position != buffer.Length)
                throw new EndOfStreamException();
            target.Write(buffer);
        }
    }

    public static WiiuStreamFile FromFile(string path, CancellationToken cancellationToken = default) {
        var s = new WiiuStreamFile();
        s.ReadFrom(null, path, cancellationToken);
        return s;
    }

    public struct FileEntryHeader {
        public int CompressedSize;
        public int DecompressedSize;
        public uint Hash;
        public ushort Unknown;
        public SkinFlag SkinFlag;
        public string InnerPath;

        public bool IsCompressed => CompressedSize != 0;

        public int WrittenDataSize => IsCompressed ? CompressedSize : DecompressedSize;

        public string LocalPath => SkinFlag.TransformPath(InnerPath).Replace("..", "__");

        public void ReadFrom(NativeReader reader) {
            CompressedSize = reader.ReadInt32();
            DecompressedSize = reader.ReadInt32();
            Hash = reader.ReadUInt32();
            Unknown = reader.ReadUInt16();
            SkinFlag = (SkinFlag) reader.ReadUInt16();
            InnerPath = reader.ReadCString();
        }

        public readonly void WriteTo(NativeWriter writer) {
            writer.Write(CompressedSize);
            writer.Write(DecompressedSize);
            writer.Write(Hash);
            writer.Write(Unknown);
            writer.Write((ushort) SkinFlag);
            writer.WriteCString(InnerPath);
        }

        public readonly string ToLine() => $"{InnerPath};{SkinFlag};{Unknown}";

        public void ReadFromLine(string line) {
            var s = line.Split(";");
            Unknown = ushort.Parse(s[2].Trim());
            SkinFlag = Enum.Parse<SkinFlag>(s[1].Trim());
            InnerPath = s[0].Trim();
        }
    }

    public struct FileEntrySource {
        public FileEntrySourceType SourceType;
        public int Offset;
        public int RawLength;
        public int StoredLength;
        public uint Hash;
        public string? Path;
        public byte[]? Data;

        public bool IsCompressed =>
            SourceType is FileEntrySourceType.CompressedBytes or FileEntrySourceType.CompressedFile;

        public FileEntrySource() { }

        public FileEntrySource(byte[] data) {
            SourceType = FileEntrySourceType.RawBytes;
            Offset = 0;
            RawLength = StoredLength = data.Length;
            Data = data;
            Hash = Crc32.Brb.Get(data);
        }

        public FileEntrySource(byte[] data, int length) {
            if (length > data.Length)
                throw new ArgumentOutOfRangeException(nameof(length), length, null);
            SourceType = FileEntrySourceType.RawBytes;
            Offset = 0;
            RawLength = StoredLength = length;
            Data = data;
            Hash = Crc32.Brb.Get(data);
        }

        public readonly async Task<FileEntrySource> ToCompressed(
            int level,
            int chunkSize,
            CancellationToken cancellationToken) {
            var raw = SourceType == FileEntrySourceType.RawBytes
                ? Data?.AsMemory(0, RawLength) ?? throw new InvalidOperationException()
                : ReadRaw(cancellationToken).AsMemory();

            using var compressedStream = new MemoryStream();
            await CompressOne(raw, compressedStream, level, chunkSize, cancellationToken);
            if (compressedStream.Length > StoredLength)
                return this;

            var compressedData = compressedStream.ToArray();
            return new() {
                SourceType = FileEntrySourceType.CompressedBytes,
                RawLength = raw.Length,
                StoredLength = compressedData.Length,
                Hash = Crc32.Brb.Get(compressedData.AsSpan()),
                Data = compressedData,
            };
        }

        public readonly Stream GetRawStream(CancellationToken cancellationToken) =>
            SourceType == FileEntrySourceType.RawFile
                ? File.OpenRead(Path!)
                : new MemoryStream(ReadRaw(cancellationToken));

        public readonly byte[] ReadRaw(CancellationToken cancellationToken) {
            var buf = new byte[RawLength];
            using var ms = new MemoryStream(buf);
            ReadRawInto(ms, cancellationToken);
            return buf;
        }

        public readonly void ReadRawInto(Stream into, CancellationToken cancellationToken) {
            switch (SourceType) {
                case FileEntrySourceType.RawFile: {
                    using var f = File.OpenRead(Path!);
                    f.Position = Offset;

                    Span<byte> buffer = stackalloc byte[4096];
                    for (var remaining = RawLength; remaining > 0; remaining -= buffer.Length) {
                        cancellationToken.ThrowIfCancellationRequested();
                        buffer = buffer[..Math.Min(buffer.Length, remaining)];
                        f.ReadExactly(buffer);
                        into.Write(buffer);
                    }

                    return;
                }
                case FileEntrySourceType.RawBytes:
                    if (into.Length != RawLength)
                        throw new ArgumentException(null, nameof(into));
                    into.Write(Data!, 0, RawLength);
                    return;
                case FileEntrySourceType.CompressedBytes:
                    DecompressOne(new MemoryStream(Data!), into, RawLength, StoredLength, cancellationToken);
                    return;
                case FileEntrySourceType.CompressedFile: {
                    using var f = File.OpenRead(Path!);
                    f.Position = Offset;
                    DecompressOne(f, into, RawLength, StoredLength, cancellationToken);
                    return;
                }
                default:
                    throw new InvalidOperationException();
            }
        }

        public readonly void ReadCompressedInto(Stream into) {
            switch (SourceType) {
                case FileEntrySourceType.CompressedBytes:
                    into.Write(Data!, 0, StoredLength);
                    return;
                case FileEntrySourceType.CompressedFile: {
                    using var f = File.OpenRead(Path!);
                    f.Position = Offset;

                    Span<byte> buffer = stackalloc byte[4096];
                    for (var remaining = StoredLength; remaining > 0; remaining -= buffer.Length) {
                        buffer = buffer[..Math.Min(buffer.Length, remaining)];
                        f.ReadExactly(buffer);
                        into.Write(buffer);
                    }

                    return;
                }
                case FileEntrySourceType.RawFile:
                case FileEntrySourceType.RawBytes:
                default:
                    throw new InvalidOperationException();
            }
        }
    }

    public enum FileEntrySourceType {
        RawFile,
        CompressedFile,
        RawBytes,
        CompressedBytes,
    }

    public class FileEntry {
        public FileEntrySource Source;
        public FileEntryHeader Header;

        public FileEntry(FileEntryHeader header, FileEntrySource source) {
            Header = header;
            Source = source;
        }

        public FileEntry(NativeReader reader, string? path) {
            Header.ReadFrom(reader);
            if (Header.DecompressedSize < 4096 || path is null)
                Source = new() {
                    SourceType = Header.IsCompressed
                        ? FileEntrySourceType.CompressedBytes
                        : FileEntrySourceType.RawBytes,
                    Data = reader.ReadBytes(Header.WrittenDataSize),
                    Hash = Header.Hash,
                    RawLength = Header.DecompressedSize,
                    StoredLength = Header.WrittenDataSize,
                };
            else {
                Source = new() {
                    SourceType = Header.IsCompressed
                        ? FileEntrySourceType.CompressedFile
                        : FileEntrySourceType.RawFile,
                    Offset = checked((int) reader.BaseStream.Position),
                    Path = path,
                    Hash = Header.Hash,
                    RawLength = Header.DecompressedSize,
                    StoredLength = Header.WrittenDataSize,
                };
                reader.BaseStream.Position += Header.WrittenDataSize;
            }
        }
    }

    public struct SaveConfig {
        public const int CompressionLevelAuto = -1;
        public const int CompressionLevelIfAuto = 8;

        public bool SkipAlreadyCompressed = true;
        public bool PreserveXml = false;
        public int CompressionLevel = CompressionLevelAuto;
        public int CompressionChunkSize = 24576;

        public SaveConfig() { }
    }
}
