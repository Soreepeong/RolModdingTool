using System.CommandLine;
using System.CommandLine.Parsing;
using System.Threading;
using System.Threading.Tasks;

namespace SynergyTools.ProgramCommands;

public class TestDevProgramCommand : RootProgramCommand {
    public new static readonly Command Command = new("testdev");

    static TestDevProgramCommand() {
        Command.SetHandler(ic => new TestDevProgramCommand(ic.ParseResult).Handle(ic.GetCancellationToken()));
    }

    public TestDevProgramCommand(ParseResult parseResult) : base(parseResult) { }

    public async Task<int> Handle(CancellationToken cancellationToken) {
        return 0;
    }
}
