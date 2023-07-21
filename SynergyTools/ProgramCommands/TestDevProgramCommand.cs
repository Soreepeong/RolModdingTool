using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BCnEncoder.Shared.ImageFiles;
using SixLabors.ImageSharp;
using SynergyLib.FileFormat;
using SynergyLib.FileFormat.CryEngine;
using SynergyLib.FileFormat.CryEngine.CryXml.MaterialElements;
using SynergyLib.FileFormat.DirectDrawSurface;
using SynergyLib.FileFormat.DotSquish;
using SynergyLib.FileFormat.GltfInterop;
using SynergyLib.Util;
using DdsFile = SynergyLib.FileFormat.DirectDrawSurface.DdsFile;

namespace SynergyTools.ProgramCommands;

public class TestDevProgramCommand : RootProgramCommand {
    public new static readonly Command Command = new("testdev");

    public static readonly Argument<string[]> PathArgument = new("path") {
        Arity = ArgumentArity.OneOrMore,
    };

    static TestDevProgramCommand() {
        Command.AddArgument(PathArgument);
        Command.SetHandler(ic => new TestDevProgramCommand(ic.ParseResult).Handle());
    }

    public readonly string[] InPathArray;

    private readonly GameFileSystemReader _reader;

    public TestDevProgramCommand(ParseResult parseResult) : base(parseResult) {
        InPathArray = parseResult.GetValueForArgument(PathArgument);

        _reader = new();
        foreach (var p in InPathArray)
            _reader.WithRootDirectory(p);
    }

    private Func<string, CancellationToken, Task<Stream>> ReaderFunc => _reader.AsFunc(SkinFlag.LookupDefault);

    private const string TestLevelName = "hub02_seasidevillage";

    private Task<CryCharacter> ReadSonic() =>
        CryCharacter.FromCryEngineFiles(ReaderFunc, "objects/characters/1_heroes/sonic/sonic", default);

    private Task<CryCharacter> ReadAmy() =>
        CryCharacter.FromCryEngineFiles(ReaderFunc, "objects/characters/1_heroes/amy/amy", default);

    private Task<CryCharacter> ReadKnuckles() =>
        CryCharacter.FromCryEngineFiles(ReaderFunc, "objects/characters/1_heroes/knuckles/knuckles", default);

    private Task<CryCharacter> ReadTails() =>
        CryCharacter.FromCryEngineFiles(ReaderFunc, "objects/characters/1_heroes/tails/tails", default);

    private Task<CryCharacter> ReadShadow() =>
        CryCharacter.FromCryEngineFiles(ReaderFunc, "objects/characters/5_minibosses/shadow/shadow", default);

    private Task<CryCharacter> ReadMetalSonic() =>
        CryCharacter.FromCryEngineFiles(ReaderFunc, "objects/characters/5_minibosses/metal_sonic/metal_sonic", default);

    public async Task<int> Handle() {
        var level = await _reader.GetPackfile(TestLevelName);
        var sonic = await ReadSonic();

        var gltf2 = GltfTuple.FromFile("Z:/ROL3D/stixsonic/untitled.glb");
        var char2 = CryCharacter.FromGltf(gltf2, gltf2.Root.Nodes[0].Name, default);
        foreach (var (k, v) in char2.Model.ExtraTextures) {
            using var asdf = File.Create(Path.Join("Z:/rol3d", "asdf_" + Path.GetFileName(k)));
            v.Position = 0;
            v.CopyTo(asdf);
        }
        var bb = char2.Model.CalculateBoundingBox();
        char2.ApplyScaleTransformation(sonic.Model.CalculateBoundingBox().Radius / bb.Radius);
        foreach (var m in char2.Model.Nodes.SelectMany(x => x.EnumerateHierarchy().SelectMany(y => y.Item1.Meshes))) {
            for (var i = 0; i < m.Vertices.Length; i++)
                m.Vertices[i].Position.Z += 1f;
        }
        char2.Model.RootController = sonic.Model.RootController;

        char2.Model.Material!.SubMaterialsAndRefs!.AddRange(sonic.Model.Material!.SubMaterialsAndRefs!);
        foreach (var (k, v) in char2.Model.ExtraTextures)
            level.PutEntry(0, k, new(v.ToArray()), SkinFlag.Sonic);
        PbxmlFile.SaveObjectToTextFile("Z:/ROL3D/test.xml", char2.Model.Material);
        level.GetEntry(sonic.Definition!.Model!.File!, false).Source = new(char2.Model.GetGeometryBytes());
        level.GetEntry(sonic.Definition!.Model!.Material!, false).Source = new(char2.Model.GetMaterialBytes());
        var targetPath = _reader.GetPackfilePath(TestLevelName);
        while (targetPath.EndsWith(".bak"))
            targetPath = targetPath[..^4];
        await CompressProgramCommand.WriteAndPrintProgress(targetPath, level, default, default);
        return 0;
    }

