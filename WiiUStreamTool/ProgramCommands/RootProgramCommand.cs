using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WiiUStreamTool.FileFormat;
using WiiUStreamTool.Util;

namespace WiiUStreamTool.ProgramCommands;

public class RootProgramCommand {
    public static readonly Option<bool> OverwriteOption = new(
        "--overwrite",
        () => false,
        "Overwrite the target file if it exists.");

    static RootProgramCommand() {
        OverwriteOption.AddAlias("-y");

        Command.AddGlobalOption(OverwriteOption);
        Command.AddCommand(ExtractProgramCommand.Command);
        Command.AddCommand(CompressProgramCommand.Command);
    }

    public static readonly Command Command = new RootCommand(
        "Tool for extracting and repacking Sonic Boom: Rise of Lyric wiiu.stream archives.\nGithub: https://github.com/ik-01/WiiUStreamTool");
    
    public readonly bool Overwrite;

    public RootProgramCommand(ParseResult parseResult) {
        Overwrite = parseResult.GetValueForOption(OverwriteOption);
    }

    public static Task<int> InvokeFromArgsAsync(string[] args) {
        // Special case when we received 1 argument (excluding application name),
        // and the second parameter is an existing folder or file.
        if (args.Length == 1
            && !Command.Subcommands.Any(x => x.Aliases.Contains(args[0]))
            && Path.Exists(args[0])) {
            if (Directory.Exists(args[0])) {
                if (Path.Exists(Path.Combine(args[0], WiiUStream.MetadataFilename))) {
                    using (ScopedConsoleColor.Foreground(ConsoleColor.Yellow))
                        Console.WriteLine("Assuming {0} with default options.", CompressProgramCommand.Command.Name);
                    args = new[] {CompressProgramCommand.Command.Name, args[0]};
                }
            } else if (File.Exists(args[0])) {
                Span<byte> peekResult = stackalloc byte[Math.Max(WiiUStream.Magic.Length, Pbxml.Magic.Length)];
                using (var peeker = File.OpenRead(args[0]))
                    peekResult = peekResult[..peeker.Read(peekResult)];

                if (peekResult.StartsWith(WiiUStream.Magic.AsSpan())) {
                    using (ScopedConsoleColor.Foreground(ConsoleColor.Yellow))
                        Console.WriteLine("Assuming {0} with default options.", ExtractProgramCommand.Command.Name);
                    args = new[] {ExtractProgramCommand.Command.Name, args[0]};
                }
            }
        }

        return Command.InvokeAsync(args);
    }
}