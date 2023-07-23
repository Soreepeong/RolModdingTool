using System.CommandLine;
using System.CommandLine.Parsing;
using SynergyTools.ProgramCommands.ModSubCommands;

namespace SynergyTools.ProgramCommands;

public class ModProgramCommand : RootProgramCommand {
    public new static readonly Command Command = new(
        "mod",
        "Create or use mods for the game.");

    static ModProgramCommand() {
        Command.AddAlias("from-gltf");
        Command.AddAlias("import-gltf");
        Command.AddCommand(ExportMetadataCommand.Command);
        Command.AddCommand(ExportPackCommand.Command);
        Command.AddCommand(ImportPackCommand.Command);
    }

    public ModProgramCommand(ParseResult parseResult) : base(parseResult) { }
}
