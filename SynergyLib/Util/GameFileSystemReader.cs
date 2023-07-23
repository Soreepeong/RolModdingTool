using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SynergyLib.FileFormat;

namespace SynergyLib.Util;

public sealed class GameFileSystemReader : IAsyncDisposable, IDisposable {
    private readonly List<string> _rootPaths = new();
    private readonly Dictionary<string, Tuple<string, Task<WiiuStreamFile>>> _packfiles = new();
    private readonly List<Task<WiiuStreamFile>> _packfileLoaders = new();
    private readonly Dictionary<string, Tuple<string?, WiiuStreamFile?>> _entries = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public GameFileSystemReader() { }

    public GameFileSystemReader(params string[] rootPaths) {
        foreach (var r in rootPaths)
            WithRootDirectory(r);
    } 
    
    public GameFileSystemReader(IEnumerable<string> rootPaths) {
        foreach (var r in rootPaths)
            WithRootDirectory(r);
    }

    public void Dispose() {
        if (!_cancellationTokenSource.IsCancellationRequested) {
            _cancellationTokenSource.Cancel();
        }

        Task.WhenAll(_packfileLoaders).Wait();
    }

    public async ValueTask DisposeAsync() {
        if (!_cancellationTokenSource.IsCancellationRequested) {
            _cancellationTokenSource.Cancel();
        }

        await Task.WhenAll(_packfileLoaders);
    }

    public bool TryAddRootDirectory(string rootDirectory) {
        _cancellationTokenSource.Token.ThrowIfCancellationRequested();

        rootDirectory = Path.GetFullPath(rootDirectory.TrimEnd('\\', '/'));

        var basePath = Path.Join(rootDirectory, "Sonic_Crytek");
        if (!Directory.Exists(basePath))
            basePath = Path.Join(rootDirectory, "content", "Sonic_Crytek");
        if (!Directory.Exists(basePath) && Path.GetFileName(rootDirectory).ToLowerInvariant() == "sonic_crytek")
            basePath = rootDirectory;

        if (!Directory.Exists(basePath))
            return false;

        _rootPaths.Add(basePath);

        lock (_entries) {
            foreach (var f in new DirectoryInfo(rootDirectory).EnumerateFiles("", SearchOption.AllDirectories)) {
                Debug.Assert(f.FullName.StartsWith(rootDirectory));
                if (f.Name.EndsWith(".bak", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (f.Name.EndsWith(".wiiu.stream", StringComparison.OrdinalIgnoreCase)) {
                    var key = f.Name[..^12];
                    if (_packfiles.ContainsKey(key))
                        continue;

                    var path = File.Exists(f.FullName + ".bak") ? f.FullName + ".bak" : f.FullName;
                    var entry = _packfiles[key] = Tuple.Create(
                        path,
                        new Task<WiiuStreamFile>(() => WiiuStreamFile.FromFile(path)));
                    TryLoad(entry.Item2);
                } else {
                    _entries.TryAdd(f.FullName[rootDirectory.Length..], new(rootDirectory, null));
                }
            }
        }

        return true;
    }

    public GameFileSystemReader WithRootDirectory(string rootDirectory) => TryAddRootDirectory(rootDirectory)
        ? this
        : throw new DirectoryNotFoundException(rootDirectory);

    public async IAsyncEnumerable<Tuple<string, Func<Stream>>> FindFiles(
        Regex pattern,
        SkinFlag lookupSkinFlag,
        [EnumeratorCancellation] CancellationToken cancellationToken) {
        await Task.WhenAny(Task.WhenAll(_packfileLoaders), Task.Delay(Timeout.Infinite, cancellationToken));
        cancellationToken.ThrowIfCancellationRequested();
        lock (_entries) {
            foreach (var (path, (rootPath, packfile)) in _entries) {
                cancellationToken.ThrowIfCancellationRequested();
                if (!pattern.IsMatch("/" + path))
                    continue;
                
                if (rootPath is not null)
                    yield return new(path, () => File.OpenRead(Path.Join(rootPath, path)));
                else if (packfile is not null)
                    yield return new(
                        path,
                        () => packfile.GetEntry(path, lookupSkinFlag).Source.GetRawStream(cancellationToken));
                else
                    Debug.Assert(false);
            }
        }
    }

    public async Task<Stream> GetStreamAsync(
        string path,
        SkinFlag lookupSkinFlag,
        CancellationToken cancellationToken) {
        var originalPath = path;
        path = path.Replace('\\', '/').Trim('/');
        while (path.StartsWith("../"))
            path = path[3..];
        if (path == "..")
            throw new FileNotFoundException($"File not found: {originalPath}");

        if (lookupSkinFlag.IsAltSkin()) {
            var basePath = Path.Join(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
            var ext = Path.GetExtension(path);
            var modifiedPath = $"{basePath}.alt{ext}";
            if (_rootPaths.Select(x => Path.Join(x, modifiedPath)).FirstOrDefault(File.Exists) is { } altFsPath)
                return File.OpenRead(altFsPath);
        }

        if (_rootPaths.Select(x => Path.Join(x, path)).FirstOrDefault(File.Exists) is { } fsPath)
            return File.OpenRead(fsPath);

        foreach (var (_, (_, packfileTask)) in _packfiles) {
            await Task.WhenAny(packfileTask, Task.Delay(Timeout.Infinite, cancellationToken));
            cancellationToken.ThrowIfCancellationRequested();

            if (packfileTask.Result.TryGetEntry(out var entry, path, lookupSkinFlag)) {
                var data = entry.Source.ReadRaw(cancellationToken);
                return new MemoryStream(data, 0, data.Length, false, true);
            }
        }

        throw new FileNotFoundException($"File not found: {originalPath}");
    }

    public async Task<byte[]> GetBytesAsync(
        string path,
        SkinFlag lookupSkinFlag,
        CancellationToken cancellationToken) {
        await using var fp = await GetStreamAsync(path, lookupSkinFlag, cancellationToken);
        if (fp is MemoryStream ms)
            return ms.GetBuffer();

        var result = new byte[fp.Length];
        fp.ReadExactly(result);
        return result;
    }

    public Func<string, CancellationToken, Task<Stream>> AsFunc(SkinFlag lookupSkinFlag) =>
        (path, cancellationToken) => GetStreamAsync(path, lookupSkinFlag, cancellationToken);

    public Task<WiiuStreamFile> GetPackfile(string packfileName) => _packfiles[packfileName.ToLowerInvariant()].Item2;

    public string GetPackfilePath(string packfileName) => _packfiles[packfileName.ToLowerInvariant()].Item1;

    private void TryLoad(Task<WiiuStreamFile> packfile) {
        lock (_packfileLoaders) {
            _cancellationTokenSource.Token.ThrowIfCancellationRequested();

            if (_packfileLoaders.Count >= Environment.ProcessorCount) {
                Task.WhenAny(_packfileLoaders).ContinueWith(_ => TryLoad(packfile), _cancellationTokenSource.Token);
            } else {
                _packfileLoaders.Add(packfile);
                packfile.Start();
                packfile.ContinueWith(
                    result => {
                        lock (_entries) {
                            foreach (var f in result.Result.Entries) {
                                var key = f.Header.InnerPath.ToLowerInvariant();
                                if (!_entries.TryGetValue(key, out var v) || v.Item2 == null)
                                    _entries[key] = new(null, result.Result);
                            }
                        }

                        lock (_packfileLoaders)
                            _packfileLoaders.Remove(packfile);
                    },
                    _cancellationTokenSource.Token);
            }
        }
    }
}
