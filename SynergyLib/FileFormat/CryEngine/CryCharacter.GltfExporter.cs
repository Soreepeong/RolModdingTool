using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;
using SynergyLib.FileFormat.CryEngine.CryModelElements;
using SynergyLib.FileFormat.CryEngine.CryXml;
using SynergyLib.FileFormat.CryEngine.CryXml.MaterialElements;
using SynergyLib.FileFormat.DirectDrawSurface;
using SynergyLib.FileFormat.DirectDrawSurface.PixelFormats.Channels;
using SynergyLib.FileFormat.GltfInterop;
using SynergyLib.FileFormat.GltfInterop.Models;
using SynergyLib.Util;
using SynergyLib.Util.MathExtras;

namespace SynergyLib.FileFormat.CryEngine;

public partial class CryCharacter {
    public class GltfExporter {
        private readonly CryCharacter _character;
        private readonly Func<string, CancellationToken, Task<Stream>> _getStreamAsync;
        private readonly bool _useAnimation;
        private readonly bool _exportOnlyRequiredTextures;
        private readonly CancellationToken _cancellationToken;

        private readonly GltfTuple _gltf;
        private readonly GltfNode _rootNode;
        private readonly Dictionary<uint, int> _controllerIdToNodeIndex = new();
        private readonly Dictionary<uint, int> _controllerIdToBoneIndex = new();
        private readonly Dictionary<string, int> _materialNameToIndex = new();
        private readonly List<Material> _flatMaterials = new();

        public GltfExporter(
            CryCharacter character,
            Func<string, CancellationToken, Task<Stream>> streamAsyncOpener,
            bool useAnimation,
            bool exportOnlyRequiredTextures,
            CancellationToken cancellationToken) {
            _character = character;
            _getStreamAsync = streamAsyncOpener;
            _useAnimation = useAnimation;
            _exportOnlyRequiredTextures = exportOnlyRequiredTextures;
            _cancellationToken = cancellationToken;

            _gltf = new();
            _gltf.Root.Scenes[_gltf.Root.Scene].Nodes.Add(_gltf.Root.Nodes.AddAndGetIndex(_rootNode = new()));
            _gltf.Root.ExtensionsUsed.Add("KHR_materials_specular");
            _gltf.Root.ExtensionsUsed.Add("KHR_materials_pbrSpecularGlossiness");
            _gltf.Root.ExtensionsUsed.Add("KHR_materials_emissive_strength");
        }

        public async Task<GltfTuple> Convert() {
            Step01WriteSkin();
            await Step02WriteMaterials();
            Step03WriteMeshes();
            if (_useAnimation)
                Step04WriteAnimations();
            return _gltf;
        }

        private void Step01WriteSkin() {
            var controllers = _character.Model.Controllers.OrderBy(x => x.Depth).ToArray();
            if (!controllers.Any())
                return;

            _controllerIdToNodeIndex.EnsureCapacity(controllers.Length);
            _controllerIdToBoneIndex.EnsureCapacity(controllers.Length);

            var skin = new GltfSkin {Joints = new()};
            _rootNode.Skin = _gltf.Root.Skins.AddAndGetIndex(skin);

            foreach (var boneIndex in Enumerable.Range(0, controllers.Length)) {
                ref var controller = ref controllers[boneIndex];

                var (tra, rot) = controller.Decomposed;

                var nodeIndex = _gltf.Root.Nodes.AddAndGetIndex(
                    new() {
                        Name = controller.Id == controller.CalculatedId
                            ? controller.Name
                            : $"{controller.Name}$${controller.Id:x08}",
                        Children = new(controller.Children.Count),
                        Translation = SwapAxes(tra).ToFloatList(Vector3.Zero, 1e-6f),
                        Rotation = SwapAxes(rot).ToFloatList(Quaternion.Identity, 1e-6f),
                    });
                skin.Joints.Add(nodeIndex);
                _controllerIdToNodeIndex.Add(controller.Id, nodeIndex);
                _controllerIdToBoneIndex.Add(controller.Id, boneIndex);

                if (controller.Parent is null)
                    _rootNode.Children.Add(nodeIndex);
                else
                    _gltf.Root.Nodes[_controllerIdToNodeIndex[controller.Parent.Id]].Children.Add(nodeIndex);
            }

            skin.InverseBindMatrices = _gltf.AddAccessor(
                null,
                controllers.Select(x => SwapAxes(x.AbsoluteBindPoseMatrix).Normalize()).ToArray().AsSpan());
        }

