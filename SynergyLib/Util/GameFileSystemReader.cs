using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SynergyLib.FileFormat;

namespace SynergyLib.Util;

public sealed class GameFileSystemReader : IAsyncDisposable, IDisposable {
    private readonly List<string> _rootPaths = new();
    private readonly Dictionary<string, Tuple<string, Task<WiiuStreamFile>>> _packfiles = new();
    private readonly List<Task<WiiuStreamFile>> _packfileLoaders = new();

    private readonly CancellationTokenSource _cancellationTokenSource = new();

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

    public bool AddRootDirectory(string rootDirectory) {
        _cancellationTokenSource.Token.ThrowIfCancellationRequested();

        rootDirectory = rootDirectory.TrimEnd('\\', '/');

        var basePath = Path.Join(rootDirectory, "Sonic_Crytek");
        if (!Directory.Exists(basePath))
            basePath = Path.Join(rootDirectory, "content", "Sonic_Crytek");
        if (!Directory.Exists(basePath) && Path.GetFileName(rootDirectory).ToLowerInvariant() == "sonic_crytek")
            basePath = rootDirectory;

        if (!Directory.Exists(basePath))
            return false;

        _rootPaths.Add(basePath);

        var levelsPath = Path.Join(basePath, "levels");
        var packfilePathList = new List<string>();
        if (Directory.Exists(levelsPath))
            packfilePathList.AddRange(
                Directory.GetFiles(levelsPath)
                    .Where(x => x.EndsWith(".wiiu.stream", StringComparison.OrdinalIgnoreCase))
                    .Select(x => File.Exists($"{x}.bak") ? $"{x}.bak" : x));

        packfilePathList.AddRange(
            Directory.GetFiles(basePath)
                .Where(x => x.EndsWith(".wiiu.stream", StringComparison.OrdinalIgnoreCase))
                .Select(x => File.Exists($"{x}.bak") ? $"{x}.bak" : x));

        foreach (var path in packfilePathList) {
            var key = Path.GetFileName(path).Split('.', 2)[0].ToLowerInvariant();
            if (_packfiles.ContainsKey(key))
                continue;

            var entry = _packfiles[key] = Tuple.Create(
                path,
                new Task<WiiuStreamFile>(() => WiiuStreamFile.FromFile(path)));
            TryLoad(entry.Item2);
        }

        return true;
    }

    public GameFileSystemReader WithRootDirectory(string rootDirectory) => AddRootDirectory(rootDirectory)
        ? this
        : throw new DirectoryNotFoundException(rootDirectory);

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

            if (packfileTask.Result.TryGetEntry(out var entry, path, lookupSkinFlag))
                return new MemoryStream(entry.Source.ReadRaw(cancellationToken));
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
                    _ => {
                        lock (_packfileLoaders)
                            _packfileLoaders.Remove(packfile);
                    },
                    _cancellationTokenSource.Token);
            }
        }
    }
}
