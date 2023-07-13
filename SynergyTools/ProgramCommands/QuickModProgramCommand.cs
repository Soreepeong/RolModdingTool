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
using SynergyLib.FileFormat;
using SynergyTools.Misc;

namespace SynergyTools.ProgramCommands;

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

    public static readonly Dictionary<string, string> SonicToShadowExertionMap = new() {
        ["Sonic_disgusted_17"] = "Shadow_disgusted",
        ["Sonic_disgusted_18"] = "Shadow_disgusted",
        ["Sonic_disgusted_19"] = "Shadow_disgusted",
        ["Sonic_disgusted_21"] = "Shadow_disgusted",
        ["Sonic_boost_pad"] = "",
        ["Sonic_boost_ring_gate"] = "",
        ["Sonic_boost_ring_gate_super"] = "",
        ["Sonic_bouncepad"] = "Shadow_sprintbegin",
        ["Sonic_bungee_pull_target_2D"] = "",
        ["Sonic_buttstomp_button"] = "",
        ["Sonic_buttstomp_button_and_breakable"] = "",
        ["Sonic_buttstomp_compliment"] = "",
        ["Sonic_charselect"] = "Shadow_positive",
        ["Sonic_collect_shiny"] = "Shadow_laugh",
        ["Sonic_combat_begin"] = "",
        ["Sonic_death_via_combat"] = "Shadow_frustrated",
        ["Sonic_death_via_drown"] = "Shadow_frustrated",
        ["Sonic_death_via_fall"] = "Shadow_frustrated",
        ["Sonic_death_via_hazard"] = "Shadow_frustrated",
        ["Sonic_death_via_road"] = "Shadow_frustrated",
        ["Sonic_death_via_tunnelbot"] = "Shadow_frustrated",
        ["Sonic_modal_weapon_pickup"] = "",
        ["Sonic_slingshotpull_noplay"] = "Shadow_bungeethrow_noplay",
        ["Sonic_spindash"] = "Shadow_dashbegin",
        ["Sonic_warthog_begin"] = "",
        ["Sonic_warthog_running"] = "",
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
        string? soundsPath = null;
        var levelPaths = new Dictionary<string, string>();

        Console.WriteLine("Looking for files to patch...");
        foreach (var inPath in PathArray) {
            var levelsPath = Path.Join(inPath, "Sonic_Crytek", "Levels");
            if (!Directory.Exists(levelsPath))
                throw new DirectoryNotFoundException("Given path does not contain Sonic_Crytek\\Level folder.");

            if (Directory.Exists(Path.Join(inPath, "Sonic_Crytek", "Sounds")))
                soundsPath = Path.Join(inPath, "Sonic_Crytek", "Sounds");

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
            if (soundsPath is not null) {
                foreach (var f in Directory.GetFiles(soundsPath)) {
                    if (f.EndsWith(".acb.bak"))
                        File.Move(f, f[..^4], true);
                    else if (f.EndsWith(".awb.bak"))
                        File.Move(f, f[..^4], true);
                }

                foreach (var f in Directory.GetFiles(Path.Join(soundsPath, "exertions"))) {
                    if (f.EndsWith(".acb.bak"))
                        File.Move(f, f[..^4], true);
                    else if (f.EndsWith(".awb.bak"))
                        File.Move(f, f[..^4], true);
                }
            }

            Console.WriteLine("All files restored.");
            return 0;
        }

        if (heroes is null || heroesPath is null)
            throw new FileNotFoundException("heroes.wiiu.stream could not be found.");

        if (soundsPath is null)
            throw new DirectoryNotFoundException("Sounds folder could not be found.");

        Console.WriteLine("Patching data...");

        await CommonSilenceVoices(soundsPath, cancellationToken);
        await CommonAdjustSonicExertions(soundsPath, cancellationToken);
        return -1;
        
        if (Mode == SonicClones.Shadow) 
            await ShadowAdjustSpinDashBallColor(cancellationToken);
        
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

    private static Task CommonSilenceVoices(string soundsPath, CancellationToken cancellationToken) => Task.Run(
        () => {
            foreach (var f in Directory.GetFiles(soundsPath)) {
                if (!f.EndsWith(".acb"))
                    continue;

                var baseName = f[..^4];
                var acbFile = baseName + ".acb";
                var awbFile = File.Exists(baseName + ".awb") ? baseName + ".awb" : null;
                
                var targetAcb = new AcbFile(
                    File.Exists(acbFile + ".bak") ? acbFile + ".bak" : acbFile,
                    awbFile is not null && File.Exists(awbFile + ".bak") ? awbFile + ".bak" : awbFile);

                // silence all sonic dialogues
                var changed = false;
                foreach (var (cueName, sequenceIndex) in targetAcb.CueNameToSequenceIndex) {
                    if (!cueName.Contains("_Sonic_x_x_"))
                        continue;
                    
                    targetAcb.SetTrackIndices(sequenceIndex, Array.Empty<ushort>());
                    changed = true;
                }

                if (!changed)
                    continue;
                
                if (!File.Exists(acbFile + ".bak"))
                    File.Copy(acbFile, acbFile + ".bak");
                if (awbFile is not null && !File.Exists(awbFile + ".bak"))
                    File.Copy(awbFile, awbFile + ".bak");
                
                targetAcb.Save(baseName);
                Console.WriteLine("Silenced Sonic dialogues in acb/awb file(s): {0}", baseName);
            }
        },
        cancellationToken);

    private Task CommonAdjustSonicExertions(string soundsPath, CancellationToken cancellationToken) => Task.Run(
        () => {
            var baseName = Path.Join(soundsPath, "exertions/hero_sonic");
            var acbFile = baseName + ".acb";
            var awbFile = File.Exists(baseName + ".awb") ? baseName + ".awb" : null;
            if (!File.Exists(acbFile + ".bak"))
                File.Copy(acbFile, baseName + ".acb.bak");
            if (awbFile is not null && !File.Exists(awbFile + ".bak"))
                File.Copy(awbFile, awbFile + ".bak");
            var targetAcb = new AcbFile(acbFile + ".bak", awbFile + ".bak");

            switch (Mode) {
                case SonicClones.Shadow: {
                    var shadowAcb = new AcbFile(Path.Join(soundsPath, "exertions", "shadow.acb"), null);
                    var trackIndicesOfShadowWaveformsInSonic = new List<ushort>(shadowAcb.InternalWaveforms.Count);
                    trackIndicesOfShadowWaveformsInSonic.AddRange(
                        shadowAcb.InternalWaveformRows.Select(
                            row => targetAcb.AddTrack(
                                row,
                                shadowAcb.InternalWaveforms[row.GetValue<ushort>("Id")])));

                    // 1. copy all shadow voices to sonic voices file
                    var translatedTrackIndices = new Dictionary<string, List<ushort>>();
                    foreach (var (key, indices) in shadowAcb.RelevantWaveformIds) {
                        var translated = translatedTrackIndices[key] = new();
                        translated.AddRange(indices.Select(index => trackIndicesOfShadowWaveformsInSonic[index]));
                    }

                    // 2. silence all sonic voices
                    for (var i = 0; i < targetAcb.SequenceTable.Rows.Count; i++)
                        targetAcb.SetTrackIndices(i, Array.Empty<ushort>());

                    // 3. for the voice lines with matching names, put shadow lines
                    foreach (var (cueName, sequenceIndex) in targetAcb.CueNameToSequenceIndex) {
                        var shadowCueName = "Shadow_" + cueName.Split("_", 2)[1];
                        if (!translatedTrackIndices.TryGetValue(shadowCueName, out var indices))
                            continue;
                        targetAcb.SetTrackIndices(sequenceIndex, indices.ToArray());
                    }

                    // 4. apply extra mappings
                    foreach (var (from, to) in SonicToShadowExertionMap) {
                        var sequenceIndex = targetAcb.CueNameToSequenceIndex[from];
                        targetAcb.SetTrackIndices(
                            sequenceIndex,
                            to == "" ? Array.Empty<ushort>() : translatedTrackIndices[to].ToArray());
                    }

                    break;
                }
                case SonicClones.MetalSonic: {
                    // he's silent type
                    for (var i = 0; i < targetAcb.SequenceTable.Rows.Count; i++)
                        targetAcb.SetTrackIndices(i, Array.Empty<ushort>());
                    break;
                }
                case SonicClones.Sonic:
                default:
                    throw new InvalidOperationException();
            }

            targetAcb.Save(baseName);
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
        Restore = Sonic,
        Metal = MetalSonic,
    }
}
