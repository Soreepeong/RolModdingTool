using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SynergyLib.FileFormat;
using SynergyLib.FileFormat.CryEngine;
using SynergyLib.FileFormat.CryEngine.CryXml;
using SynergyLib.FileFormat.GltfInterop;
using SynergyLib.ModMetadata;
using SynergyLib.Util;

namespace SynergyTools.ProgramCommands.ModSubCommands;

public class ExportPackCommand : ModProgramCommand {
    public new static readonly Command Command = new(
        "export-pack",
        "Export your mod as a redistributable file.");

    public static readonly Argument<string> PathArgument = new(
        "path",
        "Specify gltf/glb file.");

    public static readonly Option<string?> MetadataPathOption = new(
        "--metadata",
        () => null,
        "Specify metadata path. Defaults to .json file of same name with the model file.");

    public static readonly Option<bool> UseAltSkinsOption = new(
        "--alt",
        () => false,
        "Patch alternative costumes (luminous suits.)");

    public static readonly Option<string?> OutPathOption = new(
        "--out-path",
        () => null,
        "Specify output path. Defaults to .wiiu.stream file of same name with the model file.\n" +
        "Use NUL to skip writing to a file.");

    public static readonly Option<string[]> GamePathOption = new(
        "--game-path",
        Array.Empty<string>,
        "When specified, the model will be written to game data files.\n" +
        "Specify root content directory, such as \"C:\\mlc01\\usr\\title\\00050000\\10175b00\\content\".\n" +
        "If you have an update applied, specify the update first, and then the base game next.") {
        Arity = ArgumentArity.ZeroOrMore,
    };
    
    public static readonly Option<string[]> LevelNameOption = new(
        "--level-name",
        Array.Empty<string>,
        "When specified, the model will be written to game data files.\n" +
        "Specify level names to apply your model immediately, such as \"hub02_seasidevillage\".") {
        Arity = ArgumentArity.ZeroOrMore,
    };

    static ExportPackCommand() {
        Command.AddAlias("export");
        
        Command.AddArgument(PathArgument);

        MetadataPathOption.AddAlias("-m");
        Command.AddOption(MetadataPathOption);

        UseAltSkinsOption.AddAlias("-a");
        Command.AddOption(UseAltSkinsOption);

        OutPathOption.AddAlias("-o");
        Command.AddOption(OutPathOption);

        GamePathOption.AddAlias("-g");
        Command.AddOption(GamePathOption);

        LevelNameOption.AddAlias("-n");
        Command.AddOption(LevelNameOption);

        Command.SetHandler(ic => new ExportPackCommand(ic.ParseResult).Handle(ic.GetCancellationToken()));
    }

    public readonly string InPath;
    public readonly string? MetadataPath;
    public readonly bool UseAltSkins;
    public readonly string? OutPath;
    public readonly string[] LevelNameArray;
    public readonly string[] GamePathArray;

    public ExportPackCommand(ParseResult parseResult) : base(parseResult) {
        InPath = parseResult.GetValueForArgument(PathArgument);
        MetadataPath = parseResult.GetValueForOption(MetadataPathOption);
        UseAltSkins = parseResult.GetValueForOption(UseAltSkinsOption);
        OutPath = parseResult.GetValueForOption(OutPathOption);
        LevelNameArray = parseResult.GetValueForOption(LevelNameOption)!;
        GamePathArray = parseResult.GetValueForOption(GamePathOption)!;
    }