    // public async Task<int> Handle() {
    //     var level = await _reader.GetPackfile(TestLevelName);
    //     var sonic = await ReadSonic();
    //     
    //     var gltf = GltfTuple.FromFile("Z:/m0361b0001.glb");
    //     // var gltf = await sonic.ToGltf(ReaderFunc, false, true, default);
    //     // foreach (var m in gltf.Root.Materials) {
    //     //     m.Name = null;
    //     //     m.Extensions!.SynergyToolsCryMaterial = null;
    //     // }
    //
    //     var char2 = CryCharacter.FromGltf(gltf, "m0361b0001", default);
    //     foreach (var (k, v) in char2.Model.ExtraTextures) {
    //         using var asdf = File.Create(Path.Join("Z:/rol3d", "asdf_" + Path.GetFileName(k)));
    //         v.Position = 0;
    //         v.CopyTo(asdf);
    //     }
    //     
    //     char2.ApplyScaleTransformation(5 * sonic.Model.CalculateBoundingBox().Radius / char2.Model.CalculateBoundingBox().Radius);
    //     
    //     char2.Model.Material!.SubMaterialsAndRefs!.AddRange(sonic.Model.Material!.SubMaterialsAndRefs!);
    //     char2.Model.PseudoMaterials.First().Name = char2.Model.Nodes.First().MaterialName = sonic.Model.Nodes.First().MaterialName!;
    //     
    //     foreach (var (k, v) in char2.Model.ExtraTextures)
    //         level.PutEntry(0, k, new(v.ToArray()), SkinFlag.Sonic);
    //     PbxmlFile.SaveObjectToTextFile("Z:/ROL3D/test.xml", char2.Model.Material);
    //     level.GetEntry(sonic.Definition!.Model!.File!, false).Source = new(char2.Model.GetGeometryBytes());
    //     level.GetEntry(sonic.Definition!.Model!.Material!, false).Source = new(char2.Model.GetMaterialBytes());
    //
    //     foreach (var (a, b) in SonicToCcAnimationMap) {
    //         var key = sonic.CryAnimationDatabase!.Animations.Keys.Single(x => x.EndsWith("/" + a + ".caf"));
    //         sonic.CryAnimationDatabase!.Animations[key] = char2.CryAnimationDatabase!.Animations[b];
    //     }
    //     
    //     level.GetEntry(sonic.CharacterParameters!.TracksDatabasePath!, false).Source =
    //         new(sonic.CryAnimationDatabase!.GetBytes());
    //     var targetPath = _reader.GetPackfilePath(TestLevelName);
    //     while (targetPath.EndsWith(".bak"))
    //         targetPath = targetPath[..^4];
    //     await CompressProgramCommand.WriteAndPrintProgress(targetPath, level, default, default);
    //     return 0;
    // }
    //
    // private static readonly Dictionary<string, string> SonicToCcAnimationMap = new() {
    //     ["idle"] = "/chara/monster/m0361/animation/a0001/bt_common/resident/monster.pap:cbbm_id0",
    //     ["combat_idle"] = "/chara/monster/m0361/animation/a0001/bt_common/resident/monster.pap:cbbm_id0",
    //     
    //     ["walk"] = "/chara/monster/m0361/animation/a0001/bt_common/resident/monster.pap:cbbm_02f_lp0",
    //     ["walk_notran"] = "/chara/monster/m0361/animation/a0001/bt_common/resident/monster.pap:cbbm_02f_lp0",
    //     // ["walk_left"] = "/chara/monster/m0361/animation/a0001/bt_common/resident/monster.pap:cbbm_trn_l_lp",
    //     // ["walk_right"] = "/chara/monster/m0361/animation/a0001/bt_common/resident/monster.pap:cbbm_trn_r_lp",
    //     ["run_5"] = "/chara/monster/m0361/animation/a0001/bt_common/resident/monster.pap:cbbm_02f_lp0",
    //     ["run_8"] = "/chara/monster/m0361/animation/a0001/bt_common/resident/monster.pap:cbbm_02f_lp0",
    //     ["run_8_notran"] = "/chara/monster/m0361/animation/a0001/bt_common/resident/monster.pap:cbbm_02f_lp0",
    //     ["run_fast"] = "/chara/monster/m0361/animation/a0001/bt_common/resident/monster.pap:cbbm_02f_lp0",
    //     ["run_16"] = "/chara/monster/m0361/animation/a0001/bt_common/resident/monster.pap:cbbm_02f_lp0",
    //     ["run_30"] = "/chara/monster/m0361/animation/a0001/bt_common/resident/monster.pap:cbbm_02f_lp0",
    //     
    //     ["atk_pri_01"] = "/chara/monster/m0361/animation/a0001/bt_common/resident/monster.pap:cbbm_atk1",
    //     ["atk_pri_02"] = "/chara/monster/m0361/animation/a0001/bt_common/mon_sp/m0361/mon_sp003.pap:cbbm_sp01",
    //     ["atk_pri_03"] = "/chara/monster/m0361/animation/a0001/bt_common/mon_sp/m0361/mon_sp004.pap:cbbm_sp02",
    //     ["atk_pri_04"] = "/chara/monster/m0361/animation/a0001/bt_common/mon_sp/m0361/mon_sp008.pap:cbbm_sp06",
    // };
}
