using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SynergyLib.FileFormat;
using SynergyLib.FileFormat.CryEngine;
using SynergyLib.FileFormat.CryEngine.CryXml;
using SynergyLib.FileFormat.GltfInterop;
using SynergyLib.ModMetadata;
using SynergyLib.Util;

namespace SynergyTools.ProgramCommands.ImportFromGltfSubCommands;

public class TestGltfCommand : ImportFromGltfProgramCommand {
    public new static readonly Command Command = new(
        "test",
        "Test your model with a metadata file.");

    public static readonly Argument<string> PathArgument = new(
        "path",
        "Specify gltf/glb file.");

    public static readonly Option<string?> MetadataPathOption = new(
        "--metadata",
        () => null,
        "Specify metadata path. Defaults to .json file of same name with the model file.");

    public static readonly Option<string[]> LevelNameOption = new(
        "--level-name",
        () => new[] {"hub02_seasidevillage"},
        "Specify level names to apply. Defaults to hub02_seasidevillage.") {
        Arity = ArgumentArity.ZeroOrMore,
    };

    public static readonly Option<string[]> GamePathOption = new(
        "--game-path",
        Array.Empty<string>,
        "When specified, the test will be written to game data files.\n" +
        "Specify root content directory, such as \"C:\\mlc01\\usr\\title\\00050000\\10175b00\\content\".\n" +
        "If you have an update applied, specify the update first, and then the base game next.") {
        Arity = ArgumentArity.ZeroOrMore,
    };

    static TestGltfCommand() {
        Command.AddArgument(PathArgument);

        MetadataPathOption.AddAlias("-m");
        Command.AddOption(MetadataPathOption);

        LevelNameOption.AddAlias("-l");
        Command.AddOption(LevelNameOption);

        GamePathOption.AddAlias("-g");
        Command.AddOption(GamePathOption);

        Command.SetHandler(ic => new TestGltfCommand(ic.ParseResult).Handle(ic.GetCancellationToken()));
    }

    public readonly string InPath;
    public readonly string? MetadataPath;
    public readonly string[] LevelNameArray;
    public readonly string[] GamePathArray;

    public TestGltfCommand(ParseResult parseResult) : base(parseResult) {
        InPath = parseResult.GetValueForArgument(PathArgument);
        MetadataPath = parseResult.GetValueForOption(MetadataPathOption);
        LevelNameArray = parseResult.GetValueForOption(LevelNameOption)!;
        GamePathArray = parseResult.GetValueForOption(GamePathOption)!;
    }

    public async Task<int> Handle(CancellationToken cancellationToken) {
        if (!File.Exists(InPath)) {
            using (ScopedConsoleColor.Foreground(ConsoleColor.Red))
                Console.WriteLine("File not found: {0}", InPath);
            return -1;
        }

        var reader = new GameFileSystemReader();
        foreach (var p in GamePathArray) {
            if (!reader.TryAddRootDirectory(p)) {
                using (ScopedConsoleColor.Foreground(ConsoleColor.Yellow))
                    Console.WriteLine("Folder \"Sonic_Crytek\" not found in: {0}", p);
            }
        }

        var metadataPath = MetadataPath ?? Path.ChangeExtension(InPath, ".json");
        if (!File.Exists(metadataPath)) {
            using (ScopedConsoleColor.Foreground(ConsoleColor.Red))
                Console.WriteLine("Metadata file not found: {0}", metadataPath);
            return -1;
        }

        var gltf = GltfTuple.FromFile(InPath);
        var metadata = CharacterMetadata.FromJsonFile(metadataPath);

        var sourceTask = CryCharacter.FromGltfAndMetadata(
            gltf,
            metadata,
            Path.GetDirectoryName(Path.GetFullPath(metadataPath))!,
            cancellationToken);
        var targetTask = CryCharacter.FromCryEngineFiles(
            reader.AsFunc(SkinFlag.LookupDefault),
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

        var newGeoSource = new WiiuStreamFile.FileEntrySource(source.Model.GetGeometryBytes());
        var newMatSource = new WiiuStreamFile.FileEntrySource(source.Model.GetMaterialBytes());
        WiiuStreamFile.FileEntrySource? newDbaSource = source.CryAnimationDatabase is null
            ? null
            : new WiiuStreamFile.FileEntrySource(source.CryAnimationDatabase.GetBytes());
        var suppressProgressDuration = TimeSpan.FromSeconds(5);

        foreach (var levelName in LevelNameArray) {
            Console.WriteLine("Patching level: {0}", levelName);
            var level = await reader.GetPackfile(levelName);

            foreach (var (k, v) in source.Model.ExtraTextures)
                level.PutEntry(0, k, new(v.ToArray()), SkinFlag.Sonic);

            level.GetEntry(target.Definition!.Model!.File!, false).Source = newGeoSource;
            level.GetEntry(target.Definition!.Model!.Material!, false).Source = newMatSource;

            if (target.CharacterParameters?.TracksDatabasePath is { } dbaPath && newDbaSource is not null)
                level.GetEntry(dbaPath, false).Source = newDbaSource.Value;

            var targetPath = reader.GetPackfilePath(levelName);
            while (targetPath.EndsWith(".bak"))
                targetPath = targetPath[..^4];

            var bakFile = targetPath + ".bak";
            if (!File.Exists(bakFile)) {
                if (!File.Exists(bakFile)) {
                    File.Copy(reader.GetPackfilePath(levelName), bakFile);
                    Console.WriteLine("Made a backup copy: {0}", Path.GetFileName(bakFile));
                }
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
}