    public async Task<int> Handle(CancellationToken cancellationToken) {
        var reader = new GameFileSystemReader();
        foreach (var p in GamePathArray) {
            if (!reader.TryAddRootDirectory(p)) {
                using (ScopedConsoleColor.Foreground(ConsoleColor.Yellow))
                    Console.WriteLine("Folder \"Sonic_Crytek\" not found in: {0}", p);
            }
        }

        var files = new List<Tuple<WiiuStreamFile.FileEntryHeader, WiiuStreamFile.FileEntrySource>>();
        Console.WriteLine("Converting file: {0}", InPath);
        await foreach (var t in GenerateFiles(
                           InPath,
                           MetadataPath,
                           UseAltSkins,
                           reader,
                           CompressionLevel,
                           CompressionChunkSize,
                           cancellationToken)) {
            files.Add(t);
        }

        var suppressProgressDuration = TimeSpan.FromSeconds(5);

        if (OutPath?.ToLowerInvariant() is not "nul") {
            var outPath = OutPath ?? Path.ChangeExtension(InPath, ".wiiu.stream");
            if (!Overwrite && File.Exists(outPath)) {
                using (ScopedConsoleColor.Foreground(ConsoleColor.Yellow))
                    Console.WriteLine("File already exists: {0}", outPath);
            } else {
                Console.WriteLine("Writing file: {0}", outPath);
                
                var strmFile = new WiiuStreamFile();
                strmFile.Entries.AddRange(files.Select(x => new WiiuStreamFile.FileEntry(x.Item1, x.Item2)));
                await CompressProgramCommand.WriteAndPrintProgress(
                    outPath,
                    strmFile,
                    new() {CompressionLevel = 0, SkipAlreadyCompressed = true},
                    cancellationToken,
                    suppressProgressDuration);
            }
        }
        
        foreach (var levelName in LevelNameArray) {
            Console.WriteLine("Patching level: {0}", levelName);
            var level = await reader.GetPackfile(levelName);

            foreach (var (k, v) in files)
                level.PutEntry(0, k.InnerPath, v, k.SkinFlag);
            
            var targetPath = reader.GetPackfilePath(levelName);
            while (targetPath.EndsWith(".bak"))
                targetPath = targetPath[..^4];

            var bakFile = targetPath + ".bak";
            if (!File.Exists(bakFile)) {
                File.Copy(reader.GetPackfilePath(levelName), bakFile);
                Console.WriteLine("Made a backup copy: {0}", Path.GetFileName(bakFile));
            }

            await CompressProgramCommand.WriteAndPrintProgress(
                targetPath,
                level,
                default,
                cancellationToken,
                suppressProgressDuration);
        }

        Console.WriteLine("Done!");
        return 0;
    }