        private class ParsedGenMask {
            public readonly bool UseScatterInNormalMap;
            public readonly bool UseHeightInNormalMap;
            public readonly bool UseSpecAlphaInDiffuseMap;
            public readonly bool UseGlossInSpecularMap;
            public readonly bool UseNormalMapInDetailMap;
            public readonly bool UseInvertedBlendMap;
            public readonly bool UseGlowDecalMap;
            public readonly bool UseBlendSpecularInSubSurfaceMap;
            public readonly bool UseBlendDiffuseInCustomMap;
            public readonly bool UseDirtLayerInCustomMap2;
            public readonly bool UseAddNormalInCustomMap2;
            public readonly bool UseBlurRefractionInCustomMap2;

            public ParsedGenMask(IReadOnlySet<string> genMaskSet) {
                UseScatterInNormalMap = genMaskSet.Contains("TEMP_SKIN");
                UseHeightInNormalMap = genMaskSet.Contains("BLENDHEIGHT_DISPL");
                UseSpecAlphaInDiffuseMap = genMaskSet.Contains("GLOSS_DIFFUSEALPHA");
                UseGlossInSpecularMap = genMaskSet.Contains("SPECULARPOW_GLOSSALPHA");
                UseNormalMapInDetailMap = genMaskSet.Contains("DETAIL_TEXTURE_IS_NORMALMAP");
                UseInvertedBlendMap = genMaskSet.Contains("BLENDHEIGHT_INVERT");
                UseGlowDecalMap = genMaskSet.Contains("DECAL_ALPHAGLOW");
                UseBlendSpecularInSubSurfaceMap = genMaskSet.Contains("BLENDSPECULAR");
                UseBlendDiffuseInCustomMap = genMaskSet.Contains("BLENDLAYER");
                UseDirtLayerInCustomMap2 = genMaskSet.Contains("DIRTLAYER");
                UseAddNormalInCustomMap2 = genMaskSet.Contains("BLENDNORMAL_ADD");
                UseBlurRefractionInCustomMap2 = genMaskSet.Contains("BLUR_REFRACTION");
            }
        }

        private readonly struct DerivativeTextureKey : IEquatable<DerivativeTextureKey> {
            public readonly string Name;
            public readonly object? R;
            public readonly object? G;
            public readonly object? B;
            public readonly object? A;

            public DerivativeTextureKey(string name, object? r, object? g, object? b, object? a) {
                Name = name;
                R = r;
                G = g;
                B = b;
                A = a;
            }

            public bool Equals(DerivativeTextureKey other) => Name == other.Name && Equals(R, other.R) &&
                Equals(G, other.G) && Equals(B, other.B) && Equals(A, other.A);

            public override bool Equals(object? obj) => obj is DerivativeTextureKey other && Equals(other);

            public override int GetHashCode() => HashCode.Combine(Name, R, G, B, A);

            public override string ToString() => Name;

            public static DerivativeTextureKey Raw(object source) =>
                new($"Raw[{source}]", source, source, source, source);

            public static DerivativeTextureKey Gloss(object source) =>
                new($"Gloss[{source}]", source, null, null, null);

            public static DerivativeTextureKey Scatter(object source) =>
                new($"Scatter[{source}]", source, null, null, null);

            public static DerivativeTextureKey Height(object source) =>
                new($"Height[{source}]", source, null, null, null);

            public static DerivativeTextureKey SpecularOpaque(object source) =>
                new($"SpecularOpaque[{source}]", source, source, source, null);

            public static DerivativeTextureKey SpecularAlpha(object source, object alpha) =>
                new($"SpecularAlpha[{source},{alpha}]", source, source, source, alpha);

            public static DerivativeTextureKey SpecularGloss(object specular, object gloss) =>
                new($"SpecularGloss[{specular},{gloss}]", specular, specular, specular, gloss);

            public static DerivativeTextureKey DiffuseOpaque(object source) =>
                new($"DiffuseOpaque[{source}]", source, source, source, null);

            public static DerivativeTextureKey DiffuseAlpha(object source) =>
                new($"DiffuseAlpha[{source}]", source, source, source, source);

            public static DerivativeTextureKey MetallicRoughness(object? metallic, object? roughness) =>
                new($"MetallicRoughness[{metallic},{roughness}]", metallic, roughness, null, null);

            public static DerivativeTextureKey Normal(object source) =>
                new($"Normal[{source}]", source, source, source, null);
        }

