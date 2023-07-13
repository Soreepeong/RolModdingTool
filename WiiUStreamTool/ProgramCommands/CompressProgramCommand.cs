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
    public new static readonly Command Command = new("compress");

    public static readonly Argument<string[]> PathArgument = new(
        "path",
        "Specify path to a folder to compress.") {
        Arity = ArgumentArity.OneOrMore,
    };

    public static readonly Option<string?> BaseOutPathOption = new(
        "--out-path",
        () => null,
        "Specify target path. Defaults to given folder name with .wiiu.stream extension for each file.");

    public static readonly Option<bool> PreserveXmlOption = new(
        "--preserve-xml",
        () => false,
        "Keep text XML files as-is.");

    static CompressProgramCommand() {
        Command.AddAlias("c");
        Command.AddArgument(PathArgument);
        BaseOutPathOption.AddAlias("-o");
        Command.AddOption(BaseOutPathOption);
        PreserveXmlOption.AddAlias("-p");
        Command.AddOption(PreserveXmlOption);
        Command.SetHandler(ic => new CompressProgramCommand(ic.ParseResult).Handle(ic.GetCancellationToken()));
    }

    public readonly string[] InPathArray;
    public readonly string? BaseOutPath;
    public readonly bool PreserveXml;

    public CompressProgramCommand(ParseResult parseResult) : base(parseResult) {
        InPathArray = parseResult.GetValueForArgument(PathArgument);
        BaseOutPath = parseResult.GetValueForOption(BaseOutPathOption);
        PreserveXml = parseResult.GetValueForOption(PreserveXmlOption);
    }

    public async Task<int> Handle(CancellationToken cancellationToken) {
        var saveConfig = new WiiuStreamFile.SaveConfig() {
            CompressionChunkSize = CompressionChunkSize,
            CompressionLevel = CompressionLevel,
            PreserveXml = PreserveXml,
        };
        foreach (var inPath in InPathArray) {
            var outPath = Path.Combine(
                BaseOutPath ?? Path.GetDirectoryName(inPath)!,
                Path.GetFileName(inPath) + ".wiiu.stream");

            if (!Overwrite && Path.Exists(outPath)) {
                Console.Error.WriteLine("File {0} already exists; skipping. Use -y to overwrite.", outPath);
                continue;
            }

            try {
                var strm = new WiiuStreamFile();
                await using (var s = File.OpenRead(Path.Join(inPath, WiiuStreamFile.MetadataFilename)))
                    await strm.ReadFromMetadata(s, inPath, cancellationToken);

                await WriteAndPrintProgress(outPath, strm, saveConfig, cancellationToken);
            } catch (Exception e) {
                if (e is OperationCanceledException) {
                    using (ScopedConsoleColor.Foreground(ConsoleColor.Yellow))
                        Console.WriteLine("Cancelled per user request.");
                    return -1;
                }

                throw;
            }
        }

        Console.WriteLine("Done!");

        return 0;
    }

    public static async Task WriteAndPrintProgress(
        string outPath,
        WiiuStreamFile strm,
        WiiuStreamFile.SaveConfig saveConfig,
        CancellationToken cancellationToken,
        TimeSpan printProgressDelay = default) {
        var tmpPath = $"{outPath}.{Environment.TickCount64:X}.tmp";
        try {
            var printProgressAfter = Environment.TickCount64 + printProgressDelay.TotalMilliseconds;
            Console.WriteLine("Saving: {0}", outPath);
            await using (var stream = new FileStream(tmpPath, FileMode.Create)) {
                await foreach (var (progress, max, entry, entryComplete) in strm.WriteTo(
                                   stream,
                                   saveConfig,
                                   cancellationToken)) {
                    if (Environment.TickCount64 < printProgressAfter)
                        continue;
                    
                    if (!entryComplete) {
                        Console.Write(
                            "[{0:00.00}%] {1}... ",
                            100.0 * progress / max,
                            entry.Header.InnerPath);
                    } else if (entry.Header.CompressedSize == 0) {
                        Console.WriteLine("not compressed");
                    } else {
                        Console.WriteLine(
                            "{0:##,###} bytes to {1:##,###} bytes ({2:00.00}%)",
                            entry.Header.DecompressedSize,
                            entry.Header.CompressedSize,
                            100.0 * entry.Header.CompressedSize / entry.Header.DecompressedSize);
                    }
                }
            }

            File.Move(tmpPath, outPath, true);
        } catch (Exception) {
            try {
                File.Delete(tmpPath);
            } catch (Exception) {
                // swallow
            }

            throw;
        }
    }
}
