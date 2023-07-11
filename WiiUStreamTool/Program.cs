using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using BCnEncoder.Decoder;
using BCnEncoder.Encoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WiiUStreamTool.FileFormat;
using WiiUStreamTool.FileFormat.CryEngine;

namespace WiiUStreamTool;

public static class Program {
    public enum Mode {
        Sonic,
        Shadow,
        Metal,
    }

    private static async Task<int> Test(Mode mode = Mode.Metal) {
        var packFiles = new Dictionary<string, string>();

        {
            var heroes = @"Z:\ROL\0005000010175B00\content\Sonic_Crytek\heroes";
            await using (var s = File.OpenRead($"{heroes}.wiiu.stream"))
                await WiiUStream.Extract(
                    s,
                    heroes,
                    false,
                    true,
                    null,
                    default);
            foreach (var filename in new[] {
                         "ball_blue.dds", "glide_sonic.dds", "glide_sonic_soft.dds", "bungee_sonic.dds",
                         "bungee_sonic_additive.dds"
                     }) {
                var path = Path.Join(heroes, "art", "textures", "effects", "playerfx", filename);
                if (!Path.Exists(path + ".original"))
                    File.Copy(path, path + ".original");
                if (mode == Mode.Shadow) {
                    var decoder = new BcDecoder();
                    var encoder = new BcEncoder {
                        OutputOptions = {
                            GenerateMipMaps = false,
                            Quality = CompressionQuality.BestQuality,
                            Format = filename is "ball_blue.dds" or "bungee_sonic_additive.dds"
                                ? CompressionFormat.Bc1
                                : CompressionFormat.Bc3,
                            FileFormat = OutputFileFormat.Dds
                        }
                    };
                    Image<Rgba32> image;
                    await using (var s = File.OpenRead(path + ".original"))
                        image = await decoder.DecodeToImageRgba32Async(s);
                    image.ProcessPixelRows(
                        row => {
                            var divisor = filename == "ball_blue.dds" ? 18 : 3;
                            for (var y = 0; y < row.Height; y++) {
                                var span = row.GetRowSpan(y);
                                for (var i = 0; i < span.Length; i++)
                                    span[i].R = span[i].G = span[i].B =
                                        (byte) ((span[i].R + span[i].G + span[i].B) / divisor);
                            }
                        });
                    await using (var s = File.OpenWrite(path))
                        await encoder.EncodeToStreamAsync(image, s);
                } else
                    File.Copy(path + ".original", path, true);
            }

            var outPath = @"C:\tools\cemu\mlc01\usr\title\00050000\10175b00\content\Sonic_Crytek\heroes.wiiu.stream";
            Console.WriteLine($"Saving: {outPath}");
            await using (var stream = new FileStream(outPath, FileMode.Create))
                await WiiUStream.Compress(
                    heroes,
                    stream,
                    false,
                    0,
                    0,
                    null,
                    default);
        }

        foreach (var rootPath in new[] {
                     @"Z:\ROL\0005000E10175B00\content\Sonic_Crytek\Levels",
                     @"Z:\ROL\0005000010175B00\content\Sonic_Crytek\Levels"
                 }) {
            foreach (var f in Directory.GetFiles(rootPath)) {
                Console.WriteLine($"Loading: {f}");
                var nameOnly = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(f));
                var targetDir = Path.Join(rootPath, nameOnly);
                packFiles.TryAdd(nameOnly.ToLowerInvariant(), targetDir);
                continue;
                using var s = File.OpenRead(f);
                await WiiUStream.Extract(
                    s,
                    targetDir,
                    false,
                    true,
                    null,
                    default);
            }
        }

