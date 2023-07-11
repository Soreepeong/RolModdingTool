using System.IO;
using WiiUStreamTool.FileFormat.CryEngine;
using WiiUStreamTool.ProgramCommands;

namespace WiiUStreamTool;

public static class Program {
    public static int Main(string[] args) {
        // const string testroot = @"Z:\ROL\0005000010175B00\content\Sonic_Crytek\";
        const string testroot = @"C:\Tools\0005000010175B00\content\Sonic_Crytek\";
        
        var inBytes = File.ReadAllBytes(
            // @$"{testroot}Levels\level05_sunkenruins\animations\characters\5_minibosses\metal_sonic\metal_sonic.dba"
            // @$"{testroot}Levels\level05_sunkenruins\objects\characters\5_minibosses\metal_sonic\metal_sonic.chr"
            @$"{testroot}Levels\level02_ancientfactorypresent_a\animations\characters\5_minibosses\shadow\shadow.dba"
            // @$"{testroot}Levels\level02_ancientfactorypresent_a\objects\characters\5_minibosses\shadow\shadow.chr"

            // @$"{testroot}Levels\level02_ancientfactorypresent_a\animations\characters\1_heroes\sonic\sonic.dba"
            // @$"{testroot}Levels\level02_ancientfactorypresent_a\objects\characters\1_heroes\sonic\sonic.chr"
        );

        var testfile = CryFile.FromBytesAndVerify(inBytes);
        
        return 0;

        var argtask = RootProgramCommand.InvokeFromArgsAsync(args);
        argtask.Wait();
        return argtask.Result;
    }
}
