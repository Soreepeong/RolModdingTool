using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
using SynergyLib.FileFormat;
using SynergyLib.FileFormat.CryEngine;
using SynergyLib.FileFormat.CryEngine.CryXml;
using SynergyLib.Util;
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

    public static readonly Option<Characters> ModeOption = new(
        "--mode",
        () => Characters.Default,
        "Specify which character to replace Sonic.");

    public static readonly Option<CharacterVoices> VoiceOption = new(
        "--voice",
        () => CharacterVoices.Auto,
        "Specify which voice to replace Sonic's voice.");

    private const string SonicBaseName = "objects/characters/1_heroes/sonic/sonic";

    private static readonly Tuple<string, int>[] DesaturationTargetTextures = {
        Tuple.Create("art/textures/effects/playerfx/ball_blue.dds", 0),
        Tuple.Create("art/textures/effects/playerfx/glide_sonic.dds", 1),
        Tuple.Create("art/textures/effects/playerfx/glide_sonic_soft.dds", 1),
        Tuple.Create("art/textures/effects/playerfx/bungee_sonic.dds", 1),
        Tuple.Create("art/textures/effects/playerfx/bungee_sonic_additive.dds", 1),
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

    public static readonly Dictionary<string, string> SonicToShadowAnimationMap = new() {
        ["animations/characters/1_heroes/sonic/final/additive/rec_add.caf"] =
            "animations/characters/5_minibosses/shadow/final/additive/rec_high_add.caf",

        ["animations/characters/1_heroes/sonic/final/msc_taunt.caf"] =
            "animations/characters/5_minibosses/shadow/final/msc/msc_taunt01.caf",

        ["animations/characters/1_heroes/sonic/final/combat_idle.caf"] =
            "animations/characters/5_minibosses/shadow/final/nav/nav_idle.caf",

        ["animations/characters/1_heroes/sonic/final/atk_pri_01.caf"] =
            "animations/characters/5_minibosses/shadow/final/atk/atk_flip_kick.caf",
        ["animations/characters/1_heroes/sonic/final/atk_pri_02.caf"] =
            "animations/characters/5_minibosses/shadow/final/atk/atk_roundhouse.caf",
        ["animations/characters/1_heroes/sonic/final/atk_pri_03.caf"] =
            "animations/characters/5_minibosses/shadow/final/atk/atk_rush.caf",

        ["animations/characters/1_heroes/sonic/final/run_fast.caf"] =
            "animations/characters/5_minibosses/shadow/final/nav/nav_skate_fwd.caf",
        ["animations/characters/1_heroes/sonic/final/run_fast_attack.caf"] =
            "animations/characters/5_minibosses/shadow/final/atk/atk_rush.caf",
        ["animations/characters/1_heroes/sonic/final/run_fast_dash_left.caf"] =
            "animations/characters/5_minibosses/shadow/final/nav/nav_skate_left.caf",
        ["animations/characters/1_heroes/sonic/final/run_fast_dash_right.caf"] =
            "animations/characters/5_minibosses/shadow/final/nav/nav_skate_right.caf",

        ["animations/characters/1_heroes/sonic/final/run_16.caf"] =
            "animations/characters/5_minibosses/shadow/final/nav/nav_fly_strafe_f.caf",
        ["animations/characters/1_heroes/sonic/final/run_16_left.caf"] =
            "animations/characters/5_minibosses/shadow/final/nav/nav_fly_strafe_l.caf",
        ["animations/characters/1_heroes/sonic/final/run_16_right.caf"] =
            "animations/characters/5_minibosses/shadow/final/nav/nav_fly_strafe_r.caf",
        ["animations/characters/1_heroes/sonic/final/run_30.caf"] =
            "animations/characters/5_minibosses/shadow/final/nav/nav_fly_strafe_f.caf",
        ["animations/characters/1_heroes/sonic/final/run_30_left.caf"] =
            "animations/characters/5_minibosses/shadow/final/nav/nav_fly_strafe_l.caf",
        ["animations/characters/1_heroes/sonic/final/run_30_right.caf"] =
            "animations/characters/5_minibosses/shadow/final/nav/nav_fly_strafe_r.caf",

        ["animations/characters/1_heroes/sonic/final/warthog_idle.caf"] =
            "animations/characters/5_minibosses/shadow/final/nav/nav_fly_idle.caf",
        ["animations/characters/1_heroes/sonic/final/warthog_run.caf"] =
            "animations/characters/5_minibosses/shadow/final/nav/nav_fly_strafe_f.caf",
        ["animations/characters/1_heroes/sonic/final/warthog_run_lean_left.caf"] =
            "animations/characters/5_minibosses/shadow/final/nav/nav_fly_strafe_l.caf",
        ["animations/characters/1_heroes/sonic/final/warthog_run_lean_right.caf"] =
            "animations/characters/5_minibosses/shadow/final/nav/nav_fly_strafe_r.caf",
    };

    public static readonly Dictionary<string, string> SonicToMetalAnimationMap = new() {
        ["animations/characters/1_heroes/sonic/final/combat_idle.caf"] =
            "animations/characters/5_minibosses/metal_sonic/test/combat_idle.caf",

        ["animations/characters/1_heroes/sonic/final/run_5.caf"] =
            "animations/characters/5_minibosses/metal_sonic/test/run.caf",
        ["animations/characters/1_heroes/sonic/final/run_8.caf"] =
            "animations/characters/5_minibosses/metal_sonic/test/run.caf",
        ["animations/characters/1_heroes/sonic/final/run_8_notran.caf"] =
            "animations/characters/5_minibosses/metal_sonic/test/run.caf",

        ["animations/characters/1_heroes/sonic/final/run_16.caf"] =
            "animations/characters/5_minibosses/metal_sonic/test/run.caf",
        ["animations/characters/1_heroes/sonic/final/run_30.caf"] =
            "animations/characters/5_minibosses/metal_sonic/test/run.caf",
    };

    public static readonly Dictionary<string, string> SonicToSticksAnimationMap = new() {
        ["animations/characters/1_heroes/sonic/final/idle.caf"] =
            "animations/characters/9_majornpc/sticks/final/idle.caf",
    };

    static QuickModProgramCommand() {
        Command.AddAlias("qm");
        Command.AddArgument(PathArgument);
        ModeOption.AddAlias("-m");
        Command.AddOption(ModeOption);
        VoiceOption.AddAlias("-v");
        Command.AddOption(VoiceOption);
        Command.SetHandler(ic => new QuickModProgramCommand(ic.ParseResult).Handle(ic.GetCancellationToken()));
    }

    public readonly string[] PathArray;
    public readonly Characters Mode;
    public readonly CharacterVoices Voice;

    public string? SoundsPath;
    public string? HeroesPath;
    public WiiuStreamFile? Heroes;
    public readonly Dictionary<string, WiiuStreamFile> Levels = new();

    public QuickModProgramCommand(ParseResult parseResult) : base(parseResult) {
        PathArray = parseResult.GetValueForArgument(PathArgument);
        Mode = parseResult.GetValueForOption(ModeOption);
        Voice = parseResult.GetValueForOption(VoiceOption);
    }

    private WiiuStreamFile ReferenceLevel => Levels[Mode switch {
        Characters.Shadow => "level02_ancientfactorypresent_a",
        Characters.MetalSonic => "level05_sunkenruins",
        Characters.Sticks => "hub02_seasidevillage",
        Characters.Sonic => throw new InvalidOperationException(),
        _ => throw new InvalidOperationException(),
    }];

    private string ReferenceObjectBaseName => Mode switch {
        Characters.Shadow => "objects/characters/5_minibosses/shadow/shadow",
        Characters.MetalSonic => "objects/characters/5_minibosses/metal_sonic/metal_sonic",
        Characters.Sticks => "objects/characters/9_majornpc/sticks/sticks",
        Characters.Sonic => throw new InvalidOperationException(),
        _ => throw new InvalidOperationException(),
    };

    private string ReferenceTexturePathPrefix => Mode switch {
        Characters.Shadow => "art/characters/5_minibosses/shadow/texture/",
        Characters.MetalSonic => "art/characters/5_minibosses/metal_sonic/textures/",
        Characters.Sticks => "art/characters/9_majornpc/sticks/textures/",
        Characters.Sonic => throw new InvalidOperationException(),
        _ => throw new InvalidOperationException(),
    };

    public async Task<int> Handle(CancellationToken cancellationToken) {
        Heroes = null;
        Levels.Clear();

        if (Mode == Characters.Sticks)
            using (ScopedConsoleColor.Foreground(ConsoleColor.Yellow))
                Console.WriteLine(
                    "Sticks does not have perfectly compatible bone structures with Sonic, and will " +
                    "not look right.");

        var levelPaths = new Dictionary<string, string>();

        Console.WriteLine("Looking for files to patch...");
        foreach (var inPath in PathArray) {
            var levelsPath = Path.Join(inPath, "Sonic_Crytek", "Levels");
            if (!Directory.Exists(levelsPath))
                throw new DirectoryNotFoundException("Given path does not contain Sonic_Crytek\\Level folder.");

            if (Directory.Exists(Path.Join(inPath, "Sonic_Crytek", "Sounds")))
                SoundsPath = Path.Join(inPath, "Sonic_Crytek", "Sounds");

            if (HeroesPath is null) {
                HeroesPath = Path.Join(inPath, "Sonic_Crytek", "heroes.wiiu.stream");
                var bakFile = HeroesPath + ".bak";
                if (Mode == Characters.Sonic) {
                    if (File.Exists(bakFile)) {
                        File.Delete(HeroesPath);
                        File.Move(bakFile, HeroesPath);
                        Console.WriteLine(
                            "Restored: {0} from {1}",
                            Path.GetFileName(HeroesPath),
                            Path.GetFileName(bakFile));
                    }
                } else if (File.Exists(HeroesPath)) {
                    if (!File.Exists(bakFile)) {
                        File.Copy(HeroesPath, bakFile);
                        Console.WriteLine("Made a backup copy: {0}", Path.GetFileName(bakFile));
                    }

                    Heroes = new();
                    Heroes.ReadFrom(null, bakFile, cancellationToken);
                } else {
                    HeroesPath = null;
                }
            }

            foreach (var levelPath in Directory.GetFiles(levelsPath)) {
                if (!levelPath.EndsWith(".wiiu.stream", StringComparison.OrdinalIgnoreCase))
                    continue;

                var bakFile = levelPath + ".bak";
                if (Mode == Characters.Sonic) {
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
                if (Levels.ContainsKey(key))
                    continue;

                if (!File.Exists(bakFile)) {
                    File.Copy(levelPath, bakFile);
                    Console.WriteLine("Made a backup copy: {0}", Path.GetFileName(bakFile));
                }

                var strm = new WiiuStreamFile();
                strm.ReadFrom(null, bakFile, cancellationToken);
                Levels.Add(key, strm);
                levelPaths.Add(key, levelPath);
            }
        }

        if (SoundsPath is null)
            throw new DirectoryNotFoundException("Sounds folder could not be found.");

        if ((Voice == CharacterVoices.Auto && Mode == Characters.Sonic)
            || Voice == CharacterVoices.Sonic) {
            Console.WriteLine("Reverting voices...");

            foreach (var f in Directory.GetFiles(SoundsPath)) {
                if (f.EndsWith(".acb.bak"))
                    File.Move(f, f[..^4], true);
                else if (f.EndsWith(".awb.bak"))
                    File.Move(f, f[..^4], true);
            }

            foreach (var f in Directory.GetFiles(Path.Join(SoundsPath, "exertions"))) {
                if (f.EndsWith(".acb.bak"))
                    File.Move(f, f[..^4], true);
                else if (f.EndsWith(".awb.bak"))
                    File.Move(f, f[..^4], true);
            }
        } else {
            Console.WriteLine("Patching voices...");
            await PatchSonicDialogues(cancellationToken);
            await PatchSonicExertions(cancellationToken);
        }

        if (Mode != Characters.Sonic) {
            if (Heroes is null || HeroesPath is null)
                throw new FileNotFoundException("heroes.wiiu.stream could not be found.");

            Console.WriteLine("Patching graphics...");

            await Task.WhenAll(
                PatchSonicFxColor(cancellationToken),
                PatchSonicModel(cancellationToken),
                PatchAddCloneTextures(cancellationToken));

            Console.WriteLine("Saving data...");
            var saveConfig = new WiiuStreamFile.SaveConfig {
                CompressionLevel = CompressionLevel,
                CompressionChunkSize = CompressionChunkSize,
            };

            var suppressProgressDuration = TimeSpan.FromSeconds(5);
            await CompressProgramCommand.WriteAndPrintProgress(
                HeroesPath,
                Heroes,
                saveConfig,
                cancellationToken,
                suppressProgressDuration);
            foreach (var (levelName, level) in Levels)
                await CompressProgramCommand.WriteAndPrintProgress(
                    levelPaths[levelName],
                    level,
                    saveConfig,
                    cancellationToken,
                    suppressProgressDuration);
        }

        Console.WriteLine("Done!");

        return 0;
    }

    private Task PatchSonicFxColor(CancellationToken cancellationToken) {
        switch (Mode) {
            case Characters.Shadow:
                return PatchSonicFxColor(
                    cancellationToken,
                    new(
                        1 / 18f,
                        1 / 18f,
                        1 / 18f,
                        0,
                        1 / 18f,
                        1 / 18f,
                        1 / 18f,
                        0,
                        1 / 18f,
                        1 / 18f,
                        1 / 18f,
                        0,
                        0,
                        0,
                        0,
                        1),
                    new(
                        1 / 6f,
                        1 / 6f,
                        1 / 6f,
                        0,
                        1 / 6f,
                        1 / 6f,
                        1 / 6f,
                        0,
                        1 / 6f,
                        1 / 6f,
                        1 / 6f,
                        0,
                        0,
                        0,
                        0,
                        1));
            case Characters.Sticks:
                return PatchSonicFxColor(
                    cancellationToken,
                    new(
                        0,
                        1,
                        0,
                        0,
                        0,
                        1,
                        0,
                        0,
                        1,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        1),
                    new(
                        0,
                        1,
                        0,
                        0,
                        0,
                        1,
                        0,
                        0,
                        1,
                        0,
                        0,
                        0,
                        0,
                        0,
                        0,
                        1));
            case Characters.Sonic:
            case Characters.MetalSonic:
            default:
                return Task.CompletedTask;
        }
    }

    private async Task PatchSonicFxColor(CancellationToken cancellationToken, params Matrix4x4[] patchMatrices) {
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
        foreach (var (filename, patchMatrixIndex) in DesaturationTargetTextures) {
            var entry = Heroes!.GetEntry(filename, false);
            var image = await decoder.DecodeToImageRgba32Async(
                new MemoryStream(entry.Source.ReadRaw(cancellationToken)),
                cancellationToken);

            var hasAlpha = false;
            image.ProcessPixelRows(
                row => {
                    for (var y = 0; y < row.Height; y++) {
                        var span = row.GetRowSpan(y);
                        for (var i = 0; i < span.Length; i++) {
                            var item = new Vector4(span[i].R, span[i].G, span[i].B, span[i].A) / 255;
                            item = Vector4.Transform(item, patchMatrices[patchMatrixIndex]);
                            span[i].R = (byte) float.Clamp(item.X * 255, 0, 255);
                            span[i].G = (byte) float.Clamp(item.Y * 255, 0, 255);
                            span[i].B = (byte) float.Clamp(item.Z * 255, 0, 255);
                            span[i].A = (byte) float.Clamp(item.W * 255, 0, 255);
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

    private Task PatchSonicDialogues(CancellationToken cancellationToken) => Task.Run(
        () => {
            Debug.Assert(SoundsPath is not null);
            foreach (var f in Directory.GetFiles(SoundsPath)) {
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

                    foreach (var trackIndex in targetAcb.GetTrackIndices(sequenceIndex))
                        targetAcb.SilenceTrack(trackIndex);
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

    private void PatchSonicExertionsWith(
        AcbFile targetAcb,
        string sourceAcbFileName,
        string cuePrefix,
        IReadOnlyDictionary<string, string>? extraExertionMap) {
        var sourceAcb = new AcbFile(Path.Join(SoundsPath, "exertions", sourceAcbFileName), null);
        var trackIndicesOfSourceWaveformsInSonic = new List<ushort>(sourceAcb.InternalWaveforms.Count);
        trackIndicesOfSourceWaveformsInSonic.AddRange(
            sourceAcb.InternalWaveformRows.Select(
                row => targetAcb.AddTrack(
                    row,
                    sourceAcb.InternalWaveforms[row.GetValue<ushort>("Id")])));

        // 1. copy all new voices to sonic voices file
        var translatedTrackIndices = new Dictionary<string, List<ushort>>();
        foreach (var (key, indices) in sourceAcb.RelevantWaveformIds) {
            var translated = translatedTrackIndices[key] = new();
            translated.AddRange(indices.Select(index => trackIndicesOfSourceWaveformsInSonic[index]));
        }

        // 2. silence all sonic voices
        for (var i = 0; i < targetAcb.SequenceTable.Rows.Count; i++)
            targetAcb.SetTrackIndices(i, Array.Empty<ushort>());

        // 3. for the voice lines with matching names, put new lines
        foreach (var (cueName, sequenceIndex) in targetAcb.CueNameToSequenceIndex) {
            var newCueName = cuePrefix + "_" + cueName.Split("_", 2)[1];
            if (!translatedTrackIndices.TryGetValue(newCueName, out var indices))
                continue;
            targetAcb.SetTrackIndices(sequenceIndex, indices.ToArray());
        }

        // 4. apply extra mappings
        if (extraExertionMap is not null)
            foreach (var (from, to) in extraExertionMap) {
                var sequenceIndex = targetAcb.CueNameToSequenceIndex[from];
                targetAcb.SetTrackIndices(
                    sequenceIndex,
                    to == "" ? Array.Empty<ushort>() : translatedTrackIndices[to].ToArray());
            }
    }

    private Task PatchSonicExertions(CancellationToken cancellationToken) => Task.Run(
        () => {
            Debug.Assert(SoundsPath is not null);
            var baseName = Path.Join(SoundsPath, "exertions/hero_sonic");
            var acbFile = baseName + ".acb";
            var awbFile = File.Exists(baseName + ".awb") ? baseName + ".awb" : null;
            if (!File.Exists(acbFile + ".bak"))
                File.Copy(acbFile, baseName + ".acb.bak");
            if (awbFile is not null && !File.Exists(awbFile + ".bak"))
                File.Copy(awbFile, awbFile + ".bak");
            var targetAcb = new AcbFile(acbFile + ".bak", awbFile + ".bak");

            switch (Voice) {
                case CharacterVoices.Auto when Mode == Characters.Shadow:
                case CharacterVoices.Shadow:
                    PatchSonicExertionsWith(targetAcb, "shadow.acb", "Shadow", SonicToShadowExertionMap);
                    break;
                case CharacterVoices.Auto when Mode == Characters.Sticks:
                case CharacterVoices.Sticks: {
                    PatchSonicExertionsWith(targetAcb, "sticks.acb", "Sticks", null);
                    break;
                }
                case CharacterVoices.Auto when Mode == Characters.MetalSonic:
                case CharacterVoices.Silence: {
                    // he's a silent type
                    for (var i = 0; i < targetAcb.SequenceTable.Rows.Count; i++)
                        targetAcb.SetTrackIndices(i, Array.Empty<ushort>());
                    break;
                }
                case CharacterVoices.Sonic:
                default:
                    throw new InvalidOperationException();
            }

            targetAcb.Save(baseName);
        },
        cancellationToken);

    private Task PatchAddCloneTextures(CancellationToken cancellationToken) => Task.Run(
        () => {
            foreach (var entry in ReferenceLevel.Entries.Where(
                         entry => entry.Header.InnerPath.StartsWith(
                             ReferenceTexturePathPrefix,
                             StringComparison.OrdinalIgnoreCase))) {
                Heroes!.PutEntry(-1, entry.Header.InnerPath, entry.Source);
            }
        },
        cancellationToken);

    private async Task PatchSonicModel(CancellationToken cancellationToken) {
        var sonic = await CryCharacter.FromCryEngineFiles(
            ReferenceLevel.AsFunc(SkinFlag.LookupDefault),
            SonicBaseName,
            cancellationToken);
        var reference = await CryCharacter.FromCryEngineFiles(
            ReferenceLevel.AsFunc(SkinFlag.LookupDefault),
            ReferenceObjectBaseName,
            cancellationToken);
        var sonicMaterials = sonic.Model.Material?.SubMaterialsAndRefs ?? throw new InvalidDataException();
        var referenceMaterials = reference.Model.Material?.SubMaterialsAndRefs ?? throw new InvalidDataException();
        sonicMaterials.RemoveAll(
            x => x is Material xm && referenceMaterials.Any(y => y is Material ym && ym.Name == xm.Name));
        sonic.Model.PseudoMaterials[0].Children
            .RemoveAll(x => reference.Model.PseudoMaterials[0].Children.Any(y => y.Name == x.Name));
        sonicMaterials.AddRange(referenceMaterials);
        sonic.Model.PseudoMaterials[0].Children.AddRange(reference.Model.PseudoMaterials[0].Children);
        sonic.Model.PseudoMaterials[0].Name = reference.Model.PseudoMaterials[0].Name;

        sonic.Model.Nodes.Clear();
        sonic.Model.Nodes.AddRange(reference.Model.Nodes.Select(x => x.Clone()));

        var sonicControllers = sonic.Model.RootController?.GetEnumeratorBreadthFirst().ToList() ??
            throw new InvalidDataException("Sonic.RootController is not set");
        var referenceControllers = reference.Model.RootController?.GetEnumeratorBreadthFirst().ToList() ??
            throw new InvalidDataException("Reference.RootController is not set");
        foreach (var controller in referenceControllers) {
            var target = sonicControllers.SingleOrDefault(x => x.Id == controller.Id);
            if (target is null) {
                target = sonicControllers.SingleOrDefault(
                    x => x.Name + "_joint" == controller.Name
                        || x.Name == controller.Name + "_joint"
                        || x.Name == controller.Name[..2] + "mouth_" + controller.Name[2..]
                        || x.Name + "_joint" == controller.Name[..2] + "mouth_" + controller.Name[2..]
                        || x.Name == controller.Name[..2] + "mouth_" + controller.Name[2..] + "_joint"
                );

                if (target is null) {
                    var parent = controller.Parent is null
                        ? null
                        : sonicControllers.Single(x => x.Name == controller.Parent.Name);

                    target = new(controller.Id, controller.Name);
                    parent?.Children.Add(target);
                    sonicControllers.Add(target);
                    // need hierarchy info to calculate new relative bind pose matrix from absolute bind pose matrix
                    target.AbsoluteBindPoseMatrix = controller.AbsoluteBindPoseMatrix;
                } else {
                    foreach (var mesh in sonic.Model.Nodes.SelectMany(node => node.Meshes))
                        for (var i = 0; i < mesh.Vertices.Length; i++) {
                            for (var j = 0; j < 4; j++)
                                if (mesh.Vertices[i].ControllerIds[j] == controller.Id)
                                    mesh.Vertices[i].ControllerIds[j] = target.Id;
                        }
                }
            }

            if (Mode == Characters.Metal && target.Name
                    is not "L_ball_joint"
                    and not "R_ball_joint"
                    and not "L_ankle_joint"
                    and not "R_ankle_joint"
                    and not "_L_toe_joint"
                    and not "_R_toe_joint"
                    and not "C_pelvis_joint"
                    and not "C_spine_1_joint"
                    and not "C_torso_joint") {
                target.AbsoluteBindPoseMatrix = controller.AbsoluteBindPoseMatrix;
            } else if (Mode == Characters.Sticks)
                target.RelativeBindPoseMatrix = controller.RelativeBindPoseMatrix;
        }

        sonic.Attachments.AddRange(reference.Attachments);
        sonic.Definition ??= new();
        sonic.Definition.Attachments ??= new();
        if (reference.Definition?.Attachments is not null)
            sonic.Definition.Attachments.AddRange(reference.Definition.Attachments);
        switch (Mode) {
            case Characters.Shadow:
                foreach (var (a, b) in SonicToShadowAnimationMap)
                    sonic.CryAnimationDatabase!.Animations[a] = reference.CryAnimationDatabase!.Animations[b];
                break;
            case Characters.MetalSonic:
                foreach (var (a, b) in SonicToMetalAnimationMap)
                    sonic.CryAnimationDatabase!.Animations[a] = reference.CryAnimationDatabase!.Animations[b];
                break;
            case Characters.Sticks:
                foreach (var (a, b) in SonicToSticksAnimationMap)
                    sonic.CryAnimationDatabase!.Animations[a] = reference.CryAnimationDatabase!.Animations[b];
                break;
            case Characters.Sonic:
            default:
                throw new InvalidOperationException();
        }

        var geoBytes = sonic.Model.GetGeometryBytes();
        var matBytes = sonic.Model.GetMaterialBytes();
        var dbaBytes = sonic.CryAnimationDatabase!.GetBytes();
        foreach (var level in Levels.Values) {
            var sonicChr = level.GetEntry(sonic.Definition.Model!.File!, false);
            var sonicMtl = level.GetEntry(sonic.Definition.Model!.Material!, false);
            var sonicMtlAlt = level.GetEntry(sonic.Definition.Model!.Material!, true);
            var sonicDba = level.GetEntry(sonic.CharacterParameters!.TracksDatabasePath!, false);

            sonicChr.Source = new(geoBytes);
            sonicMtl.Source = sonicMtlAlt.Source = new(matBytes);
            sonicDba.Source = new(dbaBytes);
        }
    }

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum Characters {
        Sonic = 0,
        Shadow = 1,
        MetalSonic = 2,
        Sticks = 3,

        // Aliases for command line invocation
        Default = Sonic,
        Revert = Sonic,
        Restore = Sonic,
        Metal = MetalSonic,
    }

    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public enum CharacterVoices {
        Auto = 0,
        Sonic = 1,
        Shadow = 2,
        Sticks = 3,
        Silence = 4,

        // Aliases for command line invocation
        Default = Auto,
        Revert = Auto,
        Restore = Auto,
    }
}
