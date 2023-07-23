# SynergyTools
Tool for dealing with and modding Sonic Boom: Rise of Lyric.

## Features
* Compress and extract .wiiu.stream archives.
* Repack files to play as Shadow or Metal Sonic.
* Export models as glTF file.
* Play the game with your glTF model.
* Convert your model into a shareable `.wiiu.stream` mod file.
* Import `.wiiu.stream` mod files made by other people.

| Metal Sonic                                                                                                                                                         | Cruise Chaser                                                                                                                                               | 
|---------------------------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------| 
| ![Metal Sonic in place of Regular Sonic](https://github-production-user-asset-6210df.s3.amazonaws.com/3614868/253370854-7f236860-8449-4663-b658-391baa8fc262.png)   | ![Cruise Chaser in place of Sonic](https://github-production-user-asset-6210df.s3.amazonaws.com/3614868/255335915-243d7696-5209-4015-b3ef-11748d69d46d.png) |

## License
GPLv3

## How to
Some of the following operations that modify the game files will make a copy of target files with a `.bak` extension.

### Placeholders for the following examples
* `<GameBasePath>` stands for the path to the base game, such as: `C:\mlc01\usr\title\00050000\10175b00\content`
* `<GameUpdatePath>` stands for the path to the update, such as: `C:\mlc01\usr\title\0005000e\10175b00\content`
* `<TargetModelPath>` stands for the inner path to model you want to replace, such as: `objects\characters\1_heroes\sonic\sonic.cdf`
* `<WorkspaceDir>` stands for a random temporary directory, such as: `C:\Users\User\Desktop`
* `<SourceModelPath>` stands for the path to your glTF model file, such as: `C:\Users\User\Desktop\sanic\sanic.glb`
* `<ModpackPath>` stands for the path to the packaged mod file, such as: `C:\Users\User\Desktop\sanic.wiiu.stream`

### Play as Shadow
```
.\SynergyTools.exe quickmod -m shadow <GameUpdatePath> <GameBasePath>
```

### Play as Metal Sonic
```
.\SynergyTools.exe quickmod -m metal <GameUpdatePath> <GameBasePath>
```

### Unpack `.wiiu.stream` archives
```
.\SynergyTools.exe extract <GameBasePath>\Sonic_Crytek\Levels\hub02_seasidevillage.wiiu.stream
.\SynergyTools.exe e <GameUpdatePath>\Sonic_Crytek\Levels\hub01_excavationsite.wiiu.stream -o <WorkspaceDir>
```
Replace `Levels\hub02_seasidevillage.wiiu.stream` with the file of your choice. You can specify multiple paths in the command line.

### Repack a folder into `.wiiu.stream` archive
```
.\SynergyTools.exe compress <GameBasePath>\Sonic_Crytek\Levels\hub02_seasidevillage
.\SynergyTools.exe c <WorkspaceDir>\hub01_excavationsite -o <GameUpdatePath>\Sonic_Crytek\Levels\hub01_excavationsite.wiiu.stream -l 0
```
Specify `-l 0` to disable compression.

### Export a model
```
.\SynergyTools.exe to-gltf <GameUpdatePath> <GameBasePath> -f <TargetModelPath> -o <WorkspaceDir> -m
.\SynergyTools.exe to-gltf <GameUpdatePath> <GameBasePath> -f <TargetModelPath> -o <WorkspaceDir> -s -r -a
.\SynergyTools.exe to-gltf <GameUpdatePath> <GameBasePath> -f **/*.chr -f **/*.cgf -f **/*.cdf -o <WorkspaceDir> -s -r -p -d
```
* Use `-f` or `--file` to specify a file inside game. Wildcards, including `?`, `*`, and `**` are supported.
* Use `-m` or `--metadata` to export a metadata file to refer to, when you're editing the metadata template below.
* Use `-s` or `--single-file` to export as a `.glb` file instead of a `.gltf` file and multiple associated files.
* Use `-r` or `--export-required-textures-only` to export only the essential files for glTF format.
* Use `-a` or `--alt` to export alternative costumes (luminous suit) for Team Sonic, obviously except for Sticks.
* Use `-d` or `--disable-animation` to disable exporting animations. This will often noticeably reduce the output file size.
* Use `-p` or `--preserve-directory-structure` if you're exporting multiple files recursively.

### Play as your model: export metadata template for your model
```
.\SynergyTools.exe mod metadata -g <GameUpdatePath> -g <GameBasePath> -r <TargetModelPath> <SourceModelPath>
```
* Use `-r` or `--reference-model` to specify a model you're intending to replace.

This will create a metadata template file, with the same file name but with `.json` extension.
For example, if you specify `Z:/m0361b0001.glb`, then the command will produce `Z:/m0361b0001.json`.

Scroll down to check the requirements for the model.

### Play as your model: export your model as a shareable file
```
.\SynergyTools.exe mod export -g <GameUpdatePath> -g <GameBasePath> <SourceModelPath> -o test.wiiu.stream
.\SynergyTools.exe mod export -g <GameUpdatePath> -g <GameBasePath> <SourceModelPath> -n hub01_excavationsite -n hub02_seasidevillage
```
* Use `-m` or `--metadata` to specify the location to your metadata file, if it's stored in non-default location.
* Use `-n` or `--level-name` to specify the levels to test your model with. Omit this option to not modify game level files.
* Use `-o` or `--out-path` to specify the output path. It will default to `<SourceModelPath>` but with `.wiiu.stream` extension.

You can zone to other level (map) and zone back to check your modifications, after this command has successfully ended.

### Play as your model: import exported files
```
.\SynergyTools.exe mod import -g <GameUpdatePath> -g <GameBasePath> <ModpackPath>`
.\SynergyTools.exe mod import -g <GameUpdatePath> -g <GameBasePath> <ModpackPath1> <ModpackPath2> <ModpackPath3>`
```
* Use `-n` or `--level-name` to specify the levels to test your model with. Omit this option to apply to all levels.

## Model import notes
Currently, the supported glTF files are limited in its file structure.
* If it contains **more than one node** in the **current scene**, it may or may not work.
  It's more likely that it will **NOT** work.
  * Merge the different nodes (often called *objects* in 3D editor softwares) into one.
    Make one node(object) contain multiple submeshes.
* **Normals** and **UVs** must be exported.
* **Tangents** should be exported, or it may not render well enough.
* **Colors** are optional.
* Scatter map can be provided by setting `TEMP_SKIN` in `GenMask` in material metadata, and creating a `bsg`
  (bump-scatter-gloss) map. Refer to the following channel mapping.
  * `bsg.R` = `scatter.R`
  * `bsg.G` = `normal.R`
  * `bsg.B` = `gloss.R`
  * `bsg.A` = `normal.G`
* If the model is skinned, then the same names should be used with the game model.
  Export the in-game models to check the bone names.
* Number of vertices per submesh should not exceed 65535.
* Animations involving scaling(zoom) are **NOT** supported. Only translation and rotation are supported.
* Animations should be exported in 30 frames per seconds (fps.)
* The textures that aren't in DDS format, or are DDS files not in raw or DXT1/3/5 formats, will be converted to DXT5.

The model import has only been tested with Sonic exported with this application, and Cruise Chaser from Final Fantasy XIV: Heavensward.
Your model may or may not work. If it doesn't, create an issue with the file, or with an example model that exhibits the same problem.

## Credits
* Idea for this tool from [ik-01/WiiUStreamTool](https://github.com/ik-01/WiiUStreamTool)
* .wiiu.stream format from [Paraxade's post](https://forums.sonicretro.org/index.php?posts/811201/)
* .acb/.awb structures from [vgmstream](https://github.com/vgmstream/vgmstream)
* Criware de/serialization from [blueskythlikesclouds/SonicAudioLib](https://github.com/blueskythlikesclouds/SonicAudioTools)
* More Criware stuff from [LazyBone152/XV2-Tools](https://github.com/LazyBone152/XV2-Tools)
* CryEngine stuff from [Markemp/Cryengine-Converter](https://github.com/Markemp/Cryengine-Converter)
* Squish for DXT1/3/5 De/compression

