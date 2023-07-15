using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
        var sonic = new CryCharacter(
            x => File.OpenRead(Path.Join(InPath, "Sonic_Crytek", "Levels", "level02_ancientfactorypresent_a", x)),
            Path.Join("objects", "characters", "1_heroes", "sonic", "sonic"));
        var shadow = new CryCharacter(
            x => File.OpenRead(Path.Join(InPath, "Sonic_Crytek", "Levels", "level02_ancientfactorypresent_a", x)),
            Path.Join("objects", "characters", "5_minibosses", "shadow", "shadow"));
        var metal = new CryCharacter(
            x => File.OpenRead(Path.Join(InPath, "Sonic_Crytek", "Levels", "level05_sunkenruins", x)),
            Path.Join("objects", "characters", "5_minibosses", "metal_sonic", "metal_sonic"));
        return 0;
    }
}
