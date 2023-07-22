using System.CommandLine;
using System.CommandLine.Parsing;
using SynergyTools.ProgramCommands.ImportFromGltfSubCommands;

namespace SynergyTools.ProgramCommands;

public class ImportFromGltfProgramCommand : RootProgramCommand {
    public new static readonly Command Command = new(
        "from-gltf",
        "Converts a glb/gltf file into a game model.");

    static ImportFromGltfProgramCommand() {
        Command.AddAlias("import-gltf");
        Command.AddCommand(ExportMetadataCommand.Command);
    }

    public ImportFromGltfProgramCommand(ParseResult parseResult) : base(parseResult) { }
}
