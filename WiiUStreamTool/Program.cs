using System.Threading.Tasks;
using WiiUStreamTool.ProgramCommands;

namespace WiiUStreamTool;

public static class Program {
    public static Task<int> Main(string[] args) {
        return RootProgramCommand.InvokeFromArgsAsync(args);
        /*
        const string testroot = @"Z:\ROL\0005000010175B00\content\Sonic_Crytek\";
        // const string testroot = @"C:\Tools\0005000010175B00\content\Sonic_Crytek\";
        const string level02 = @$"{testroot}Levels\level02_ancientfactorypresent_a\";
        const string level05 = @$"{testroot}Levels\level05_sunkenruins\";

        // var sonic = new CryCharacter(level02, @"objects\characters\1_heroes\sonic\sonic.cdf");
        // var shadow = new CryCharacter(level02, @"objects\characters\5_minibosses\shadow\shadow.cdf");

        var sonicBallModel = CryChunks.FromFile(@$"{level02}objects\characters\1_heroes\sonic_ball\sonic_ball.chr");
        // var sonicModel = CryChunks.FromFile(@$"{level02}objects\characters\1_heroes\sonic\sonic.chr");
        // var sonicAnim = CryChunks.FromFile(@$"{level02}animations\characters\1_heroes\sonic\sonic.dba");
        // var shadowModel = CryFile.FromFile(@$"{level02}objects\characters\5_minibosses\shadow\shadow.chr");
        // var shadowAnim = CryChunks.FromFile(@$"{level02}animations\characters\5_minibosses\shadow\shadow.dba");
        // var metalModel = CryFile.FromFile(@$"{level05}objects\characters\5_minibosses\metal_sonic\metal_sonic.chr");
        // var metalAnim = CryFile.FromFile(@$"{level05}animations\characters\5_minibosses\metal_sonic\metal_sonic.dba");

        return Task.FromResult(-1);
        //*/
    }
}
