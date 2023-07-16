using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SynergyLib.FileFormat;
using SynergyLib.FileFormat.CryEngine;

namespace SynergyTools.ProgramCommands;

public class TestDevProgramCommand : RootProgramCommand {
    public new static readonly Command Command = new("testdev");

    public static readonly Argument<string> PathArgument = new("path");

    static TestDevProgramCommand() {
        Command.AddArgument(PathArgument);
        Command.SetHandler(ic => new TestDevProgramCommand(ic.ParseResult).Handle(ic.GetCancellationToken()));
    }

    public readonly string InPath;

    public TestDevProgramCommand(ParseResult parseResult) : base(parseResult) {
        InPath = parseResult.GetValueForArgument(PathArgument);
    }

    public async Task<int> Handle(CancellationToken cancellationToken) {
        // const string strmPath = @"C:\Tools\cemu\mlc01\usr\title\00050000\10175b00\content\Sonic_Crytek\Levels\level05_sunkenruins.wiiu.stream";
        const string strmPath = @"C:\Tools\cemu\mlc01\usr\title\0005000e\10175b00\content\Sonic_Crytek\Levels\hub01_excavationsite.wiiu.stream";
        var wiiu = new WiiuStreamFile();
        wiiu.ReadFrom(null, $"{strmPath}.bak", cancellationToken);

        var sonicBase = Path.Join("objects", "characters", "1_heroes", "sonic", "sonic");
        var sonic = new CryCharacter(
            x => new MemoryStream((wiiu.TryGetEntry(out var e, x, SkinFlag.Sonic_Default) ? e : wiiu.GetEntry(x)).Source.ReadRaw()),
            sonicBase);
        var shadow = new CryCharacter(
            x => File.OpenRead(Path.Join(InPath, "Sonic_Crytek", "Levels", "level02_ancientfactorypresent_a", x)),
            Path.Join("objects", "characters", "5_minibosses", "shadow", "shadow"));
        // var metal = new CryCharacter(
        //     x => File.OpenRead(Path.Join(InPath, "Sonic_Crytek", "Levels", "level05_sunkenruins", x)),
        //     Path.Join("objects", "characters", "5_minibosses", "metal_sonic", "metal_sonic"));
        sonic.Model.Meshes.Clear();
        sonic.Model.Meshes.AddRange(shadow.Model.Meshes.Select(x => x.Clone()));
        var sonicMaterials = sonic.Model.Material.SubMaterials!;
        sonicMaterials.RemoveAll(x => x.Name == "SonicHead_M");
        sonicMaterials.RemoveAll(x => x.Name == "SonicBody_M1");
        sonicMaterials.Add(shadow.Model.Material.SubMaterials!.Single(x => x.Name == "Shadow_Head_M"));
        sonicMaterials.Add(shadow.Model.Material.SubMaterials!.Single(x => x.Name == "Shadow_Body_M"));
        sonic.Attachments.AddRange(shadow.Attachments);
        sonic.Definition!.Attachments!.AddRange(shadow.Definition!.Attachments!);

        var animMaps = new Dictionary<string, string> {
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
            
            ["animations/characters/1_heroes/sonic/final/warthog_idle.caf"] = "animations/characters/5_minibosses/shadow/final/nav/nav_fly_idle.caf",
            ["animations/characters/1_heroes/sonic/final/warthog_run.caf"] = "animations/characters/5_minibosses/shadow/final/nav/nav_fly_strafe_f.caf",
            ["animations/characters/1_heroes/sonic/final/warthog_run_lean_left.caf"] = "animations/characters/5_minibosses/shadow/final/nav/nav_fly_strafe_l.caf",
            ["animations/characters/1_heroes/sonic/final/warthog_run_lean_right.caf"] = "animations/characters/5_minibosses/shadow/final/nav/nav_fly_strafe_r.caf",
        };

        foreach (var (a, b) in animMaps) {
            sonic.CryAnimationDatabase!.Animations[a] = shadow.CryAnimationDatabase!.Animations[b];
        }
        
        wiiu.GetEntry(sonicBase + ".chr").Source = new(sonic.Model.GetGeometryBytes());
        wiiu.GetEntry(sonic.CharacterParameters!.TracksDatabasePath!).Source = new(sonic.CryAnimationDatabase!.GetBytes());
        var test = new CryAnimationDatabase(new MemoryStream(sonic.CryAnimationDatabase!.GetBytes()));
        
        Debugger.Break();

        wiiu.GetEntry(sonicBase + ".mtl", SkinFlag.Sonic_Default).Source = new(sonic.Model.GetMaterialBytes());
        await CompressProgramCommand.WriteAndPrintProgress(
            strmPath,
            wiiu,
            default,
            cancellationToken);
        return 0;
    }
}