        private class AddedTexture<TPixel> where TPixel : unmanaged, IPixel<TPixel> {
            public readonly string Path;
            public readonly bool UseAlphaChannel;
            public readonly DdsFile? DdsFile;
            public readonly Image<TPixel> Image;

            public bool HasAlphaValues;

            private int _gltfTextureIndex;

            public AddedTexture(
                string path,
                int textureIndex,
                bool useAlphaChannel,
                Image<TPixel> image,
                DdsFile? ddsFile) {
                Path = path;
                _gltfTextureIndex = textureIndex;
                UseAlphaChannel = useAlphaChannel;
                Image = image;
                DdsFile = ddsFile;
                HasAlphaValues = false;
                if (image is Image<Bgra32> ib32)
                    ib32.ProcessPixelRows(
                        a => {
                            for (var i = 0; i < a.Height; i++) {
                                var s = a.GetRowSpan(i);
                                for (var j = 0; j < a.Width; j++) {
                                    HasAlphaValues = s[j].A != 255;
                                    if (HasAlphaValues)
                                        return;
                                }
                            }
                        });
            }

            public bool HasGltfTextureIndex => _gltfTextureIndex != -1;

            private int GetGltfTextureIndex(GltfTuple gltf) {
                if (_gltfTextureIndex == -1) {
                    _gltfTextureIndex = gltf.AddTexture(
                        Path,
                        Image,
                        UseAlphaChannel ? PngColorType.RgbWithAlpha : PngColorType.Rgb,
                        DdsFile);
                }

                return _gltfTextureIndex;
            }

            public GltfTextureInfo GetGltfTextureInfo(GltfTuple gltf) => new() {Index = GetGltfTextureIndex(gltf)};

            // public string SuggestSuffix() =>
            //     CryTexture.Map switch {
            //         Texture.MapTypeEnum.Diffuse => "dif",
            //         Texture.MapTypeEnum.Normals when UseScatterInNormalMap => "bsg_nrm",
            //         Texture.MapTypeEnum.Normals when UseHeightInNormalMap => "bhg_nrm",
            //         Texture.MapTypeEnum.Normals => "nrm",
            //         Texture.MapTypeEnum.Specular => "spec",
            //         Texture.MapTypeEnum.Env => "env",
            //         Texture.MapTypeEnum.Detail when UseNormalMapInDetailMap => "n_detail",
            //         Texture.MapTypeEnum.Detail => "detail",
            //         Texture.MapTypeEnum.Opacity when UseInvertedBlendMap => "invert_blend",
            //         Texture.MapTypeEnum.Opacity => "blend",
            //         Texture.MapTypeEnum.Decal when UseGlowDecalMap => "glow_decal",
            //         Texture.MapTypeEnum.Decal => "decal",
            //         Texture.MapTypeEnum.SubSurface when UseBlendSpecularInSubSurfaceMap => "blendspec_sss",
            //         Texture.MapTypeEnum.SubSurface => "sss",
            //         Texture.MapTypeEnum.Custom when UseBlendDiffuseInCustomMap => "blenddif_cust",
            //         Texture.MapTypeEnum.Custom => "cust",
            //         Texture.MapTypeEnum.Custom2 when UseDirtLayerInCustomMap2 => "dirt_cust2",
            //         Texture.MapTypeEnum.Custom2 when UseAddNormalInCustomMap2  => "addnrm_cust2",
            //         Texture.MapTypeEnum.Custom2 when UseBlurRefractionInCustomMap2  => "refraction_cust2",
            //         Texture.MapTypeEnum.Custom2 => "cust2",
            //         _ => $"{(int) CryTexture.Map}",
            //     };

            public void ClearAlpha() {
                HasAlphaValues = false;
                if (Image is Image<Bgra32> ib32)
                    ib32.ProcessPixelRows(
                        a => {
                            for (var y = 0; y < a.Height; y++) {
                                var glossRow = a.GetRowSpan(y);
                                for (var x = 0; x < a.Width; x++)
                                    glossRow[x].A = 255;
                            }
                        });
            }

            public override string ToString() => Path;
        }

