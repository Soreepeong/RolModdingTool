using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
using WiiUStreamTool.FileFormat;

namespace WiiUStreamTool.ProgramCommands;

public class QuickModProgramCommand : RootProgramCommand {
    public new static readonly Command Command = new("quickmod");

    public static readonly Argument<string[]> PathArgument = new(
        "path",
        "Specify root content directory, such as \"C:\\mlc01\\usr\\title\\00050000\\10175b00\\content\".\n" +
        "If you have an update applied, specify the update first, and then the base game next.") {
        Arity = ArgumentArity.OneOrMore,
    };

    public static readonly Option<SonicClones> ModeOption = new(
        "--mode",
        () => SonicClones.Default,
        "Specify which character to replace Sonic.");

    private static readonly Tuple<string, int>[] DesaturationTargetTextures = {
        Tuple.Create("art/textures/effects/playerfx/ball_blue.dds", 18),
        Tuple.Create("art/textures/effects/playerfx/glide_sonic.dds", 3),
        Tuple.Create("art/textures/effects/playerfx/glide_sonic_soft.dds", 3),
        Tuple.Create("art/textures/effects/playerfx/bungee_sonic.dds", 3),
        Tuple.Create("art/textures/effects/playerfx/bungee_sonic_additive.dds", 3),
    };

    static QuickModProgramCommand() {
        Command.AddAlias("qm");
        Command.AddAlias("metadow");
        Command.AddArgument(PathArgument);
        ModeOption.AddAlias("-m");
        Command.AddOption(ModeOption);
        CompressionLevelOption.AddAlias("-l");
        Command.AddOption(CompressionLevelOption);
        CompressionChunkSizeOption.AddAlias("-c");
        Command.AddOption(CompressionChunkSizeOption);
        Command.SetHandler(ic => new QuickModProgramCommand(ic.ParseResult).Handle(ic.GetCancellationToken()));
    }

    public readonly string[] PathArray;
    public readonly SonicClones Mode;

    public WiiuStreamFile? heroes;
    public Dictionary<string, WiiuStreamFile> levels = new();

    public QuickModProgramCommand(ParseResult parseResult) : base(parseResult) {
        PathArray = parseResult.GetValueForArgument(PathArgument);
        Mode = parseResult.GetValueForOption(ModeOption);
    }

    public async Task<int> Handle(CancellationToken cancellationToken) {
        heroes = null;
        levels.Clear();

        string? heroesPath = null;
        var levelPaths = new Dictionary<string, string>();

        Console.WriteLine("Looking for files to patch...");
        foreach (var inPath in PathArray) {
            var levelsPath = Path.Join(inPath, "Sonic_Crytek", "Levels");
            if (!Directory.Exists(levelsPath))
                throw new DirectoryNotFoundException("Given path does not contain Sonic_Crytek\\Level folder.");

            if (heroesPath is null) {
                heroesPath = Path.Join(inPath, "Sonic_Crytek", "heroes.wiiu.stream");
                var bakFile = heroesPath + ".bak";
                if (Mode == SonicClones.Sonic) {
                    if (File.Exists(bakFile)) {
                        File.Delete(heroesPath);
                        File.Move(bakFile, heroesPath);
                        Console.WriteLine(
                            "Restored: {0} from {1}",
                            Path.GetFileName(heroesPath),
                            Path.GetFileName(bakFile));
                    }
                } else if (File.Exists(heroesPath)) {
                    if (!File.Exists(bakFile)) {
                        File.Copy(heroesPath, bakFile);
                        Console.WriteLine("Made a backup copy: {0}", Path.GetFileName(bakFile));
                    }

                    heroes = new();
                    heroes.ReadFrom(null, bakFile, cancellationToken);
                } else {
                    heroesPath = null;
                }
            }

            foreach (var levelPath in Directory.GetFiles(levelsPath)) {
                if (!levelPath.EndsWith(".wiiu.stream", StringComparison.InvariantCultureIgnoreCase))
                    continue;

                var bakFile = levelPath + ".bak";
                if (Mode == SonicClones.Sonic) {
                    if (File.Exists(bakFile)) {
                        File.Delete(levelPath);
                        File.Move(bakFile, levelPath);
                        Console.WriteLine(
                            "Restored: {0} from {1}",
                            Path.GetFileName(levelPath),
                            Path.GetFileName(bakFile));
                    }

                    continue;
                }

                var key = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(levelPath))
                    .ToLowerInvariant();
                if (levels.ContainsKey(key))
                    continue;

                if (!File.Exists(bakFile)) {
                    File.Copy(levelPath, bakFile);
                    Console.WriteLine("Made a backup copy: {0}", Path.GetFileName(bakFile));
                }

                var strm = new WiiuStreamFile();
                strm.ReadFrom(null, bakFile, cancellationToken);
                levels.Add(key, strm);
                levelPaths.Add(key, levelPath);
            }
        }

