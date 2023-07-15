using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SynergyLib.FileFormat;
using SynergyLib.Util;
using SynergyLib.Util.BinaryRW;

namespace SynergyTools.ProgramCommands;

public class ExtractProgramCommand : RootProgramCommand {
    public new static readonly Command Command = new("extract");

    public static readonly Argument<string[]> PathArgument = new(
        "path",
        "Specify path to a .wiiu.stream archive.") {
        Arity = ArgumentArity.OneOrMore,
    };

    public static readonly Option<string?> BaseOutPathOption = new(
        "--out-path",
        () => null,
        "Specify base target directory. Defaults to filename without extension for each directory.");

    public static readonly Option<bool> PreservePbxmlOption = new(
        "--preserve-pbxml",
        () => false,
        "Keep packed binary XML files as-is.");

    static ExtractProgramCommand() {
        Command.AddAlias("e");
        Command.AddArgument(PathArgument);
        BaseOutPathOption.AddAlias("-o");
        Command.AddOption(BaseOutPathOption);
        PreservePbxmlOption.AddAlias("-p");
        Command.AddOption(PreservePbxmlOption);
        Command.SetHandler(ic => new ExtractProgramCommand(ic.ParseResult).Handle(ic.GetCancellationToken()));
    }

    public readonly string[] InPathArray;
    public readonly string? BaseOutPath;
    public readonly bool PreservePbxml;

    public ExtractProgramCommand(ParseResult parseResult) : base(parseResult) {
        InPathArray = parseResult.GetValueForArgument(PathArgument);
        BaseOutPath = parseResult.GetValueForOption(BaseOutPathOption);
        PreservePbxml = parseResult.GetValueForOption(PreservePbxmlOption);
    }

    public async Task<int> Handle(CancellationToken cancellationToken) {
        try {
            foreach (var inPath in InPathArray) {
                var outPath = Path.Join(
                    BaseOutPath ?? Path.GetDirectoryName(inPath),
                    Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(inPath)));

                Console.WriteLine("Working on: {0}", inPath);
                Console.WriteLine("Saving into: {0}", outPath);

                var strm = new WiiuStreamFile();
                strm.ReadFrom(null, inPath, cancellationToken);

                Directory.CreateDirectory(outPath);
                await using (var s = File.OpenWrite(Path.Join(outPath, WiiuStreamFile.MetadataFilename)))
                    await strm.WriteMetadata(s);

                var maxProgress = strm.Entries.Sum(x => x.Source.StoredLength);
                var currentProgress = 0;
                using var ms = new MemoryStream();
                using var msr = new NativeReader(ms);
                foreach (var entry in strm.Entries) {
                    var localPath = Path.Join(outPath, entry.Header.LocalPath);

                    Console.Write(
                        "[{0:00.00}%] {1} ({2:##,###} bytes)... ",
                        100.0 * currentProgress / maxProgress,
                        entry.Header.InnerPath,
                        entry.Header.DecompressedSize);

                    if (!Overwrite && Path.Exists(localPath)) {
                        currentProgress += entry.Source.StoredLength;
                        Console.WriteLine("not overwriting");
                        continue;
                    }

                    ms.SetLength(entry.Header.DecompressedSize);
                    ms.Position = 0;
                    entry.Source.ReadRawInto(ms);

                    Directory.CreateDirectory(Path.GetDirectoryName(localPath)!);
                    var tempPath = $"{localPath}.tmp{Environment.TickCount64:X}";
                    try {
                        await using (var target = new FileStream(tempPath, FileMode.Create)) {
                            msr.BaseStream.Position = 0;
                            if (!PreservePbxml)
                                PbxmlFile.FromReader(msr).WriteText(target);
                            else
                                await target.WriteAsync(ms.GetBuffer().AsMemory(0, (int) ms.Length), cancellationToken);
                        }

                        File.Move(tempPath, localPath, true);

                        currentProgress += entry.Source.StoredLength;
                    } catch (Exception) {
                        try {
                            File.Delete(tempPath);
                        } catch (Exception) {
                            // swallow
                        }

                        throw;
                    }

                    Console.WriteLine("done.");
                }
            }

            Console.WriteLine("Complete!");
            return 0;
        } catch (OperationCanceledException) {
            using (ScopedConsoleColor.Foreground(ConsoleColor.Yellow))
                Console.WriteLine("Cancelled per user request.");
            return -1;
        }
    }
}