        private sealed class DerivativeTextureManager<TPixel> : Dictionary<DerivativeTextureKey, AddedTexture<TPixel>>,
            IDisposable
            where TPixel : unmanaged, IPixel<TPixel> {
            public bool GetOrAdd<TPixel2>(
                DerivativeTextureKey key,
                AddedTexture<TPixel2> baseImage,
                out AddedTexture<TPixel> res) where TPixel2 : unmanaged, IPixel<TPixel2> {
                if (TryGetValue(key, out res!))
                    return false;

                if (typeof(TPixel) == typeof(TPixel2))
                    res = new(
                        key.Name,
                        -1,
                        false,
                        (Image<TPixel>) (object) baseImage.Image.Clone(),
                        null);
                else
                    res = new(
                        key.Name,
                        -1,
                        false,
                        new(baseImage.Image.Width, baseImage.Image.Height),
                        null);

                this[key] = res;
                return true;
            }

            public void Dispose() {
                foreach (var k in Values)
                    k.Image.Dispose();
                Clear();
            }
        }

        private async Task Step02WriteMaterials() {
            if (_character.Model.Material is not null) {
                _flatMaterials.Add(_character.Model.Material);
                for (var i = 0; i < _flatMaterials.Count; i++) {
                    if (!_flatMaterials[i].MaterialFlags.HasFlag(MaterialFlags.MultiSubmtl))
                        continue;
                    if (_flatMaterials[i].SubMaterials is not { } subMaterials)
                        continue;
                    _flatMaterials.AddRange(subMaterials.Where(x => x is not null)!);
                }
            }

            var cryMaterials = _character.Model.Nodes
                .SelectMany(x => x.Meshes.Select(y => y.MaterialName))
                .Distinct()
                .Select(x => _flatMaterials.SingleOrDefault(y => y.Name == x))
                .Where(x => x is not null)
                .Cast<Material>()
                .ToArray();

            using var src32 = new DerivativeTextureManager<Bgra32>();
            using var deriv32 = new DerivativeTextureManager<Bgra32>();
            using var deriv8 = new DerivativeTextureManager<L8>();

            var texturePaths = cryMaterials.Where(x => x.Textures is not null)
                .SelectMany(x => x.Textures!)
                .Where(x => x.File is not null && x.File != "nearest_cubemap")
                .Select(x => x.File!)
                .DistinctBy(x => x.ToLowerInvariant())
                .ToArray();
            var textureNames = texturePaths.StripCommonParentPaths()
                .Select(x => Path.GetFileNameWithoutExtension(x.Replace("/", "_")))
                .ToArray();

            foreach (var (textureTask, name) in texturePaths.Zip(textureNames)
                         .Select(
                             pathAndName => Task.Run(
                                 async () => {
                                     try {
                                         var (path, name) = pathAndName;
                                         if (path.StartsWith("engine/", StringComparison.OrdinalIgnoreCase))
                                             path = path[7..];
                                         var ddsFile = new DdsFile(
                                             name + ".dds",
                                             await _getStreamAsync(
                                                 Path.ChangeExtension(path, ".dds"),
                                                 _cancellationToken));
                                         return new AddedTexture<Bgra32>(
                                             name,
                                             -1,
                                             ddsFile.PixelFormat.Alpha != AlphaType.None,
                                             ddsFile.ToImageBgra32(0, 0, 0),
                                             _exportOnlyRequiredTextures ? null : ddsFile);
                                     } catch (FileNotFoundException) {
                                         return null;
                                     }
                                 },
                                 _cancellationToken))
                         .Zip(textureNames)) {
                if (await textureTask is { } texture)
                    src32.Add(DerivativeTextureKey.Raw(name), texture);
            }

            foreach (var cryMaterial in cryMaterials) {
                var name = cryMaterial.Name!;
                var materialTextures =
                    ((IEnumerable<Texture>?) cryMaterial.Textures ?? Array.Empty<Texture>())
                    .Where(x => x.File != "nearest_cubemap" && x.File is not null)
                    .Select(
                        x => Tuple.Create(
                            x.Map,
                            src32.GetValueOrDefault(
                                DerivativeTextureKey.Raw(
                                    textureNames.Zip(texturePaths).Single(
                                        y => string.Equals(
                                            y.Second,
                                            x.File,
                                            StringComparison.OrdinalIgnoreCase)).First))))
                    .Where(x => x.Item2 is not null)
                    .ToDictionary(x => x.Item1, x => x.Item2!);

                var genMask = new ParsedGenMask(cryMaterial.GenMaskSet);

                var diffuseRaw = materialTextures.GetValueOrDefault(Texture.MapTypeEnum.Diffuse);
                var normalRaw = materialTextures.GetValueOrDefault(Texture.MapTypeEnum.Normals);
                var specularRaw = materialTextures.GetValueOrDefault(Texture.MapTypeEnum.Specular);

                AddedTexture<Bgra32>? specularTexture = null;
                AddedTexture<Bgra32>? specularGlossinessTexture = null;
                AddedTexture<Bgra32>? normalTexture = null;
                AddedTexture<Bgra32>? diffuseTexture = null;
                AddedTexture<Bgra32>? metallicRoughnessTexture = null;
                AddedTexture<L8>? glossFromSpecularTexture = null;
                AddedTexture<L8>? glossFromNormalTexture = null;

                if (specularRaw is not null) {
                    if (genMask.UseGlossInSpecularMap)
                        specularGlossinessTexture = specularRaw;

                    if (genMask.UseGlossInSpecularMap && deriv8.GetOrAdd(
                            DerivativeTextureKey.Gloss(specularRaw),
                            specularRaw,
                            out glossFromSpecularTexture))
                        glossFromSpecularTexture.Image.ProcessPixelRows(
                            specularRaw.Image,
                            (gloss, specular) => {
                                for (var y = 0; y < gloss.Height; y++) {
                                    var glossRow = gloss.GetRowSpan(y);
                                    var specularRow = specular.GetRowSpan(y);
                                    for (var x = 0; x < gloss.Width; x++)
                                        glossRow[x].PackedValue = specularRow[x].A;
                                }
                            });

                    if (specularRaw.HasAlphaValues) {
                        if (deriv32.GetOrAdd(
                                DerivativeTextureKey.SpecularOpaque(specularRaw),
                                specularRaw,
                                out specularTexture))
                            specularTexture.ClearAlpha();
                    } else
                        specularTexture = specularRaw;
                }

                if (diffuseRaw is not null) {
                    if (diffuseRaw.HasAlphaValues) {
                        if (genMask.UseSpecAlphaInDiffuseMap) {
                            if (specularTexture is null) {
                                deriv32.GetOrAdd(
                                    DerivativeTextureKey.SpecularOpaque(
                                        diffuseRaw),
                                    diffuseRaw,
                                    out specularTexture);
                            } else {
                                if (deriv32.GetOrAdd(
                                        DerivativeTextureKey.SpecularAlpha(specularTexture, diffuseRaw),
                                        diffuseRaw,
                                        out specularTexture)) {
                                    specularTexture.Image.ProcessPixelRows(
                                        diffuseRaw.Image,
                                        (specular, diffuse) => {
                                            for (var y = 0; y < specular.Height; y++) {
                                                var specularRow = specular.GetRowSpan(y);
                                                var diffuseRow = diffuse.GetRowSpan(y);
                                                for (var x = 0; x < specular.Width; x++)
                                                    specularRow[x].A = diffuseRow[x].A;
                                            }
                                        });
                                    specularTexture.HasAlphaValues = true;
                                }
                            }
                        }

                        if (cryMaterial.AlphaTest == 0) {
                            if (deriv32.GetOrAdd(
                                    DerivativeTextureKey.DiffuseOpaque(diffuseRaw),
                                    diffuseRaw,
                                    out diffuseTexture)) {
                                diffuseTexture.ClearAlpha();
                            }
                        }
                    }

                    diffuseTexture ??= diffuseRaw;
                }

                if (normalRaw is not null) {
                    if (genMask.UseScatterInNormalMap || genMask.UseHeightInNormalMap) {
                        var added = false;
                        added |= deriv32.GetOrAdd(
                            DerivativeTextureKey.Normal(normalRaw),
                            normalRaw,
                            out normalTexture);
                        added |= deriv8.GetOrAdd(
                            genMask.UseScatterInNormalMap
                                ? DerivativeTextureKey.Scatter(normalRaw)
                                : DerivativeTextureKey.Height(normalRaw),
                            normalRaw,
                            out var redTexture);
                        added |= deriv8.GetOrAdd(
                            DerivativeTextureKey.Gloss(normalRaw),
                            normalRaw,
                            out glossFromNormalTexture);
                        if (added) {
                            normalTexture.Image.ProcessPixelRows(
                                redTexture.Image,
                                glossFromNormalTexture.Image,
                                (normal, red, gloss) => {
                                    for (var y = 0; y < normal.Height; y++) {
                                        var normalRow = normal.GetRowSpan(y);
                                        var redRow = red.GetRowSpan(y);
                                        var glossRow = gloss.GetRowSpan(y);
                                        for (var x = 0; x < normal.Width; x++) {
                                            var src = normalRow[x];
                                            var nz = MathF.Sqrt(
                                                1
                                                - MathF.Pow(src.G / 255f * 2 - 1, 2)
                                                - MathF.Pow(src.A / 255f * 2 - 1, 2)
                                            ) / 2 + 0.5f;
                                            normalRow[x] = new(src.G, src.A, (byte) (nz * 255f));
                                            redRow[x].PackedValue = src.R;
                                            glossRow[x].PackedValue = src.B;
                                        }
                                    }
                                });
                        }
                    } else
                        normalTexture = normalRaw;
                }

                var glossTextureAny = glossFromSpecularTexture ?? glossFromNormalTexture;
                if (specularGlossinessTexture is null && specularTexture is not null && glossTextureAny is not null &&
                    deriv32.GetOrAdd(
                        DerivativeTextureKey.SpecularGloss(specularTexture, glossTextureAny),
                        specularTexture,
                        out specularGlossinessTexture)) {
                    specularGlossinessTexture.Image.ProcessPixelRows(
                        glossTextureAny.Image,
                        (specular, gloss) => {
                            for (var y = 0; y < specular.Height; y++) {
                                var specularRow = specular.GetRowSpan(y);
                                var glossRow = gloss.GetRowSpan(y);
                                for (var x = 0; x < specular.Width; x++)
                                    specularRow[x].A = glossRow[x].PackedValue;
                            }
                        });
                }

                if (glossTextureAny is not null && deriv32.GetOrAdd(
                        DerivativeTextureKey.MetallicRoughness(null, glossTextureAny),
                        glossTextureAny,
                        out metallicRoughnessTexture)) {
                    metallicRoughnessTexture.HasAlphaValues = false;
                    metallicRoughnessTexture.Image.ProcessPixelRows(
                        glossTextureAny.Image,
                        (mr, gloss) => {
                            for (var y = 0; y < mr.Height; y++) {
                                var mrRow = mr.GetRowSpan(y);
                                var glossRow = gloss.GetRowSpan(y);
                                // Roughness = 1 - Glossiness
                                for (var x = 0; x < mr.Width; x++) {
                                    mrRow[x].R = 255;
                                    mrRow[x].G = (byte) (255 - glossRow[x].PackedValue);
                                    mrRow[x].B = 255;
                                    mrRow[x].A = 255;
                                }
                            }
                        });
                }

                _materialNameToIndex[name] = _gltf.Root.Materials.AddAndGetIndex(
                    new() {
                        Name = name,
                        AlphaCutoff = cryMaterial.AlphaTest == 0 ? null : cryMaterial.AlphaTest,
                        AlphaMode = cryMaterial.AlphaTest == 0
                            ? GltfMaterialAlphaMode.Opaque
                            : GltfMaterialAlphaMode.Mask,
                        DoubleSided = cryMaterial.MaterialFlags.HasFlag(MaterialFlags.TwoSided),
                        NormalTexture = normalTexture?.GetGltfTextureInfo(_gltf),
                        PbrMetallicRoughness = new() {
                            BaseColorTexture = diffuseTexture?.GetGltfTextureInfo(_gltf),
                            BaseColorFactor = new[] {
                                (cryMaterial.DiffuseColor ?? Vector3.One).X,
                                (cryMaterial.DiffuseColor ?? Vector3.One).Y,
                                (cryMaterial.DiffuseColor ?? Vector3.One).Z,
                                float.Clamp(cryMaterial.Opacity, 0f, 1f),
                            },
                            MetallicFactor = float.Clamp(cryMaterial.PublicParams?.Metalness ?? 1, 0, 1),
                            RoughnessFactor = float.Clamp((255 - cryMaterial.Shininess) / 255f, 0, 1),
                            MetallicRoughnessTexture = metallicRoughnessTexture?.GetGltfTextureInfo(_gltf),
                        },
                        EmissiveFactor = new[] {
                            float.Clamp(cryMaterial.GlowAmount, 0f, 1f),
                            float.Clamp(cryMaterial.GlowAmount, 0f, 1f),
                            float.Clamp(cryMaterial.GlowAmount, 0f, 1f)
                        },
                        EmissiveTexture = diffuseTexture?.GetGltfTextureInfo(_gltf),
                        Extensions = new() {
                            KhrMaterialsSpecular = new() {
                                SpecularTexture = specularTexture?.GetGltfTextureInfo(_gltf),
                                SpecularFactor = float.Clamp(cryMaterial.Shininess / 255f, 0f, 1f),
                                SpecularColorFactor = new[] {
                                    (cryMaterial.SpecularColor ?? Vector3.One).X,
                                    (cryMaterial.SpecularColor ?? Vector3.One).Y,
                                    (cryMaterial.SpecularColor ?? Vector3.One).Z,
                                },
                                SpecularColorTexture = specularTexture?.GetGltfTextureInfo(_gltf),
                            },
                            KhrMaterialsPbrSpecularGlossiness = new() {
                                DiffuseFactor = new[] {
                                    (cryMaterial.DiffuseColor ?? Vector3.One).X,
                                    (cryMaterial.DiffuseColor ?? Vector3.One).Y,
                                    (cryMaterial.DiffuseColor ?? Vector3.One).Z,
                                    float.Clamp(cryMaterial.Opacity, 0f, 1f),
                                },
                                DiffuseTexture = diffuseTexture?.GetGltfTextureInfo(_gltf),
                                SpecularFactor = new[] {
                                    (cryMaterial.SpecularColor ?? Vector3.One).X,
                                    (cryMaterial.SpecularColor ?? Vector3.One).Y,
                                    (cryMaterial.SpecularColor ?? Vector3.One).Z,
                                },
                                GlossinessFactor = float.Clamp(cryMaterial.Shininess / 255f, 0f, 1f),
                                SpecularGlossinessTexture = specularGlossinessTexture?.GetGltfTextureInfo(_gltf),
                            },
                            KhrMaterialsEmissiveStrength = cryMaterial.GlowAmount <= 0
                                ? null
                                : new() {
                                    EmissiveStrength = cryMaterial.GlowAmount,
                                },
                        },
                    });
            }

            if (!_exportOnlyRequiredTextures) {
                foreach (var k in src32.Values.Concat(deriv32.Values).Where(x => !x.HasGltfTextureIndex))
                    _gltf.AddTexture(
                        "extras/" + k.Path,
                        k.Image,
                        k.HasAlphaValues ? PngColorType.RgbWithAlpha : PngColorType.Rgb,
                        k.DdsFile);
                foreach (var k in deriv8.Values.Where(x => !x.HasGltfTextureIndex))
                    _gltf.AddTexture(
                        "extras/" + k.Path,
                        k.Image,
                        PngColorType.Grayscale,
                        k.DdsFile);
            }
        }

