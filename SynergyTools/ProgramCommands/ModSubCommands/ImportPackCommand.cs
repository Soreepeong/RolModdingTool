using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SynergyLib.FileFormat;
using SynergyLib.Util;

namespace SynergyTools.ProgramCommands.ModSubCommands;

public class ImportPackCommand : ModProgramCommand {
    public new static readonly Command Command = new(
        "import-pack",
        "Import a mod stored in .wiiu.stream format.");

    public static readonly Argument<string[]> PathArgument = new(
        "path",
        "Specify .wiiu.stream files.");

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
        "Specify level names to apply your model immediately, such as \"hub02_seasidevillage\".\n" +
        "When empty, it will be applied to all levels.") {
        Arity = ArgumentArity.ZeroOrMore,
    };

    static ImportPackCommand() {
        Command.AddAlias("import");

        Command.AddArgument(PathArgument);

        GamePathOption.AddAlias("-g");
        Command.AddOption(GamePathOption);

        LevelNameOption.AddAlias("-n");
        Command.AddOption(LevelNameOption);

        Command.SetHandler(ic => new ImportPackCommand(ic.ParseResult).Handle(ic.GetCancellationToken()));
    }

    public readonly string[] InPathArray;
    public readonly string[] LevelNameArray;
    public readonly string[] GamePathArray;

    public ImportPackCommand(ParseResult parseResult) : base(parseResult) {
        InPathArray = parseResult.GetValueForArgument(PathArgument);
        LevelNameArray = parseResult.GetValueForOption(LevelNameOption)!;
        GamePathArray = parseResult.GetValueForOption(GamePathOption)!;
    }

    public async Task<int> Handle(CancellationToken cancellationToken) {
        var saveConfig = new WiiuStreamFile.SaveConfig {
            CompressionChunkSize = CompressionChunkSize,
            CompressionLevel = CompressionLevel,
        };
        
        var sources = new List<WiiuStreamFile>(InPathArray.Length);
        foreach (var p in InPathArray) {
            try {
                var s = new WiiuStreamFile();
                s.ReadFrom(null, p, cancellationToken);
                sources.Add(s);
                Console.WriteLine("File loaded: {0}", p);
            } catch (FileNotFoundException) {
                using (ScopedConsoleColor.Foreground(ConsoleColor.Yellow))
                    Console.WriteLine("File not found: {0}", p);
            } catch (Exception e) {
                using (ScopedConsoleColor.Foreground(ConsoleColor.Yellow)) {
                    Console.WriteLine("Failed to load file: {0}", p);
                    Console.WriteLine(e);
                }
            }
        }

        var reader = new GameFileSystemReader();
        foreach (var p in GamePathArray) {
            if (!reader.TryAddRootDirectory(p)) {
                using (ScopedConsoleColor.Foreground(ConsoleColor.Yellow))
                    Console.WriteLine("Folder \"Sonic_Crytek\" not found in: {0}", p);
            }
        }

        var suppressProgressDuration = TimeSpan.FromSeconds(5);

        var levelNames = LevelNameArray.Length > 0
            ? LevelNameArray
            : new[] {
                "bm_01_01", "bm_01_02", "bm_01_03", "bm_01_04", "bm_02_01", "bm_02_02", "bm_02_03", "bm_02_04",
                "bm_03_01", "bm_03_02", "bm_03_03", "bm_03_04", "e3_level04", "e3_level06", "e3_level07_eggman",
                "e3_road", "hub01_excavationsite", "hub02_seasidevillage", "hub02b_craterlake", "level01_lyricstomb",
                "level02_ancientfactorypresent_a", "level03_ancientfactorypast", "level04_lyricsdigsite",
                "level05_sunkenruins", "level06_oceanwaterfall", "level07_mysteryisland", "level09_cloudcity",
                "level10_lyricslair", "road_h1-r2", "road_h1-r4", "road_h2-r5", "road_h2-r7", "vehicle_06-su",
                "vehicle_h1-bo",
            };
        foreach (var levelName in levelNames) {
            Console.WriteLine("Patching level: {0}", levelName);
            var level = await reader.GetPackfile(levelName);

            foreach (var entry in sources.SelectMany(x => x.Entries))
                level.PutEntry(0, entry.Header.InnerPath, entry.Source, entry.Header.SkinFlag);

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
                saveConfig,
                cancellationToken,
                suppressProgressDuration);
        }

        Console.WriteLine("Done!");
        return 0;
    }
}
