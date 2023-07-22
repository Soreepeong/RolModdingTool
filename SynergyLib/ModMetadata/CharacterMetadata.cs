using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Numerics;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SynergyLib.FileFormat.CryEngine;
using SynergyLib.FileFormat.CryEngine.CryXml;
using SynergyLib.FileFormat.CryEngine.CryXml.MaterialElements;
using SynergyLib.FileFormat.GltfInterop;
using SynergyLib.Util;
using SynergyLib.Util.CustomJsonConverters;

namespace SynergyLib.ModMetadata;

public class CharacterMetadata {
    public CharacterMetadata() : this(string.Empty, string.Empty) { }

    public CharacterMetadata(string name, string targetPath) {
        Name = name;
        TargetPath = targetPath;
    }

    [JsonProperty]
    public string Name { get; set; }

    [JsonProperty]
    public string TargetPath { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Include)]
    public float? HeightScaleRelativeToTarget { get; set; }

    [JsonProperty]
    public List<Material> Materials { get; set; } = new();

    [UsedImplicitly]
    public bool ShouldSerializeMaterials() => true;

    [JsonProperty]
    public List<AnimationMetadata> Animations { get; set; } = new();

    [UsedImplicitly]
    public bool ShouldSerializeAnimations() => Animations.Any();

    public void ToJson(JsonTextWriter writer, Vector3JsonConverter.Notation colorNotation) {
        var serializer = new JsonSerializer {
            Formatting = Formatting.Indented,
            ContractResolver = new JsonContractResolver {
                ColorConverter = new() {ValueNotation = colorNotation},
            },
        };
        serializer.Serialize(writer, this);
    }

    public void ToJson(string path, Vector3JsonConverter.Notation colorNotation) {
        using var fp = new JsonTextWriter(new StreamWriter(File.Create(path)));
        fp.Indentation = 1;
        fp.IndentChar = '\t';
        ToJson(fp, colorNotation);
    }

    public static CharacterMetadata FromJson(JsonTextReader reader) {
        var serializer = new JsonSerializer {
            Formatting = Formatting.Indented,
            ContractResolver = new JsonContractResolver(),
        };
        return serializer.Deserialize<CharacterMetadata>(reader) ??
            throw new InvalidDataException("File contained null value.");
    }

    public static CharacterMetadata FromJsonFile(string path) {
        using var fp = new JsonTextReader(File.OpenText(path));
        return FromJson(fp);
    }

    [return: NotNullIfNotNull(nameof(character))]
    public static CharacterMetadata? FromCharacter(CryCharacter? character, string path, GltfTuple? gltf) {
        if (character is null)
            return null;

        var metadata = new CharacterMetadata(character.Model.Nodes.Single().Name, path) {
            HeightScaleRelativeToTarget = 1,
        };
        if (character.Model.Material is { } srcmat) {
            if (srcmat.SubMaterials?.Any() is true)
                metadata.Materials.AddRange(
                    srcmat.SubMaterials.Where(x => x is not null).Select(x => x!.Clone()).Cast<Material>());
            else
                metadata.Materials.Add((Material) srcmat.Clone());

            for (var i = 0; i < metadata.Materials.Count; i++) {
                var mat = metadata.Materials[i];
                if (mat.SubMaterials?.Any() is true) {
                    metadata.Materials.RemoveAt(i);
                    i--;
                    metadata.Materials.InsertRange(i, mat.SubMaterials!);
                }
            }

            if (gltf is not null) {
                foreach (var mat in metadata.Materials) {
                    if (mat.Textures is not { } textures)
                        continue;
                    foreach (var texture in textures) {
                        var gltfTextureInfo = texture.GltfTextureInfo;
                        texture.GltfTextureInfo = null;
                        if (gltfTextureInfo?.Index is null)
                            continue;

                        var gltfTexture = gltf.Root.Textures[gltfTextureInfo.Index.Value];
                        if (gltfTexture.Name is not null) {
                            texture.File = gltfTexture.Name;
                            continue;
                        }

                        if (gltfTexture.Source is null)
                            continue;

                        var gltfImage = gltf.Root.Images[gltfTexture.Source.Value];
                        if (gltfImage.Name is not null)
                            texture.File = gltfImage.Name;
                    }
                }
            }
        }

        if (character.CryAnimationDatabase?.Animations is { } animations) {
            var names = animations.Keys.Select(x => Path.ChangeExtension(x, null)).StripCommonParentPaths();
            foreach (var (name, data) in names.Zip(animations.Values).OrderBy(x => x.First.ToLowerInvariant()))
                metadata.Animations.Add(new(name, data.MotionParams));
        }

        return metadata;
    }

    public class JsonContractResolver : DefaultContractResolver {
        public Vector3JsonConverter ColorConverter = new();

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization) {
            var props = base.CreateProperties(type, memberSerialization);
            foreach (var ignoreProp in props.Where(
                         type switch {
                             _ when type == typeof(Material) => x => MaterialFilter(x.PropertyName),
                             _ when type == typeof(PublicParams) => x => PublicParamsFilter(x.PropertyName),
                             _ when type == typeof(Texture) => x => TextureFilter(x.PropertyName),
                             _ => _ => false,
                         })) {
                ignoreProp.Ignored = true;
            }

            foreach (var colorProp in props.Where(
                         x => x.PropertyName?.EndsWith("Color") is true
                             && (x.PropertyType == typeof(Vector3) || x.PropertyType == typeof(Vector3?)))) {
                colorProp.Converter = ColorConverter;
            }

            return props;
        }

        private static bool MaterialFilter(string? propertyName) => propertyName
            // Autogenerated
            is nameof(Material.Flags)

            // Always empty
            or nameof(Material.MatTemplate)

            // .AlphaCutoff
            or nameof(Material.AlphaTest)

            // .PbrMetallicRoughness.BaseColorFactor
            or nameof(Material.DiffuseColor)

            // .Extensions.KhrMaterialsEmissiveStrength.EmissiveStrength
            or nameof(Material.GlowAmount)

            // .PbrMetallicRoughness.BaseColorFactor
            or nameof(Material.Opacity)

            // .Extensions.KhrMaterialsSpecular.SpecularColorFactor
            or nameof(Material.SpecularColor)

            // 1 - .PbrMetallicRoughness.RoughnessFactor
            // Extensions.KhrMaterialsPbrSpecularGlossiness.GlossinessFactor
            or nameof(Material.Shininess);

        private static bool PublicParamsFilter(string? propertyName) => propertyName
            // .PbrMetallicRoughness.MetallicFactor
            is nameof(PublicParams.Metalness);

        private static bool TextureFilter(string? propertyName) => propertyName
            is nameof(Texture.GltfTextureInfo);
    }
}
