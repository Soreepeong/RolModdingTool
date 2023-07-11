using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WiiUStreamTool.FileFormat;
using WiiUStreamTool.Util;

namespace WiiUStreamTool.ProgramCommands;

public class ExtractProgramCommand : RootProgramCommand {
    public new static readonly Command Command = new("extract");

    public static readonly Argument<string> PathArgument = new(
        "path",
        "Specify path to a .wiiu.stream archive.");

    public static readonly Option<string?> OutPathOption = new(
        "--out-path",
        () => null,
        "Specify target directory. Defaults to filename without extension.");

    public static readonly Option<bool> PreservePbxmlOption = new(
        "--preserve-pbxml",
        () => false,
        "Keep packed binary XML files as-is.");

    static ExtractProgramCommand() {
        Command.AddAlias("e");
        Command.AddArgument(PathArgument);
        OutPathOption.AddAlias("-o");
        Command.AddOption(OutPathOption);
        PreservePbxmlOption.AddAlias("-p");
        Command.AddOption(PreservePbxmlOption);
        Command.SetHandler(ic => new ExtractProgramCommand(ic.ParseResult).Handle(ic.GetCancellationToken()));
    }

    public readonly string InPath;
    public readonly string OutPath;
    public readonly bool PreservePbxml;

    public ExtractProgramCommand(ParseResult parseResult) : base(parseResult) {
        InPath = parseResult.GetValueForArgument(PathArgument);
        OutPath = parseResult.GetValueForOption(OutPathOption) ?? Path.Combine(
            Path.GetDirectoryName(InPath) ?? Environment.CurrentDirectory,
            Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(InPath)));
        PreservePbxml = parseResult.GetValueForOption(PreservePbxmlOption);
    }

    public async Task<int> Handle(CancellationToken cancellationToken) {
        try {
            await using var f = File.OpenRead(InPath);
            await WiiUStream.Extract(
                f,
                OutPath,
                PreservePbxml,
                Overwrite,
                (ref WiiUStream.FileEntryHeader fe, long progress, long max, bool skipped, bool complete) => {
                    if (complete) {
                        Console.Write(
                            "[{0:00.00}%] {1} ({2:##,###} bytes){3}... ",
                            100.0 * progress / max,
                            fe.InnerPath,
                            fe.DecompressedSize,
                            skipped ? " [SKIPPED]" : "");
                        if (skipped)
                            Console.WriteLine();
                    } else {
                        Console.WriteLine(" done!");
                    }
                },
                cancellationToken);
            Console.WriteLine("Done!");
            return 0;
        } catch (OperationCanceledException) {
            using (ScopedConsoleColor.Foreground(ConsoleColor.Yellow))
                Console.WriteLine("Cancelled per user request.");
            return -1;
        }
    }
}
