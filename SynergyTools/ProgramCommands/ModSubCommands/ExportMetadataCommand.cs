using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SynergyLib.FileFormat;
using SynergyLib.FileFormat.CryEngine;
using SynergyLib.FileFormat.GltfInterop;
using SynergyLib.ModMetadata;
using SynergyLib.Util;
using SynergyLib.Util.CustomJsonConverters;

namespace SynergyTools.ProgramCommands.ModSubCommands;

public class ExportMetadataCommand : ModProgramCommand {
    public new static readonly Command Command = new(
        "metadata",
        "Export a metadata file for material configuration." +
        "Example usage:\n" +
        $@"{ModProgramCommand.Command.Name} metadata -y " +
        @"-b C:\mlc01\usr\title\0005000e\10175b00\content " +
        @"-b C:\mlc01\usr\title\00050000\10175b00\content " +
        "-c objects/characters/1_heroes/sonic/sonic.chr " +
        @"Z:\m0361b0001.glb ");

    public static readonly Argument<string[]> PathArgument = new(
        "path",
        "Specify gltf/glb files.") {
        Arity = ArgumentArity.OneOrMore,
    };

    public static readonly Option<string[]> GamePathOption = new(
        "--game-path",
        Array.Empty<string>,
        "Specify root content directory, such as \"C:\\mlc01\\usr\\title\\00050000\\10175b00\\content\".\n" +
        "If you have an update applied, specify the update first, and then the base game next.") {
        Arity = ArgumentArity.ZeroOrMore,
    };

    public static readonly Option<string?> ReferenceModelSubPathOption = new(
        "--reference-model",
        () => null,
        "Specify path to a .cdf, .chr, or a .cgf file, inside a game archive, to use as the reference model.");

    public static readonly Option<string?> BaseOutPathOption = new(
        "--out-path",
        () => null,
        "Specify output directory. Defaults to current directory.");

    public static readonly Option<Vector3JsonConverter.Notation> ColorNotationOption = new(
        "--color-notation",
        () => Vector3JsonConverter.Notation.HexByteString,
        "Specify text representation for color values.");

    static ExportMetadataCommand() {
        Command.AddArgument(PathArgument);

        GamePathOption.AddAlias("-g");
        Command.AddOption(GamePathOption);

        ReferenceModelSubPathOption.AddAlias("-r");
        Command.AddOption(ReferenceModelSubPathOption);

        GamePathOption.AddAlias("-o");
        Command.AddOption(BaseOutPathOption);

        ColorNotationOption.AddAlias("-c");
        Command.AddOption(ColorNotationOption);

        Command.SetHandler(ic => new ExportMetadataCommand(ic.ParseResult).Handle(ic.GetCancellationToken()));
    }

    public readonly string[] InPathArray;
    public readonly string[] GamePathArray;
    public readonly string? ReferenceModelSubPath;
    public readonly string? BaseOutPath;
    public readonly Vector3JsonConverter.Notation ColorNotation;

    public ExportMetadataCommand(ParseResult parseResult) : base(parseResult) {
        InPathArray = parseResult.GetValueForArgument(PathArgument);
        GamePathArray = parseResult.GetValueForOption(GamePathOption)!;
        ReferenceModelSubPath = parseResult.GetValueForOption(ReferenceModelSubPathOption);
        BaseOutPath = parseResult.GetValueForOption(BaseOutPathOption);
        ColorNotation = parseResult.GetValueForOption(ColorNotationOption);
    }

    public async Task<int> Handle(CancellationToken cancellationToken) {
        CryCharacter? referenceCharacter = null;
        CharacterMetadata? referenceMetadata = null;
        if (ReferenceModelSubPath is not null) {
            var path = ReferenceModelSubPath;
            path = path.Trim(' ', '/', '\\').Replace("\\", "/");
            var reader = new GameFileSystemReader();
            foreach (var p in GamePathArray) {
                if (!reader.TryAddRootDirectory(p)) {
                    using (ScopedConsoleColor.Foreground(ConsoleColor.Yellow))
                        Console.WriteLine("Folder \"Sonic_Crytek\" not found in: {0}", p);
                }
            }

            referenceCharacter = await CryCharacter.FromCryEngineFiles(
                reader.AsFunc(SkinFlag.LookupDefault),
                path,
                cancellationToken);
            Console.WriteLine("Reference model loaded: {0}", path);

            referenceMetadata = CharacterMetadata.FromCharacter(referenceCharacter, path, null);
        }

        foreach (var path in InPathArray) {
            cancellationToken.ThrowIfCancellationRequested();

            Console.WriteLine("Working on: {0}", path);

            var outDir = BaseOutPath ?? Path.GetDirectoryName(path)!;
            try {
                var metadataFilePath = Path.Join(outDir, Path.GetFileNameWithoutExtension(path) + ".json");
                if (File.Exists(metadataFilePath) && !Overwrite) {
                    using (ScopedConsoleColor.Foreground(ConsoleColor.Yellow))
                        Console.WriteLine("=> File already exists at: {0}", metadataFilePath);
                    continue;
                }

                var gltf = GltfTuple.FromFile(path);
                var character = await CryCharacter.FromGltf(gltf, gltf.Root.Nodes[0].Name, default);

                var metadata = CharacterMetadata.FromCharacter(character, referenceMetadata?.TargetPath ?? "", gltf);
                if (referenceMetadata is not null) {
                    Debug.Assert(referenceCharacter is not null);
                    metadata.Animations = referenceMetadata.Animations;
                    metadata.HeightScaleRelativeToTarget =
                        character.Model.CalculateBoundingBox().SizeVector.Z /
                        referenceCharacter.Model.CalculateBoundingBox().SizeVector.Z;
                }

                Directory.CreateDirectory(outDir);
                metadata.ToJson(metadataFilePath, ColorNotation);
            } catch (Exception e) when (e is not OperationCanceledException) {
                using (ScopedConsoleColor.Foreground(ConsoleColor.Red)) {
                    Console.WriteLine("=> {0}: {1}", e.GetType().FullName, e.Message.Trim());
                    if (e is not NotSupportedException ex || ex.Message == "Specified method is not supported.")
                        Console.WriteLine(e.StackTrace);
                }
            }
        }

        Console.WriteLine("Done!");

        return 0;
    }
}
