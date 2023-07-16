using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SynergyLib.FileFormat;
using SynergyLib.Util;

namespace SynergyTools.ProgramCommands;

public class RootProgramCommand {
    public static readonly Option<bool> OverwriteOption = new(
        "--overwrite",
        () => false,
        "Overwrite the target file if it exists.");

    public static readonly Option<int> CompressionLevelOption = new(
        "--compression-level",
        () => new WiiuStreamFile.SaveConfig().CompressionLevel,
        "Specify the effort for compressing files.\n" +
        $"Use {new WiiuStreamFile.SaveConfig().CompressionLevel} to use " +
        $"{WiiuStreamFile.SaveConfig.CompressionLevelIfAuto} if chunking is disabled, " +
        "and otherwise, use chunk size.\n" +
        "Use 0 to disable compression.");

    public static readonly Option<int> CompressionChunkSizeOption = new(
        "--compression-chunk-size",
        () => new WiiuStreamFile.SaveConfig().CompressionChunkSize,
        "Specify the compression block size. Use 0 to disable chunking.");

    static RootProgramCommand() {
        OverwriteOption.AddAlias("-y");
        CompressionLevelOption.AddAlias("-l");
        CompressionChunkSizeOption.AddAlias("-c");
        
        Command.AddGlobalOption(CompressionLevelOption);
        Command.AddGlobalOption(CompressionChunkSizeOption);
        Command.AddGlobalOption(OverwriteOption);
        
        Command.AddCommand(ExtractProgramCommand.Command);
        Command.AddCommand(CompressProgramCommand.Command);
        Command.AddCommand(QuickModProgramCommand.Command);
        Command.AddCommand(ConvertToGltfProgramCommand.Command);
        Command.AddCommand(TestDevProgramCommand.Command);
    }

    public static readonly Command Command = new RootCommand(
        "Tool for modding Sonic Boom: Rise of Lyric.\n" +
        "Github: https://github.com/Soreepeong/SynergyTools");

    public readonly bool Overwrite;
    public readonly int CompressionLevel;
    public readonly int CompressionChunkSize;

    public RootProgramCommand(ParseResult parseResult) {
        Overwrite = parseResult.GetValueForOption(OverwriteOption);
        CompressionLevel = parseResult.GetValueForOption(CompressionLevelOption);
        CompressionChunkSize = parseResult.GetValueForOption(CompressionChunkSizeOption);
    }

    public static Task<int> InvokeFromArgsAsync(string[] args) {
        // Special case when we received 1 argument (excluding application name),
        // and the second parameter is an existing folder or file.
        if (args.Length == 1
            && !Command.Subcommands.Any(x => x.Aliases.Contains(args[0]))
            && Path.Exists(args[0])) {
            if (Directory.Exists(args[0])) {
                if (Path.Exists(Path.Combine(args[0], WiiuStreamFile.MetadataFilename))) {
                    using (ScopedConsoleColor.Foreground(ConsoleColor.Yellow))
                        Console.WriteLine("Assuming {0} with default options.", CompressProgramCommand.Command.Name);
                    args = new[] {CompressProgramCommand.Command.Name, args[0]};
                }
            } else if (File.Exists(args[0])) {
                Span<byte> peekResult = stackalloc byte[Math.Max(WiiuStreamFile.Magic.Length, PbxmlFile.Magic.Length)];
                using (var peeker = File.OpenRead(args[0]))
                    peekResult = peekResult[..peeker.Read(peekResult)];

                if (peekResult.StartsWith(WiiuStreamFile.Magic.AsSpan())) {
                    using (ScopedConsoleColor.Foreground(ConsoleColor.Yellow))
                        Console.WriteLine("Assuming {0} with default options.", ExtractProgramCommand.Command.Name);
                    args = new[] {ExtractProgramCommand.Command.Name, args[0]};
                }
            }
        }

        return Command.InvokeAsync(args);
    }
}
