using System;
using System.Collections.Generic;
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
        @"to-gltf Sonic_Crytek\heroes.wiiu.stream Sonic_Crytek\Levels\level02_ancientfactorypresent_a.wiiu.stream " +
        @"Sonic_Crytek\Levels\level05_sunkenruins.wiiu.stream -f objects\characters\5_minibosses\shadow\shadow.cdf " +
        @"-f objects\characters\5_minibosses\metal_sonic\metal_sonic.cdf -o Z:\ -y");

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
        Command.SetHandler(ic => new ConvertToGltfProgramCommand(ic.ParseResult).Handle(ic.GetCancellationToken()));
    }

    public readonly string[] InPathArray;
    public readonly string[] SubPathArray;
    public readonly bool UseAltSkins;
    public readonly string? BaseOutPath;

    public ConvertToGltfProgramCommand(ParseResult parseResult) : base(parseResult) {
        InPathArray = parseResult.GetValueForArgument(PathArgument);
        SubPathArray = parseResult.GetValueForOption(SubPathOption) ?? Array.Empty<string>();
        UseAltSkins = parseResult.GetValueForOption(UseAltSkinsOption);
        BaseOutPath = parseResult.GetValueForOption(BaseOutPathOption);
    }

    public async Task<int> Handle(CancellationToken cancellationToken) {
        if (!SubPathArray.Any())
            throw new InvalidOperationException();

        var levels = new List<WiiuStreamFile>();
        foreach (var p in InPathArray) {
            levels.Add(new());
            levels.Last().ReadFrom(null, p, cancellationToken);
        }

        Stream CascadingReader(string path) {
            foreach (var level in levels) {
                foreach (var flag in UseAltSkins
                             ? new[] {
                                 SkinFlag.Sonic_Alt,
                                 SkinFlag.Amy_Alt,
                                 SkinFlag.Tails_Alt,
                                 SkinFlag.Knuckles_Alt,
                                 SkinFlag.Default,
                             }
                             : new[] {
                                 SkinFlag.Sonic_Default,
                                 SkinFlag.Sonic_Default,
                                 SkinFlag.Sonic_Default,
                                 SkinFlag.Sonic_Default,
                                 SkinFlag.Default,
                             }) {
                    if (level.TryGetEntry(out var entry, path, flag))
                        return new MemoryStream(entry.Source.ReadRaw());
                }
            }

            throw new FileNotFoundException();
        }

        foreach (var p in SubPathArray) {
            Console.WriteLine("Processing: {0}", p);
            try {
                var outPath = Path.Join(
                    BaseOutPath ?? Environment.CurrentDirectory,
                    Path.GetFileNameWithoutExtension(p)) + ".glb";

                if (File.Exists(outPath) && !Overwrite) {
                    Console.WriteLine("=> Skipping because of an already existing file: {0}", outPath);
                    continue;
                }

                var chr = CryCharacter.FromCryEngineFiles(CascadingReader, p);
                await using (var s = File.Create(outPath))
                    chr.ToGltf(CascadingReader).Compile(s);

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
