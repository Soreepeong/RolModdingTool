using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WiiUStreamTool.FileFormat;
using WiiUStreamTool.Util;

namespace WiiUStreamTool.ProgramCommands;

public class CompressProgramCommand : RootProgramCommand {
    public const int CompressionLevelDefault = 8;

    public new static readonly Command Command = new("compress");

    public static readonly Argument<string> PathArgument = new("path", "Specify path to a folder to compress.");

    public static readonly Option<string?> OutPathOption = new(
        "--out-path",
        () => null,
        "Specify target path. Defaults to given folder name with .wiiu.stream extension.");

    public static readonly Option<bool> PreserveXmlOption = new(
        "--preserve-xml",
        () => false,
        "Keep text XML files as-is.");

    public static readonly Option<int> CompressionLevelOption = new(
        "--compression-level",
        () => -1,
        "Specify the effort for compressing files.\n" +
        $"Use -1 to use {CompressionLevelDefault} if chunking is disabled, and otherwise, use chunk size.\n" +
        "Use 0 to disable compression.");

    public static readonly Option<int> CompressionChunkSizeOption = new(
        "--compression-chunk-size",
        () => 24576,
        "Specify the compression block size. Use 0 to disable chunking.");

    static CompressProgramCommand() {
        Command.AddAlias("c");
        Command.AddArgument(PathArgument);
        OutPathOption.AddAlias("-o");
        Command.AddOption(OutPathOption);
        PreserveXmlOption.AddAlias("-p");
        Command.AddOption(PreserveXmlOption);
        CompressionLevelOption.AddAlias("-l");
        Command.AddOption(CompressionLevelOption);
        CompressionChunkSizeOption.AddAlias("-c");
        Command.AddOption(CompressionChunkSizeOption);
        Command.SetHandler(ic => new CompressProgramCommand(ic.ParseResult).Handle(ic.GetCancellationToken()));
    }

    public readonly string InPath;
    public readonly string OutPath;
    public readonly bool PreserveXml;
    public readonly int CompressionLevel;
    public readonly int CompressionChunkSize;

    public CompressProgramCommand(ParseResult parseResult) : base(parseResult) {
        InPath = parseResult.GetValueForArgument(PathArgument);
        OutPath = parseResult.GetValueForOption(OutPathOption)
            ?? Path.Combine(Path.GetDirectoryName(InPath)!, Path.GetFileName(InPath) + ".wiiu.stream");
        PreserveXml = parseResult.GetValueForOption(PreserveXmlOption);
        CompressionLevel = parseResult.GetValueForOption(CompressionLevelOption);
        CompressionChunkSize = parseResult.GetValueForOption(CompressionChunkSizeOption);
    }

    public async Task<int> Handle(CancellationToken cancellationToken) {
        if (!Overwrite && Path.Exists(OutPath)) {
            Console.Error.WriteLine("File {0} already exists; aborting. Use -y to overwrite.", OutPath);
            return -1;
        }

        void CompressProgress(int progress, int max, ref WiiUStream.FileEntryHeader header) {
            if (header.DecompressedSize == 0) {
                Console.Write(
                    "[{0:00.00}%] {1}... ",
                    100.0 * progress / max,
                    header.InnerPath);
            } else if (header.CompressedSize == 0) {
                Console.WriteLine("not compressed");
            } else {
                Console.WriteLine(
                    "{0:##,###} bytes to {1:##,###} bytes ({2:00.00}%)",
                    header.DecompressedSize,
                    header.CompressedSize,
                    100.0 * header.CompressedSize / header.DecompressedSize);
            }
        }

        var tmpPath = $"{OutPath}.tmp{Environment.TickCount64:X}";
        try {
            await using (var stream = new FileStream(tmpPath, FileMode.Create))
                await WiiUStream.Compress(
                    InPath,
                    stream,
                    PreserveXml,
                    CompressionLevel == -1
                        ? CompressionChunkSize == 0 ? CompressionLevelDefault : CompressionChunkSize
                        : CompressionLevel,
                    CompressionChunkSize,
                    CompressProgress,
                    cancellationToken);

            if (File.Exists(OutPath))
                File.Replace(tmpPath, OutPath, null);
            else
                File.Move(tmpPath, OutPath);

            Console.WriteLine("Done!");
            return 0;
        } catch (Exception e) {
            try {
                File.Delete(tmpPath);
            } catch (Exception) {
                // swallow
            }

            if (e is OperationCanceledException) {
                using (ScopedConsoleColor.Foreground(ConsoleColor.Yellow))
                    Console.WriteLine("Cancelled per user request.");
                return -1;
            }

            throw;
        }
    }
}