        if (Mode == SonicClones.Sonic) {
            Console.WriteLine("All files restored.");
            return 0;
        }

        if (heroes is null || heroesPath is null)
            throw new FileNotFoundException("heroes.wiiu.stream could not be found.");

        Console.WriteLine("Patching data...");

        if (Mode == SonicClones.Shadow) {
            await Task.WhenAll(levels.Values.Select(level => ShadowAdjustGrunts(level, cancellationToken)));
            await ShadowAdjustSpinDashBallColor(cancellationToken);
        }

        await Task.WhenAll(levels.Values.Select(level => CommonReplaceModels(level, cancellationToken)));
        await CommonAddTextures(cancellationToken);

        Console.WriteLine("Saving data...");
        var saveConfig = new WiiuStreamFile.SaveConfig {
            CompressionLevel = CompressionLevel,
            CompressionChunkSize = CompressionChunkSize,
        };

        var suppressProgressDuration = TimeSpan.FromSeconds(5);
        await CompressProgramCommand.WriteAndPrintProgress(
            heroesPath,
            heroes,
            saveConfig,
            cancellationToken,
            suppressProgressDuration);
        foreach (var (levelName, level) in levels)
            await CompressProgramCommand.WriteAndPrintProgress(
                levelPaths[levelName],
                level,
                saveConfig,
                cancellationToken,
                suppressProgressDuration);

        Console.WriteLine("Done!");