    public static async IAsyncEnumerable<Tuple<WiiuStreamFile.FileEntryHeader, WiiuStreamFile.FileEntrySource>>
        GenerateFiles(
            string path,
            string? metadataPath,
            bool useAltSkins,
            GameFileSystemReader reader,
            int compressionLevel,
            int compressionChunkSize,
            [EnumeratorCancellation] CancellationToken cancellationToken) {
        if (!File.Exists(path))
            throw new FileNotFoundException($"File not found: {path}");

        metadataPath ??= Path.ChangeExtension(path, ".json");
        if (!File.Exists(metadataPath))
            throw new FileNotFoundException($"Metadata file not found: {metadataPath}");

        var gltf = GltfTuple.FromFile(path);
        var metadata = CharacterMetadata.FromJsonFile(metadataPath);

        var sourceTask = CryCharacter.FromGltfAndMetadata(
            gltf,
            metadata,
            Path.GetDirectoryName(Path.GetFullPath(metadataPath))!,
            cancellationToken);
        var targetTask = CryCharacter.FromCryEngineFiles(
            reader.AsFunc(useAltSkins ? SkinFlag.LookupAlt : SkinFlag.LookupDefault),
            metadata.TargetPath,
            cancellationToken);

        await Task.WhenAll(sourceTask, targetTask);

        var source = sourceTask.Result;
        var target = targetTask.Result;

        var newMaterial = (Material) target.Model.Material!.Clone();
        newMaterial.AddOrReplaceSubmaterialsOfSameName(source.Model.Material?.SubMaterials);
        source.Model.Material = newMaterial;

        if (source.CryAnimationDatabase is not null && target.CryAnimationDatabase is not null) {
            var newdb = new CryAnimationDatabase();
            foreach (var (k, v) in source.CryAnimationDatabase.Animations) {
                newdb.Animations.Add(k, v);
                foreach (var k2 in target.CryAnimationDatabase.Animations.Keys.Where(x => x.EndsWith($"/{k}.caf")))
                    newdb.Animations.Add(k2, v);
            }

            newdb.PasteFrom(target.CryAnimationDatabase, false);

            source.CryAnimationDatabase = newdb;
        }

        if (metadata.HeightScaleRelativeToTarget is { } heightScale && Math.Abs(heightScale - 1) >= 1e-6f) {
            source.ApplyScaleTransformation(
                target.Model.CalculateBoundingBox().SizeVector.Z *
                heightScale /
                source.Model.CalculateBoundingBox().SizeVector.Z);
        }

        var targetFiles = source.Model.ExtraTextures.AsEnumerable();
        string? geoPath;
        string? matPath;
        var dbaPath = target.CharacterParameters?.TracksDatabasePath;
        var targetPathExtension = Path.GetExtension(metadata.TargetPath).ToLowerInvariant();
        switch (targetPathExtension) {
            case ".cdf":
                geoPath = target.Definition?.Model?.File ??
                    throw new InvalidDataException("Definition.Model.File should not be null");
                matPath = target.Definition!.Model!.Material ??
                    throw new InvalidDataException("Definition.Model.Material should not be null");
                break;
            case ".chr":
            case ".cgf":
                geoPath = metadata.TargetPath;
                matPath = source.Model.PseudoMaterials.FirstOrDefault()?.Name;
                if (matPath?.Contains('/') is false)
                    matPath = Path.Join(Path.GetDirectoryName(geoPath), $"{matPath}.mtl");
                matPath ??= Path.ChangeExtension(geoPath, ".mtl");
                break;
            default:
                throw new NotSupportedException($"{targetPathExtension} is not a supported target file extension.");
        }

        geoPath = geoPath.Replace("\\", "/").Trim('/').ToLowerInvariant();
        matPath = matPath.Replace("\\", "/").Trim('/').ToLowerInvariant();
        dbaPath = dbaPath?.Replace("\\", "/").Trim('/').ToLowerInvariant();

        targetFiles = targetFiles.Append(new(geoPath, new(source.Model.GetGeometryBytes())));
        targetFiles = targetFiles.Append(new(matPath, new(source.Model.GetMaterialBytes())));
        if (dbaPath is not null && source.CryAnimationDatabase?.Animations.Any() is true)
            targetFiles = targetFiles.Append(new(dbaPath, new(source.CryAnimationDatabase.GetBytes())));

        var mtlSkinFlag = matPath switch {
            "objects/characters/1_heroes/sonic/sonic.mtl" =>
                useAltSkins ? SkinFlag.SonicAlt : SkinFlag.Sonic,
            "objects/characters/14_props/amy_hammer/amy_hammer.mtl" =>
                useAltSkins ? SkinFlag.AmyAlt : SkinFlag.Amy,
            "objects/characters/1_heroes/amy/amy.mtl" =>
                useAltSkins ? SkinFlag.AmyAlt : SkinFlag.Amy,
            "objects/characters/1_heroes/tails/tails.mtl" =>
                useAltSkins ? SkinFlag.TailsAlt : SkinFlag.Tails,
            "objects/characters/1_heroes/knuckles/knuckles.mtl" =>
                useAltSkins ? SkinFlag.KnucklesAlt : SkinFlag.Knuckles,
            _ => SkinFlag.Default,
        };

        foreach (var t in targetFiles
                     .Select(
                         x =>
                             new WiiuStreamFile.FileEntrySource(x.Value.ToArray(), (int) x.Value.Length)
                                 .ToCompressed(compressionLevel, compressionChunkSize, cancellationToken)
                                 .ContinueWith(r => Tuple.Create(x.Key, r.Result), cancellationToken))
                     .ToList()) {
            var (entryPath, entrySource) = await t;
            entryPath = entryPath.Replace("\\", "/").Trim('/').ToLowerInvariant();

            var skinFlag = entryPath.EndsWith(".dds") || entryPath.EndsWith(".mtl") ? mtlSkinFlag : SkinFlag.Default;
            yield return new(
                new() {
                    CompressedSize = entrySource.IsCompressed ? entrySource.StoredLength : 0,
                    DecompressedSize = entrySource.RawLength,
                    Hash = entrySource.Hash,
                    Unknown = ushort.MaxValue,
                    SkinFlag = skinFlag,
                    InnerPath = entryPath,
                },
                entrySource);
        }
    }
}