        foreach (var (pf1, pf2) in packFiles) {
            var copiedFiles = new List<string>();
            {
                var targetDir = Path.Join(pf2, "art", "characters", "5_minibosses", "shadow", "texture");
                Directory.CreateDirectory(targetDir);
                foreach (var f in Directory.GetFiles(
                             Path.Join(
                                 packFiles["level02_ancientfactorypresent_a"],
                                 "art",
                                 "characters",
                                 "5_minibosses",
                                 "shadow",
                                 "texture"))) {
                    if (pf1 != "level02_ancientfactorypresent_a")
                        File.Copy(f, Path.Join(targetDir, Path.GetFileName(f)), true);
                    copiedFiles.Add($"art/characters/5_minibosses/shadow/texture/{Path.GetFileName(f)}");
                }
            }

            {
                var targetDir = Path.Join(pf2, "art", "characters", "5_minibosses", "metal_sonic", "textures");
                Directory.CreateDirectory(targetDir);
                foreach (var f in Directory.GetFiles(
                             Path.Join(
                                 packFiles["level05_sunkenruins"],
                                 "art",
                                 "characters",
                                 "5_minibosses",
                                 "metal_sonic",
                                 "textures"))) {
                    if (pf1 != "level05_sunkenruins")
                        File.Copy(f, Path.Join(targetDir, Path.GetFileName(f)), true);
                    copiedFiles.Add($"art/characters/5_minibosses/metal_sonic/textures/{Path.GetFileName(f)}");
                }
            }

            var sonicChr = Path.Join(pf2, "objects", "characters", "1_heroes", "sonic", "sonic.chr");
            var sonicMtl = Path.Join(pf2, "objects", "characters", "1_heroes", "sonic", "sonic.mtl");
            var sonicAnimEvents = Path.Join(pf2, "animations", "characters", "1_heroes", "sonic", "default.animevents");
            if (!File.Exists(sonicChr + ".original"))
                File.Copy(sonicChr, sonicChr + ".original");
            if (!File.Exists(sonicMtl + ".original"))
                File.Copy(sonicMtl, sonicMtl + ".original");
            if (!File.Exists(sonicAnimEvents + ".original"))
                File.Copy(sonicAnimEvents, sonicAnimEvents + ".original");
            var targetBase = mode switch {
                Mode.Sonic => sonicMtl + ".original",
                Mode.Shadow => Path.Join(
                    packFiles["level02_ancientfactorypresent_a"],
                    "objects",
                    "characters",
                    "5_minibosses",
                    "shadow",
                    "shadow"),
                Mode.Metal => Path.Join(
                    packFiles["level05_sunkenruins"],
                    "objects",
                    "characters",
                    "5_minibosses",
                    "metal_sonic",
                    "metal_sonic"),
            };

            {
                var targetDoc = new XmlDocument();
                targetDoc.Load($"{targetBase}.mtl");
                var sourceDoc = new XmlDocument();
                sourceDoc.Load($"{sonicMtl}.original");
                var existingNames = targetDoc["Material"]!["SubMaterials"]!
                    .OfType<XmlElement>()
                    .Select(x => x.GetAttribute("Name"))
                    .ToHashSet();
                foreach (var elem in sourceDoc["Material"]!["SubMaterials"]!.OfType<XmlElement>()) {
                    if (existingNames.Contains(elem.GetAttribute("Name")))
                        continue;

                    targetDoc["Material"]!["SubMaterials"]!.AppendChild(targetDoc.ImportNode(elem, true));
                    existingNames.Add(elem.GetAttribute("Name"));
                }

                targetDoc.Save(sonicMtl);
            }

            if (mode == Mode.Shadow) {
                var x = await File.ReadAllTextAsync(sonicAnimEvents);
                x = x.Replace("Sounds:exertions/hero_sonic:Sonic_", "Sounds:exertions/shadow:Shadow_");
                x = x.Replace("death_via_tunnelbot", "deathfire_noplay");
                x = x.Replace("slingshotpull_noplay", "bungeethrow_noplay");
                await File.WriteAllTextAsync(sonicAnimEvents, x);
            }

            File.Copy(
                $"{targetBase}.chr",
                Path.Join(pf2, "objects", "characters", "1_heroes", "sonic", "sonic.chr"),
                true);

            var metaPath = Path.Join(pf2, WiiUStream.MetadataFilename);
            if (!File.Exists(metaPath + ".original"))
                File.Copy(metaPath, metaPath + ".original");
            var metadata = (await File.ReadAllTextAsync(metaPath + ".original")).ReplaceLineEndings("\n");
            var metadataSuffix = metadata[..metadata.IndexOf('\n')].Split(';', 2)[1];
            metadata = string.Join('\n', copiedFiles.Select(x => $"{x};{metadataSuffix}")) + "\n" + metadata;
            await File.WriteAllTextAsync(metaPath, metadata);
        }

        foreach (var (pf1, pf2) in packFiles) {
            var outPath = Path.Join(
                @"C:\tools\cemu\mlc01\usr\title\0005000e\10175b00\content\Sonic_Crytek\Levels",
                pf1 + ".wiiu.stream"
            );
            Console.WriteLine($"Saving: {outPath}");
            await using (var stream = new FileStream(outPath, FileMode.Create))
                await WiiUStream.Compress(
                    pf2,
                    stream,
                    false,
                    0,
                    0,
                    null,
                    default);
        }

        return 0;
    }

    public static Task<int> Main(string[] args) {
        return Test();

        // return RootProgramCommand.InvokeFromArgsAsync(args);
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
    }
}
