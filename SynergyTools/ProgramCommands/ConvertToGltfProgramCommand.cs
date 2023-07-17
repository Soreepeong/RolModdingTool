using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SynergyLib.FileFormat;
using SynergyLib.FileFormat.CryEngine;
using SynergyLib.Util;

namespace SynergyTools.ProgramCommands;

public class ConvertToGltfProgramCommand : RootProgramCommand {
    public new static readonly Command Command = new(
        "to-gltf",
        "Converts a game model file into glb file.\n\nExample usage:\n" +
        @"to-gltf -y " +
        @"C:\mlc01\usr\title\0005000e\10175b00\content\content " +
        @"C:\mlc01\usr\title\00050000\10175b00\content\content " +
        @"-f objects\characters\5_minibosses\shadow\shadow.cdf " +
        @"-f objects\characters\5_minibosses\metal_sonic\metal_sonic.cdf " +
        @"-o Z:\");

    public static readonly Argument<string[]> PathArgument = new(
        "path",
        "Specify root content directory, such as \"C:\\mlc01\\usr\\title\\00050000\\10175b00\\content\".\n" +
        "If you have an update applied, specify the update first, and then the base game next.") {
        Arity = ArgumentArity.OneOrMore,
    };

    public static readonly Option<string[]> SubPathOption = new(
        "--file",
        "Specify path to a .cdf, .chr, or a .cgf file, inside a game archive.") {
        Arity = ArgumentArity.OneOrMore,
    };

    public static readonly Option<bool> UseAltSkinsOption = new(
        "--alt",
        () => false,
        "Use alternative costume (luminous suits.)");

    public static readonly Option<bool> UseSingleFileOption = new(
        "--single-file",
        () => false,
        "Use single file output (.glb instead of .gltf).");

    public static readonly Option<string?> BaseOutPathOption = new(
        "--out-path",
        () => null,
        "Specify output directory. Defaults to current directory.");

    static ConvertToGltfProgramCommand() {
        Command.AddArgument(PathArgument);
        BaseOutPathOption.AddAlias("-o");
        Command.AddOption(BaseOutPathOption);
        SubPathOption.AddAlias("-f");
        Command.AddOption(SubPathOption);
        UseAltSkinsOption.AddAlias("-a");
        Command.AddOption(UseAltSkinsOption);
        UseSingleFileOption.AddAlias("-s");
        Command.AddOption(UseSingleFileOption);
        Command.SetHandler(ic => new ConvertToGltfProgramCommand(ic.ParseResult).Handle(ic.GetCancellationToken()));
    }

    public readonly string[] InPathArray;
    public readonly string[] SubPathArray;
    public readonly string? BaseOutPath;
    public readonly bool UseAltSkins;
    public readonly bool UseSingleFile;

    public ConvertToGltfProgramCommand(ParseResult parseResult) : base(parseResult) {
        InPathArray = parseResult.GetValueForArgument(PathArgument);
        SubPathArray = parseResult.GetValueForOption(SubPathOption) ?? Array.Empty<string>();
        BaseOutPath = parseResult.GetValueForOption(BaseOutPathOption);
        UseAltSkins = parseResult.GetValueForOption(UseAltSkinsOption);
        UseSingleFile = parseResult.GetValueForOption(UseSingleFileOption);
    }

    public async Task<int> Handle(CancellationToken cancellationToken) {
        if (!SubPathArray.Any())
            throw new InvalidOperationException();

        var reader = new GameFileSystemReader();
        foreach (var p in InPathArray) {
            if (!reader.AddRootDirectory(p)) {
                using (ScopedConsoleColor.Foreground(ConsoleColor.Yellow))
                    Console.WriteLine("Folder \"Sonic_Crytek\" not found in: {0}", p);
            }
        }

        var readerFunc = reader.AsFunc(UseAltSkins ? SkinFlag.LookupAlt : SkinFlag.LookupDefault);

        foreach (var p in SubPathArray) {
            Console.WriteLine("Processing: {0}", p);
            try {
                var outPath = Path.Join(
                    BaseOutPath ?? Environment.CurrentDirectory,
                    Path.GetFileNameWithoutExtension(p));
                if (UseSingleFile)
                    outPath += ".glb";

                if (Path.Exists(outPath) && !Overwrite) {
                    Console.WriteLine("=> Skipping because of an already existing file: {0}", outPath);
                    continue;
                }

                var chr = await CryCharacter.FromCryEngineFiles(readerFunc, p, cancellationToken);
                var gltf = await chr.ToGltf(readerFunc, cancellationToken);
                if (UseSingleFile) {
                    await using var s = File.Create(outPath);
                    gltf.Compile(s);
                } else {
                    await using (var s = File.Create(Path.Join(outPath, Path.GetFileNameWithoutExtension(p) + ".glb")))
                        gltf.Compile(s);
                    foreach (var (name, strm) in gltf.CompileToFiles(Path.GetFileNameWithoutExtension(p))) {
                        var filePath = Path.Join(outPath, name);
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                        await using var s = File.Create(filePath);
                        await strm.CopyToAsync(s, cancellationToken);
                    }
                }

                Console.WriteLine("=> File created at: {0}", outPath);
            } catch (Exception e) {
                using (ScopedConsoleColor.Foreground(ConsoleColor.Red)) {
                    Console.WriteLine("=> Error: {0}", e);
                    Console.WriteLine(e.StackTrace);
                    Console.WriteLine();
                }
            }
        }

        Console.WriteLine("Done!");

        return 0;
    }
}