        return 0;
    }

    private async Task ShadowAdjustSpinDashBallColor(
        CancellationToken cancellationToken) {
        // Turn Sonic's blue spindash ball grey
        var decoder = new BcDecoder();
        var encoder = new BcEncoder {
            OutputOptions = {
                GenerateMipMaps = false,
                Quality = CompressionQuality.BestQuality,
                FileFormat = OutputFileFormat.Dds,
            }
        };

        using var ms = new MemoryStream();
        foreach (var (filename, divisor) in DesaturationTargetTextures) {
            var entry = heroes!.GetEntry(filename);
            var image = await decoder.DecodeToImageRgba32Async(
                new MemoryStream(entry.Source.ReadRaw()),
                cancellationToken);

            var hasAlpha = false;
            image.ProcessPixelRows(
                row => {
                    for (var y = 0; y < row.Height; y++) {
                        var span = row.GetRowSpan(y);
                        for (var i = 0; i < span.Length; i++) {
                            span[i].R = span[i].G = span[i].B =
                                (byte) ((span[i].R + span[i].G + span[i].B) / divisor);
                            hasAlpha |= span[i].A != 255;
                        }
                    }
                });
            encoder.OutputOptions.Format = hasAlpha ? CompressionFormat.Bc3 : CompressionFormat.Bc1;

            ms.Position = 0;
            ms.SetLength(0);
            await encoder.EncodeToStreamAsync(image, ms, cancellationToken);
            entry.Source = new(ms.ToArray());
        }
    }

    private static Task ShadowAdjustGrunts(WiiuStreamFile level, CancellationToken cancellationToken) =>
        Task.Run(
            () => {
                using var ms = new MemoryStream();
                var entry = level.GetEntry("animations/characters/1_heroes/sonic/default.animevents");
                var rawBytes = entry.Source.ReadRaw();
                string data;
                if (PbxmlFile.IsPbxmlFile(rawBytes.AsSpan())) {
                    ms.SetLength(ms.Position = 0);
                    var inStream = new StreamWriter(ms, new UTF8Encoding());
                    PbxmlFile.Unpack(new(new MemoryStream(rawBytes)), inStream);
                    data = Encoding.UTF8.GetString(ms.ToArray());
                } else
                    data = Encoding.UTF8.GetString(rawBytes);

                data = data.Replace("Sounds:exertions/hero_sonic:Sonic_", "Sounds:exertions/shadow:Shadow_");
                data = data.Replace("death_via_tunnelbot", "deathfire_noplay");
                data = data.Replace("slingshotpull_noplay", "bungeethrow_noplay");

                ms.SetLength(ms.Position = 0);
                PbxmlFile.Pack(new MemoryStream(Encoding.UTF8.GetBytes(data)), new(ms));
                entry.Source = new(ms.ToArray());
            },
            cancellationToken);

    private Task CommonAddTextures(CancellationToken cancellationToken) => Task.Run(
        () => {
            var referenceLevel = levels[Mode switch {
                SonicClones.Shadow => "level02_ancientfactorypresent_a",
                SonicClones.MetalSonic => "level05_sunkenruins",
                _ => throw new InvalidOperationException(),
            }];
            var replacementPathPrefix = Mode switch {
                SonicClones.Shadow => "art/characters/5_minibosses/shadow/texture/",
                SonicClones.MetalSonic => "art/characters/5_minibosses/metal_sonic/textures/",
                _ => throw new InvalidOperationException(),
            };

            foreach (var entry in referenceLevel.Entries) {
                if (!entry.Header.InnerPath.StartsWith(
                        replacementPathPrefix,
                        StringComparison.InvariantCultureIgnoreCase))
                    continue;
                heroes!.PutEntry(entry.Header.InnerPath, entry.Source);
            }
        },
        cancellationToken);

    private Task CommonReplaceModels(WiiuStreamFile level, CancellationToken cancellationToken) => Task.Run(
        () => {
            var sonicBaseFile = "objects/characters/1_heroes/sonic/sonic";
            var referenceLevel = levels[Mode switch {
                SonicClones.Shadow => "level02_ancientfactorypresent_a",
                SonicClones.MetalSonic => "level05_sunkenruins",
                _ => throw new InvalidOperationException(),
            }];
            var replacementBaseFile = Mode switch {
                SonicClones.Shadow => "objects/characters/5_minibosses/shadow/shadow",
                SonicClones.MetalSonic => "objects/characters/5_minibosses/metal_sonic/metal_sonic",
                _ => throw new InvalidOperationException(),
            };

            var sonicChr = level.GetEntry($"{sonicBaseFile}.chr");
            var replacementChr = referenceLevel.GetEntry($"{replacementBaseFile}.chr");
            sonicChr.Source = replacementChr.Source;

            var sonicMtl = level.GetEntry($"{sonicBaseFile}.mtl", SkinFlag.Sonic_Default);
            var sonicMtlAlt = level.GetEntry($"{sonicBaseFile}.mtl", SkinFlag.Sonic_Alt);
            var replacementMtl = referenceLevel.GetEntry($"{replacementBaseFile}.mtl");

            var sonicDoc = PbxmlFile.Load(sonicMtl.Source.ReadRaw());
            var replacementDoc = PbxmlFile.Load(replacementMtl.Source.ReadRaw());
            var existingNames = replacementDoc["Material"]!["SubMaterials"]!
                .OfType<XmlElement>()
                .Select(x => x.GetAttribute("Name"))
                .ToHashSet();
            foreach (var elem in sonicDoc["Material"]!["SubMaterials"]!.OfType<XmlElement>()) {
                if (existingNames.Contains(elem.GetAttribute("Name")))
                    continue;

                replacementDoc["Material"]!["SubMaterials"]!.AppendChild(replacementDoc.ImportNode(elem, true));
                existingNames.Add(elem.GetAttribute("Name"));
            }

            using var targetMs = new MemoryStream();
            PbxmlFile.Pack(replacementDoc, new(targetMs));
            sonicMtl.Source = sonicMtlAlt.Source = new(targetMs.ToArray());
        },
        cancellationToken);

    public enum SonicClones {
        Sonic = 0,
        Shadow = 1,
        MetalSonic = 2,

        Default = Sonic,
        Revert = Sonic,
        Metal = MetalSonic,
    }
}
