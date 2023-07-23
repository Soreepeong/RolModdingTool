using System.Text;
using System.Threading.Tasks;
using SynergyTools.ProgramCommands;

namespace SynergyTools;

public static class Program {
    public static Task<int> Main(string[] args) {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        return RootProgramCommand.InvokeFromArgsAsync(args);
    }
}
