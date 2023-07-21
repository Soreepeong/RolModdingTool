using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using SynergyLib.FileFormat;
using SynergyLib.FileFormat.CryEngine;
using SynergyLib.FileFormat.CryEngine.CryXml.MaterialElements;
using SynergyLib.FileFormat.DirectDrawSurface;
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

    private Task<CryCharacter> ReadShadow() =>
        CryCharacter.FromCryEngineFiles(ReaderFunc, "objects/characters/5_minibosses/shadow/shadow", default);

    private Task<CryCharacter> ReadMetalSonic() =>
        CryCharacter.FromCryEngineFiles(ReaderFunc, "objects/characters/5_minibosses/metal_sonic/metal_sonic", default);

    public async Task<int> Handle() {
        var sonic = await ReadSonic();

        // var gltf = GltfTuple.FromFile("Z:/m0361b0001.glb");
        var gltf = await sonic.ToGltf(ReaderFunc, false, false, default);
        // gltf.CompileSingleBufferToFile("Z:/ROL3D/sonic.glb");

        // foreach (var k in sonic.CryAnimationDatabase.Animations.Keys.ToArray()) {
        //     // var orig = sonic.CryAnimationDatabase.Animations[k];
        //     var recr = char2.CryAnimationDatabase.Animations.Single(x => k.EndsWith($"/{x.Key}.caf")).Value;
        //     // Debugger.Break();
        //     sonic.CryAnimationDatabase.Animations[k] = recr;
        // }

        var char2 = CryCharacter.FromGltf(gltf, default);
        // char2.Scale(2 * sonic.Model.CalculateBoundingBox().Radius / char2.Model.CalculateBoundingBox().Radius);

        foreach (var c in char2.Model.Nodes.SelectMany(x => x.Meshes)) {
            var mat = char2.Model.FindMaterial(c.MaterialName);
            if (mat?.FindTexture(TextureMapType.Normals) is not {File: not null} normalTexture)
                continue;
            if (!char2.Model.ExtraTextures.TryGetValue(normalTexture.File, out var nms))
                nms = new(await _reader.GetBytesAsync(normalTexture.File, SkinFlag.LookupDefault, default));
            var dds = new DdsFile(normalTexture.File, nms);
            var image = dds.ToImageBgra32(0, 0, 0);
            foreach (var v in c.Vertices) {
                var pix = image[(int) ((1 + v.TexCoord.X) * image.Width) % image.Width,
                    (int) ((1 + v.TexCoord.Y) * image.Height) % image.Height];
                var n = mat.GenMask.UseScatterInNormalMap || mat.GenMask.UseHeightInNormalMap
                    ? new(
                        pix.G / 255f,
                        pix.A / 255f,
                        MathF.Sqrt(
                            1
                            - MathF.Pow(pix.G / 255f * 2 - 1, 2)
                            - MathF.Pow(pix.A / 255f * 2 - 1, 2)
                        ) / 2 + 0.5f)
                    : new Vector3(pix.R / 255f, pix.G / 255f, pix.B / 255f);
                Debugger.Break();
            }
        }

        var level = await _reader.GetPackfile(TestLevelName);
        foreach (var (k, v) in char2.Model.ExtraTextures)
            level.PutEntry(0, k, new(v.ToArray()));
        level.GetEntry(sonic.Definition!.Model!.File!, false).Source = new(char2.Model.GetGeometryBytes());
        level.GetEntry(sonic.Definition!.Model!.Material!, false).Source = new(char2.Model.GetMaterialBytes());
        PbxmlFile.SaveObjectToTextFile("Z:/rol3d/test2.xml", sonic.Model.Material!);

        // sonic.CryAnimationDatabase!.Animations["animations/characters/1_heroes/sonic/final/combat_idle.caf"] =
        //     sonic.CryAnimationDatabase.Animations["animations/characters/1_heroes/sonic/final/idle.caf"] =
        //         char2.CryAnimationDatabase!.Animations[
        //             "chara/monster/m0361/animation/a0001/bt_common/resident/monster.pap/cbbm_id0"];
        // level.GetEntry(sonic.CharacterParameters!.TracksDatabasePath!, false).Source =
        //     new(sonic.CryAnimationDatabase!.GetBytes());
        var targetPath = _reader.GetPackfilePath(TestLevelName);
        while (targetPath.EndsWith(".bak"))
            targetPath = targetPath[..^4];
        await CompressProgramCommand.WriteAndPrintProgress(targetPath, level, default, default);
        return 0;
    }
}