        private void Step03WriteMeshes() {
            var mesh = new GltfMesh();
            foreach (var cryNode in _character.Model.Nodes)
            foreach (var cryMesh in cryNode.Meshes) {
                var cryMaterial = _flatMaterials.SingleOrDefault(x => x.Name == cryMesh.MaterialName);
                mesh.Primitives.Add(
                    new() {
                        Attributes = new() {
                            Position = _gltf.AddAccessor(
                                null,
                                cryMesh.Vertices.Select(x => SwapAxes(x.Position)).ToArray().AsSpan(),
                                target: GltfBufferViewTarget.ArrayBuffer),
                            Normal = _gltf.AddAccessor(
                                null,
                                cryMesh.Vertices.Select(x => SwapAxes(x.Normal)).ToArray().AsSpan(),
                                target: GltfBufferViewTarget.ArrayBuffer),
                            Tangent = _gltf.AddAccessor(
                                null,
                                cryMesh.Vertices.Select(x => SwapAxesTangent(x.Tangent.Tangent)).ToArray().AsSpan(),
                                target: GltfBufferViewTarget.ArrayBuffer),
                            Color0 = cryMaterial?.ContainsGenMask("VERTCOLORS") is not true || !cryNode.HasColors
                                ? null
                                : _gltf.AddAccessor(
                                    null,
                                    cryMesh.Vertices.Select(x => new Vector4<float>(x.Color.Select(y => y / 255f)))
                                        .ToArray()
                                        .AsSpan(),
                                    target: GltfBufferViewTarget.ArrayBuffer),
                            TexCoord0 = _gltf.AddAccessor(
                                null,
                                cryMesh.Vertices.Select(x => x.TexCoord).ToArray().AsSpan(),
                                target: GltfBufferViewTarget.ArrayBuffer),
                            Weights0 = !_controllerIdToBoneIndex.Any()
                                ? null
                                : _gltf.AddAccessor(
                                    null,
                                    cryMesh.Vertices.Select(x => x.Weights).ToArray().AsSpan(),
                                    target: GltfBufferViewTarget.ArrayBuffer),
                            Joints0 = !_controllerIdToBoneIndex.Any()
                                ? null
                                : _gltf.AddAccessor(
                                    null,
                                    cryMesh.Vertices.Select(
                                            x => new Vector4<ushort>(
                                                x.ControllerIds.Select(
                                                    y => (ushort) _controllerIdToBoneIndex.GetValueOrDefault(y))))
                                        .ToArray()
                                        .AsSpan(),
                                    target: GltfBufferViewTarget.ArrayBuffer),
                        },
                        Indices = _gltf.AddAccessor(
                            null,
                            cryMesh.Indices.AsSpan(),
                            target: GltfBufferViewTarget.ElementArrayBuffer),
                        Material = cryMesh.MaterialName is null ? null :
                            _materialNameToIndex.TryGetValue(cryMesh.MaterialName, out var mi) ? mi : null,
                    });
            }

            _rootNode.Mesh = _gltf.Root.Meshes.AddAndGetIndex(mesh);
        }

