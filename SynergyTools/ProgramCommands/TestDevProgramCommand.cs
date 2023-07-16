using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SynergyLib.FileFormat;
using SynergyLib.FileFormat.CryEngine;
using SynergyLib.FileFormat.GltfInterop;
using SynergyLib.Util.MathExtras;

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
        var heroesPath = Path.Join(InPath, @"Sonic_Crytek\heroes.wiiu.stream");
        var strmPath = Path.Join(
            InPath,
            "..",
            "..",
            "..",
            "0005000e",
            "10175b00",
            "content",
            @"Sonic_Crytek\Levels\hub01_excavationsite.wiiu.stream");
        var heroes = new WiiuStreamFile();
        heroes.ReadFrom(null, $"{heroesPath}.bak", cancellationToken);
        var level = new WiiuStreamFile();
        level.ReadFrom(null, $"{strmPath}.bak", cancellationToken);

        Stream CascadingReader(string path) {
            if (level.TryGetEntry(out var entry, path, SkinFlag.Sonic_Default))
                return new MemoryStream(entry.Source.ReadRaw());
            if (level.TryGetEntry(out entry, path))
                return new MemoryStream(entry.Source.ReadRaw());
            if (heroes.TryGetEntry(out entry, path))
                return new MemoryStream(entry.Source.ReadRaw());
            throw new FileNotFoundException();
        }

        var sonicBase = Path.Join("objects", "characters", "1_heroes", "sonic", "sonic");
        var sonic = CryCharacter.FromCryEngineFiles(CascadingReader, sonicBase);
        // var shadow = new CryCharacter(
        //     x => File.OpenRead(Path.Join(InPath, "Sonic_Crytek", "Levels", "level02_ancientfactorypresent_a", x)),
        //     Path.Join("objects", "characters", "5_minibosses", "shadow", "shadow"));
        // var metal = CryCharacter.FromCryEngineFiles(
        //     CascadingReader,
        //     Path.Join("objects", "characters", "5_minibosses", "metal_sonic", "metal_sonic"));

        var aabbSonic = AaBb.NegativeExtreme;
        foreach (var m in sonic.Model.Meshes)
        foreach (var p in m.Vertices)
            aabbSonic.Expand(p.Position);

        await using (var os = File.Create("Z:/ROL3D/sonic.glb"))
            sonic.ToGltf(CascadingReader).Compile(os);
        // var char2 = CryCharacter.FromGltf(GltfTuple.FromStream(File.OpenRead("Z:/ROL3D/sonic.glb")));
        // foreach (var k in sonic.CryAnimationDatabase.Animations.Keys.ToArray()) {
        //     // var orig = sonic.CryAnimationDatabase.Animations[k];
        //     var recr = char2.CryAnimationDatabase.Animations.Single(x => k.EndsWith($"/{x.Key}.caf")).Value;
        //     // Debugger.Break();
        //     sonic.CryAnimationDatabase.Animations[k] = recr;
        // }

        var char2 = CryCharacter.FromGltf(GltfTuple.FromStream(File.OpenRead("Z:/ROL3D/m0361_b0001_v0001.glb")));
        var aabbM2 = AaBb.NegativeExtreme;
        foreach (var m in char2.Model.Meshes)
        foreach (var p in m.Vertices)
            aabbM2.Expand(p.Position);
        
        char2.Scale(2 * aabbSonic.Radius / aabbM2.Radius);
        
        foreach (var (k, v) in char2.Model.ExtraTextures)
            level.PutEntry(0, k, new(v.ToArray()));
        level.GetEntry(sonic.Definition!.Model!.File!).Source = new(char2.Model.GetGeometryBytes());
        level.GetEntry(sonic.Definition!.Model!.Material!, SkinFlag.Sonic_Default).Source =
            new(char2.Model.GetMaterialBytes());
        
        await using (var os = File.Create("Z:/ROL3D/cc.glb"))
            char2.ToGltf(CascadingReader).Compile(os);
        
        sonic.CryAnimationDatabase.Animations["animations/characters/1_heroes/sonic/final/combat_idle.caf"] =
            sonic.CryAnimationDatabase.Animations["animations/characters/1_heroes/sonic/final/idle.caf"] =
                char2.CryAnimationDatabase!.Animations[
                    "chara/monster/m0361/animation/a0001/bt_common/resident/monster.pap/cbbm_id0"];
        level.GetEntry(sonic.CharacterParameters!.TracksDatabasePath!).Source =
            new(sonic.CryAnimationDatabase!.GetBytes());
        await CompressProgramCommand.WriteAndPrintProgress(strmPath, level, default, cancellationToken);
        return 0;
    }
}
