using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WiiUStreamTool.FileFormat.CryEngine;
using WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Chunks;
using WiiUStreamTool.FileFormat.CryEngine.CryDefinitions.Enums;
using WiiUStreamTool.ProgramCommands;
using WiiUStreamTool.Util.BinaryRW;

namespace WiiUStreamTool;

public static class Program {
    public static int Main(string[] args) {
        // const string testroot = @"Z:\ROL\0005000010175B00\content\Sonic_Crytek\";
        const string testroot = @"C:\Tools\0005000010175B00\content\Sonic_Crytek\";
        
        var testfile = new CryFile();
        var inBytes = File.ReadAllBytes(
            @$"{testroot}Levels\level05_sunkenruins\animations\characters\5_minibosses\metal_sonic\metal_sonic.dba"
            // @$"{testroot}Levels\level05_sunkenruins\objects\characters\5_minibosses\metal_sonic\metal_sonic.chr"
            // @$"{testroot}Levels\level02_ancientfactorypresent_a\animations\characters\5_minibosses\shadow\shadow.dba"
            // @$"{testroot}Levels\level02_ancientfactorypresent_a\objects\characters\5_minibosses\shadow\shadow.chr"

            // @$"{testroot}Levels\level02_ancientfactorypresent_a\animations\characters\1_heroes\sonic\sonic.dba"
            // @$"{testroot}Levels\level02_ancientfactorypresent_a\objects\characters\1_heroes\sonic\sonic.chr"
        );
        using (var f = new NativeReader(new MemoryStream(inBytes)))
            testfile.ReadFrom(f);

        byte[] outBytes;
        using (var ms = new MemoryStream())
        using (var f = new NativeWriter(ms)) {
            testfile.WriteTo(f);
            outBytes = ms.ToArray();
        }

        var ignoreZones = new List<Tuple<int, int>>();
        // seems that if boneId array is not full, garbage values remain in place of unused memory
        // ignore that from comparison
        foreach (var ignoreItem in testfile.Values.OfType<MeshSubsetsChunk>())
            ignoreZones.Add(Tuple.Create(ignoreItem.Header.Offset, ignoreItem.Header.Offset + ignoreItem.WrittenSize));

        ignoreZones.Add(Tuple.Create(31, 32));
        ignoreZones.Add(Tuple.Create(51, 52));

        for (var i = 0; i < inBytes.Length; i++) {
            if (inBytes[i] != outBytes[i] && !ignoreZones.Any(x => x.Item1 <= i && i < x.Item2))
                throw new InvalidDataException();
        }

        if (inBytes.Length != outBytes.Length)
            throw new InvalidDataException();

        Debugger.Break();

        return 0;

        var argtask = RootProgramCommand.InvokeFromArgsAsync(args);
        argtask.Wait();
        return argtask.Result;
    }
}
