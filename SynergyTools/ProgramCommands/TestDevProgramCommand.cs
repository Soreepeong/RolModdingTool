using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SynergyLib.FileFormat;
using SynergyLib.FileFormat.CryEngine;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Chunks;

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
        var wiiu = new WiiuStreamFile();
        wiiu.ReadFrom(null, @"C:\Tools\cemu\mlc01\usr\title\00050000\10175b00\content\Sonic_Crytek\Levels\level05_sunkenruins.wiiu.stream.bak", cancellationToken);

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

        wiiu.GetEntry(sonicBase + ".chr").Source = new(sonic.Model.GetGeometryBytes());
        wiiu.GetEntry(sonicBase + ".mtl", SkinFlag.Sonic_Default).Source = new(sonic.Model.GetMaterialBytes());
        await CompressProgramCommand.WriteAndPrintProgress(
            @"C:\Tools\cemu\mlc01\usr\title\00050000\10175b00\content\Sonic_Crytek\Levels\level05_sunkenruins.wiiu.stream",
            wiiu,
            default,
            cancellationToken);
        return 0;
    }
}
