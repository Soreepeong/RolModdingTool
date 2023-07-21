using System;
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
        // var inf = new DdsFile(
        //     "",
        //     File.OpenRead(
        //         @"Z:\ROL\0005000010175B00\content\Sonic_Crytek\heroes\art\characters\1_heroes\sonic\textures\sonic_head_d.dds"));
        // var testb = inf.ToImageBgra32(0, 0, 0).ToDdsFile2D(
        //     "asdf",
        //     new() {Method = SquishMethod.Dxt1},
        //     "CExtCEnd"u8.ToArray(),
        //     0).Data.ToArray();
        // unsafe {
        //     fixed (byte* p = testb) 
        //         ((DdsHeaderLegacy*) p)->Header.SetCryFlags(CryDdsFlags.DontResize);
        // }
        // File.WriteAllBytes("Z:/ROL3D/test.dds", testb);
        // return -1;
        //
        var level = await _reader.GetPackfile(TestLevelName);
        var sonic = await ReadSonic();
        
        // var gltf = GltfTuple.FromFile("Z:/m0361b0001.glb");
        var gltf = await sonic.ToGltf(ReaderFunc, false, true, default);
        foreach (var m in gltf.Root.Materials) {
            m.Name = null;
            m.Extensions!.SynergyToolsCryMaterial = null;
        }

        gltf.CompileSingleBufferToFile("Z:/Rol3d/sonic_r.glb");
        var char2 = CryCharacter.FromGltf(gltf, "m0361b0001", default);
        foreach (var (k, v) in char2.Model.ExtraTextures) {
            using var asdf = File.Create(Path.Join("Z:/rol3d", "asdf_" + Path.GetFileName(k)));
            v.Position = 0;
            v.CopyTo(asdf);
        }
        
        Debugger.Break();
        // char2.ApplyScaleTransformation(5 * sonic.Model.CalculateBoundingBox().Radius / char2.Model.CalculateBoundingBox().Radius);
        
        // foreach (var k in sonic.CryAnimationDatabase.Animations.Keys.ToArray()) {
        //     // var orig = sonic.CryAnimationDatabase.Animations[k];
        //     var recr = char2.CryAnimationDatabase.Animations.Single(x => k.EndsWith($"/{x.Key}.caf")).Value;
        //     // Debugger.Break();
        //     sonic.CryAnimationDatabase.Animations[k] = recr;
        // }

        char2.Model.Material!.SubMaterialsAndRefs!.AddRange(sonic.Model.Material!.SubMaterialsAndRefs!);
        char2.Model.PseudoMaterials.First().Name = char2.Model.Nodes.First().MaterialName = sonic.Model.Nodes.First().MaterialName!;
        
        foreach (var (k, v) in char2.Model.ExtraTextures)
            level.PutEntry(0, k, new(v.ToArray()), SkinFlag.Sonic);
        PbxmlFile.SaveObjectToTextFile("Z:/ROL3D/test.xml", char2.Model.Material);
        level.GetEntry(sonic.Definition!.Model!.File!, false).Source = new(char2.Model.GetGeometryBytes());
        level.GetEntry(sonic.Definition!.Model!.Material!, false).Source = new(char2.Model.GetMaterialBytes());
        
        // sonic.CryAnimationDatabase!.Animations["animations/characters/1_heroes/sonic/final/combat_idle.caf"] =
        //     sonic.CryAnimationDatabase.Animations["animations/characters/1_heroes/sonic/final/idle.caf"] =
        //         char2.CryAnimationDatabase!.Animations[
        //             "/chara/monster/m0361/animation/a0001/bt_common/resident/monster.pap:cbbm_id0"];
        level.GetEntry(sonic.CharacterParameters!.TracksDatabasePath!, false).Source =
            new(sonic.CryAnimationDatabase!.GetBytes());
        var targetPath = _reader.GetPackfilePath(TestLevelName);
        while (targetPath.EndsWith(".bak"))
            targetPath = targetPath[..^4];
        await CompressProgramCommand.WriteAndPrintProgress(targetPath, level, default, default);
        return 0;
    }
}