        private void Step04WriteAnimations() {
            if (_character.CryAnimationDatabase?.Animations is not { } animations || !animations.Any())
                return;

            var positionAccessors = animations.Values.SelectMany(x => x.Tracks.Values.Select(y => y.Position))
                .Where(x => x is not null)
                .Distinct()
                .ToDictionary(x => x!, x => _gltf.AddAccessor(null, x!.Data.Select(SwapAxes).ToArray().AsSpan()));

            var rotationAccessors = animations.Values.SelectMany(x => x.Tracks.Values.Select(y => y.Rotation))
                .Where(x => x is not null)
                .Distinct()
                .ToDictionary(x => x!, x => _gltf.AddAccessor(null, x!.Select(SwapAxes).ToArray().AsSpan()));

            var timeAccessors = new Dictionary<Tuple<ControllerKeyTime, int>, int>();

            var names = animations.Keys.Select(x => Path.ChangeExtension(x, null)).StripCommonParentPaths();
            foreach (var (animationName, animation) in animations.Values.Zip(names)
                         .OrderBy(x => x.Second.ToLowerInvariant())
                         .Select(x => (x.Second, x.First))) {
                var target = new GltfAnimation {Name = animationName};

                int GetTimeAccessor(ControllerKeyTime keyTime) {
                    var timeKey = Tuple.Create(keyTime, animation.MotionParams.Start);
                    if (timeAccessors.TryGetValue(timeKey, out var timeAccessor))
                        return timeAccessor;

                    timeAccessor = _gltf.AddAccessor(
                        null,
                        keyTime.Ticks.Select(y => (y - animation.MotionParams.Start) / 30f).ToArray().AsSpan());
                    timeAccessors.Add(timeKey, timeAccessor);
                    return timeAccessor;
                }

                foreach (var (controllerId, tracks) in animation.Tracks) {
                    if (tracks is {Position: { } pos, PositionTime: { } posTime}) {
                        target.Channels.Add(
                            new() {
                                Sampler = target.Samplers.AddAndGetIndex(
                                    new() {
                                        Input = GetTimeAccessor(posTime),
                                        Output = positionAccessors[pos],
                                        Interpolation = GltfAnimationSamplerInterpolation.Linear,
                                    }),
                                Target = new() {
                                    Node = _controllerIdToNodeIndex[controllerId],
                                    Path = GltfAnimationChannelTargetPath.Translation,
                                },
                            });
                    }

                    if (tracks is {Rotation: { } rot, RotationTime: { } rotTime})
                        target.Channels.Add(
                            new() {
                                Sampler = target.Samplers.AddAndGetIndex(
                                    new() {
                                        Input = GetTimeAccessor(rotTime),
                                        Output = rotationAccessors[rot],
                                        Interpolation = GltfAnimationSamplerInterpolation.Linear,
                                    }),
                                Target = new() {
                                    Node = _controllerIdToNodeIndex[controllerId],
                                    Path = GltfAnimationChannelTargetPath.Rotation,
                                },
                            });
                }

                if (!target.Channels.Any() || !target.Samplers.Any())
                    continue;

                _gltf.Root.Animations.Add(target);
            }
        }
    }
}
