using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
        @"C:\mlc01\usr\title\0005000e\10175b00\content " +
        @"C:\mlc01\usr\title\00050000\10175b00\content " +
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

    public static readonly Option<bool> DisableAnimationsOption = new(
        "--disable-animation",
        () => false,
        "Do not export animations.");

    public static readonly Option<bool> UseSingleFileOption = new(
        "--single-file",
        () => false,
        "Use single file output (.glb instead of .gltf).");

    public static readonly Option<bool> PreserveDirectoryStructureOption = new(
        "--preserve-directory-structure",
        () => false,
        "Preserve directory structure.");

    public static readonly Option<bool> ExportOnlyRequiredTexturesOption = new(
        "--export-required-textures-only",
        () => false,
        "Only export required textures.");

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
        DisableAnimationsOption.AddAlias("-d");
        Command.AddOption(DisableAnimationsOption);
        UseSingleFileOption.AddAlias("-s");
        Command.AddOption(UseSingleFileOption);
        ExportOnlyRequiredTexturesOption.AddAlias("-r");
        Command.AddOption(ExportOnlyRequiredTexturesOption);
        PreserveDirectoryStructureOption.AddAlias("-p");
        Command.AddOption(PreserveDirectoryStructureOption);
        Command.SetHandler(ic => new ConvertToGltfProgramCommand(ic.ParseResult).Handle(ic.GetCancellationToken()));
    }

    public readonly string[] InPathArray;
    public readonly string[] SubPathArray;
    public readonly string? BaseOutPath;
    public readonly bool UseAltSkins;
    public readonly bool DisableAnimations;
    public readonly bool UseSingleFile;
    public readonly bool ExportOnlyRequiredTextures;
    public readonly bool PreserveDirectoryStructure;

    public ConvertToGltfProgramCommand(ParseResult parseResult) : base(parseResult) {
        InPathArray = parseResult.GetValueForArgument(PathArgument);
        SubPathArray = parseResult.GetValueForOption(SubPathOption) ?? Array.Empty<string>();
        BaseOutPath = parseResult.GetValueForOption(BaseOutPathOption);
        UseAltSkins = parseResult.GetValueForOption(UseAltSkinsOption);
        DisableAnimations = parseResult.GetValueForOption(DisableAnimationsOption);
        UseSingleFile = parseResult.GetValueForOption(UseSingleFileOption);
        ExportOnlyRequiredTextures = parseResult.GetValueForOption(ExportOnlyRequiredTexturesOption);
        PreserveDirectoryStructure = parseResult.GetValueForOption(PreserveDirectoryStructureOption);
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

        var pathList = new List<string>();
        if (SubPathArray.Any(x => x.Contains('?') || x.Contains('*'))) {
            Console.WriteLine("Looking for files...");
            foreach (var globPattern in SubPathArray) {
                await foreach (var (file, _) in reader.FindFiles(
                                   new(
                                       "^" +
                                       string.Join(
                                           "[\\s\\S]*",
                                           Regex.Split(globPattern, "\\*{2,}").Select(
                                               x =>
                                                   string.Join(
                                                       "[^/\\\\]*",
                                                       x.Split("*").Select(
                                                           y =>
                                                               string.Join(
                                                                   "[^/\\\\]",
                                                                   y.Split('?').Select(Regex.Escape)))))) +
                                       "$",
                                       RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase),
                                   UseAltSkins ? SkinFlag.LookupAlt : SkinFlag.LookupDefault,
                                   cancellationToken)) {
                    pathList.Add(file);
                }
            }

            Console.WriteLine("Found {0} file(s).", pathList.Count);
        } else
            pathList.AddRange(SubPathArray);
        
        pathList = pathList.DistinctBy(x => Path.ChangeExtension(x, null)).Order().ToList();
        var outputNameList = pathList.StripCommonParentPaths();

        for (var i = 0; i < pathList.Count; i++) {
            cancellationToken.ThrowIfCancellationRequested();
            if (i < 1024)
                continue;
            var path = pathList[i];
            var outputName = Path.ChangeExtension(
                PreserveDirectoryStructure
                    ? path
                    : outputNameList[i].Replace("/", "_"),
                null);
            Console.WriteLine(
                "[{0,5}/{1,5} {2,6:0.00}%] Processing: {3} to {4}",
                i,
                pathList.Count,
                100f * i / pathList.Count,
                path,
                outputName);
            try {
                var outPath = Path.Join(
                    BaseOutPath ?? Environment.CurrentDirectory,
                    outputName);
                if (UseSingleFile)
                    outPath += ".glb";

                if (Path.Exists(outPath) && !Overwrite) {
                    Console.WriteLine("=> Skipping because of an already existing file: {0}", outPath);
                    continue;
                }

                var chr = await CryCharacter.FromCryEngineFiles(readerFunc, path, cancellationToken);
                var gltf = await chr.ToGltf(
                    readerFunc,
                    !DisableAnimations,
                    ExportOnlyRequiredTextures,
                    cancellationToken);
                if (UseSingleFile) {
                    Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);
                    await using var s = File.Create(outPath);
                    gltf.Compile(s);
                    
                    Console.WriteLine("=> File created at: {0}", outPath);
                } else {
                    Directory.CreateDirectory(outPath);
                    foreach (var (name, strm) in gltf.CompileToFiles(Path.GetFileNameWithoutExtension(path))) {
                        var filePath = Path.Join(outPath, name);
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
                        await using var s = File.Create(filePath);
                        await strm.CopyToAsync(s, cancellationToken);
                    }
                    
                    Console.WriteLine("=> Directory created at: {0}", outPath);
                }
            } catch (Exception e) {
                using (ScopedConsoleColor.Foreground(ConsoleColor.Red)) {
                    Console.WriteLine("=> {0}: {1}", e.GetType().FullName, e.Message);
                    if (e is not NotSupportedException)
                        Console.WriteLine(e.StackTrace);
                }
            }
        }

        Console.WriteLine("Done!");

        return 0;
    }
}
