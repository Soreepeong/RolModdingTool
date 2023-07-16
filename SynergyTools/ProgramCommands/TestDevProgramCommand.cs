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
        var heroesPath = Path.Join(InPath, @"Sonic_Crytek\heroes.wiiu.stream"); 
        var strmPath = Path.Join(InPath, @"Sonic_Crytek\Levels\level05_sunkenruins.wiiu.stream");

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

        // var sonicBase = Path.Join("objects", "characters", "1_heroes", "sonic", "sonic");
        // var sonic = new CryCharacter(
        //     x => new MemoryStream((wiiu.TryGetEntry(out var e, x, SkinFlag.Sonic_Default) ? e : wiiu.GetEntry(x)).Source.ReadRaw()),
        //     sonicBase);
        // var shadow = new CryCharacter(
        //     x => File.OpenRead(Path.Join(InPath, "Sonic_Crytek", "Levels", "level02_ancientfactorypresent_a", x)),
        //     Path.Join("objects", "characters", "5_minibosses", "shadow", "shadow"));
        var metal = new CryCharacter(
            CascadingReader,
            Path.Join("objects", "characters", "5_minibosses", "metal_sonic", "metal_sonic"));
        await using var os = File.Create("Z:/ROL3D/metal.glb");
        metal.ToGltf(CascadingReader).Compile(os);
        return 0;
    }
}
