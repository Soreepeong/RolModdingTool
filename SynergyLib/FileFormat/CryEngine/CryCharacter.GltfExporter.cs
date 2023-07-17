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
using SynergyLib.FileFormat.CryEngine.CryXml;
using SynergyLib.FileFormat.CryEngine.CryXml.MaterialElements;
using SynergyLib.FileFormat.DirectDrawSurface;
using SynergyLib.FileFormat.DirectDrawSurface.PixelFormats;
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
        private readonly CancellationToken _cancellationToken;

        private readonly GltfTuple _gltf;
        private readonly GltfNode _rootNode;
        private readonly Dictionary<uint, int> _controllerIdToNodeIndex;
        private readonly Dictionary<uint, int> _controllerIdToBoneIndex;
        private readonly Dictionary<string, int> _materialNameToIndex;

        public GltfExporter(
            CryCharacter character,
            Func<string, CancellationToken, Task<Stream>> streamAsyncOpener,
            CancellationToken cancellationToken) {
            _character = character;
            _getStreamAsync = streamAsyncOpener;
            _cancellationToken = cancellationToken;

            _gltf = new();
            _gltf.Root.Scenes[_gltf.Root.Scene].Nodes.Add(_gltf.Root.Nodes.AddAndGetIndex(_rootNode = new()));
            _gltf.Root.ExtensionsUsed.Add("KHR_materials_specular");
            _gltf.Root.ExtensionsUsed.Add("KHR_materials_pbrSpecularGlossiness");
            _gltf.Root.ExtensionsUsed.Add("KHR_materials_emissive_strength");
            _controllerIdToNodeIndex = new();
            _controllerIdToBoneIndex = new();
            _materialNameToIndex = new();
        }

        public async Task<GltfTuple> Convert() {
            Step01WriteSkin();
            await Step02WriteMaterials();
            Step03WriteMeshes();
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

        private async Task<Tuple<int, Texture.MapTypeEnum, Image<Bgra32>, DdsFile>> Step02WriteMaterialsGetTextures(
            Material material,
            Texture texture) {
            var path = Path.ChangeExtension(texture.File!, ".dds");
            var useScatterInNormalMap = material.ContainsGenMask("TEMP_SKIN");
            var useHeightInNormalMap = material.ContainsGenMask("BLENDHEIGHT_DISPL");
            var suffix = texture.Map switch {
                Texture.MapTypeEnum.Diffuse => "dif",
                Texture.MapTypeEnum.Normals when useScatterInNormalMap => "bsg_nrm",
                Texture.MapTypeEnum.Normals when useHeightInNormalMap => "bhg_nrm",
                Texture.MapTypeEnum.Normals => "nrm",
                Texture.MapTypeEnum.Specular => "spec",
                Texture.MapTypeEnum.Env => "env",
                Texture.MapTypeEnum.Detail when material.ContainsGenMask(
                        "DETAIL_TEXTURE_IS_NORMALMAP")
                    => "n_detail",
                Texture.MapTypeEnum.Detail => "detail",
                Texture.MapTypeEnum.Opacity when material.ContainsGenMask("BLENDHEIGHT_INVERT")
                    => "invert_blend",
                Texture.MapTypeEnum.Opacity => "blend",
                Texture.MapTypeEnum.Decal when material.ContainsGenMask("DECAL_ALPHAGLOW") =>
                    "glow_decal",
                Texture.MapTypeEnum.Decal => "decal",
                Texture.MapTypeEnum.SubSurface when material.ContainsGenMask("BLENDSPECULAR")
                    => "blendspec_sss",
                Texture.MapTypeEnum.SubSurface => "sss",
                Texture.MapTypeEnum.Custom when material.ContainsGenMask("BLENDLAYER") =>
                    "blenddif_cust",
                Texture.MapTypeEnum.Custom => "cust",
                Texture.MapTypeEnum.Custom2 when material.ContainsGenMask("DIRTLAYER") =>
                    "dirt_cust2",
                Texture.MapTypeEnum.Custom2 when material.ContainsGenMask("BLENDNORMAL_ADD") =>
                    "addnrm_cust2",
                Texture.MapTypeEnum.Custom2 when material.ContainsGenMask("BLUR_REFRACTION")
                    => "refraction_cust2",
                Texture.MapTypeEnum.Custom2 => "cust2",
                _ => $"{(int) texture.Map}",
            };
            var ddsFile = new DdsFile(Path.GetFileName(path), await _getStreamAsync(path, _cancellationToken));
            var image = ddsFile.ToImageBgra32(0, 0, 0);
            var textureIndex = _gltf.AddTexture(
                $"{material.Name}_{suffix}.png",
                image,
                ddsFile.PixelFormat.Alpha == AlphaType.None
                    ? ddsFile.PixelFormat is LumiPixelFormat
                        ? PngColorType.Grayscale
                        : PngColorType.Rgb
                    : ddsFile.PixelFormat is LumiPixelFormat
                        ? PngColorType.GrayscaleWithAlpha
                        : PngColorType.RgbWithAlpha,
                ddsFile);
            return Tuple.Create(textureIndex, texture.Map, image, ddsFile);
        }

        private async Task Step02WriteMaterials() {
            foreach (var cryMaterial in _character.Model.Meshes
                         .Select(x => x.MaterialName)
                         .Distinct()
                         .Select(x => _character.Model.Material.SubMaterials?.SingleOrDefault(y => y.Name == x))
                         .Where(x => x is not null)
                         .Cast<Material>()) {
                var name = cryMaterial.Name!;
                var textures = await Task.WhenAll(
                    ((IEnumerable<Texture>?) cryMaterial.Textures ?? Array.Empty<Texture>()).Select(
                        texture => Task.Run(
                            () => Step02WriteMaterialsGetTextures(cryMaterial, texture),
                            _cancellationToken)));

                var useScatterInNormalMap = cryMaterial.ContainsGenMask("TEMP_SKIN");
                var useHeightInNormalMap = cryMaterial.ContainsGenMask("BLENDHEIGHT_DISPL");
                var useSpecAlphaInDiffuseMap = cryMaterial.ContainsGenMask("GLOSS_DIFFUSEALPHA");
                var useGlossInSpecularMap = cryMaterial.ContainsGenMask("SPECULARPOW_GLOSSALPHA");

                var diffuseRaw = textures.SingleOrDefault(x => x.Item2 == Texture.MapTypeEnum.Diffuse);
                var normalRaw = textures.SingleOrDefault(x => x.Item2 == Texture.MapTypeEnum.Normals);
                var specularRaw = textures.SingleOrDefault(x => x.Item2 == Texture.MapTypeEnum.Specular);

                var specularImage = specularRaw?.Item3;
                var normalImage = normalRaw?.Item3;
                var diffuseImage = diffuseRaw?.Item3;
                
                Image<L8>? glossImage = null;
                var specularAlphaValid = false;
                
                if (specularImage is not null) {
                    specularAlphaValid = specularRaw!.Item4.PixelFormat.Alpha != AlphaType.None;

                    if (useGlossInSpecularMap) {
                        specularImage = specularImage.Clone();
                        glossImage = new(specularImage.Width, specularImage.Height);
                        specularImage.ProcessPixelRows(
                            glossImage,
                            (specular, gloss) => {
                                for (var y = 0; y < specular.Height; y++) {
                                    var specularRow = specular.GetRowSpan(y);
                                    var glossRow = gloss.GetRowSpan(y);
                                    for (var x = 0; x < specular.Width; x++) {
                                        glossRow[x].PackedValue = specularRow[x].A;
                                        specularRow[x].A = 255;
                                    }
                                }
                            });
                        
                        specularAlphaValid = false;
                        _gltf.AddTexture(
                            $"{cryMaterial.Name}_gloss_spec.png",
                            glossImage,
                            PngColorType.Grayscale);
                    }
                }

                if (diffuseImage is not null) {
                    specularImage ??= diffuseImage.Clone();
                    if (diffuseRaw!.Item4.PixelFormat.Alpha != AlphaType.None) {
                        if (useSpecAlphaInDiffuseMap && useGlossInSpecularMap) {
                            specularImage.ProcessPixelRows(
                                diffuseImage,
                                (specular, diffuse) => {
                                    for (var y = 0; y < specular.Height; y++) {
                                        var specularRow = specular.GetRowSpan(y);
                                        var diffuseRow = diffuse.GetRowSpan(y);
                                        for (var x = 0; x < specular.Width; x++)
                                            specularRow[x].A = diffuseRow[x].A;
                                    }
                                });
                            specularAlphaValid = true;
                        }
                        
                        if (cryMaterial.AlphaTest == 0) {
                            diffuseImage = diffuseImage.Clone();
                            diffuseImage.ProcessPixelRows(
                                image => {
                                    for (var y = 0; y < image.Height; y++) {
                                        var span = image.GetRowSpan(y);
                                        for (var x = 0; x < image.Width; x++)
                                            span[x].A = 255;
                                    }
                                });
                        }
                    }
                }

                if (normalRaw is not null) {
                    if (useScatterInNormalMap || useHeightInNormalMap) {
                        normalImage = normalImage!.Clone();
                        using var redMap = new Image<L8>(normalImage.Width, normalImage.Height);
                        if (glossImage is null
                            || glossImage.Width != normalImage.Width
                            || glossImage.Height != normalImage.Height) {
                            glossImage?.Dispose();
                            glossImage = new(normalImage.Width, normalImage.Height);
                        }

                        normalImage.ProcessPixelRows(
                            redMap,
                            glossImage,
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
                        _gltf.AddTexture(
                            $"{cryMaterial.Name}_gloss_nrm.png",
                            glossImage,
                            PngColorType.Grayscale);
                        if (useScatterInNormalMap)
                            _gltf.AddTexture(
                                $"{cryMaterial.Name}_scatter_nrm.png",
                                redMap,
                                PngColorType.Grayscale);
                        else if (useHeightInNormalMap)
                            _gltf.AddTexture(
                                $"{cryMaterial.Name}_height_nrm.png",
                                redMap,
                                PngColorType.Grayscale);
                        else
                            throw new InvalidOperationException();
                    }
                }

                GltfTextureInfo? diffuseTextureInfo;
                if (diffuseImage is null)
                    diffuseTextureInfo = null;
                else if (diffuseImage == diffuseRaw?.Item3)
                    diffuseTextureInfo = new() {Index = diffuseRaw.Item1};
                else
                    diffuseTextureInfo = new() {
                        Index = _gltf.AddTexture(
                            $"{name}_dif_gltf.png",
                            diffuseImage,
                            cryMaterial.AlphaTest == 0 ? PngColorType.Rgb : PngColorType.RgbWithAlpha)
                    };

                GltfTextureInfo? specularTextureInfo;
                if (specularImage is null)
                    specularTextureInfo = null;
                else if (specularImage == diffuseImage)
                    specularTextureInfo = diffuseTextureInfo;
                else if (specularImage == specularRaw?.Item3)
                    specularTextureInfo = new() {Index = specularRaw.Item1};
                else
                    specularTextureInfo = new() {
                        Index = _gltf.AddTexture(
                            $"{name}_spec_gltf.png",
                            specularImage,
                            specularAlphaValid ? PngColorType.RgbWithAlpha : PngColorType.Rgb),
                    };

                GltfTextureInfo? normalTextureInfo;
                if (normalImage is null)
                    normalTextureInfo = null;
                else if (normalImage == normalRaw?.Item3)
                    normalTextureInfo = new() {Index = normalRaw.Item1};
                else
                    normalTextureInfo = new() {
                        Index = _gltf.AddTexture($"{name}_nrm_gltf.png", normalImage, PngColorType.Rgb),
                    };

                GltfTextureInfo? specularGlossinessTextureInfo;
                if (specularImage is null || glossImage is null)
                    specularGlossinessTextureInfo = null;
                else {
                    using var im = specularImage.Clone();
                    im.ProcessPixelRows(
                        glossImage,
                        (specular, gloss) => {
                            for (var y = 0; y < specular.Height; y++) {
                                var specularRow = specular.GetRowSpan(y);
                                var glossRow = gloss.GetRowSpan(y);
                                for (var x = 0; x < specular.Width; x++)
                                    specularRow[x].A = glossRow[x].PackedValue;
                            }
                        });
                    specularGlossinessTextureInfo = new() {
                        Index = _gltf.AddTexture($"{name}_sg_gltf.png", im, PngColorType.RgbWithAlpha),
                    };
                }

                GltfTextureInfo? metallicRoughnessTextureInfo;
                if (glossImage is null)
                    metallicRoughnessTextureInfo = null;
                else {
                    using var metallicRoughnessTexture = new Image<Bgra32>(
                        glossImage.Width,
                        glossImage.Height,
                        new(255, 255, 0, 0));
                    metallicRoughnessTexture.ProcessPixelRows(
                        glossImage,
                        (mr, gloss) => {
                            for (var y = 0; y < mr.Height; y++) {
                                var mrRow = mr.GetRowSpan(y);
                                var glossRow = gloss.GetRowSpan(y);
                                // Roughness = 1 - Glossiness
                                for (var x = 0; x < mr.Width; x++)
                                    mrRow[x].G = (byte) (255 - glossRow[x].PackedValue);
                            }
                        });
                    metallicRoughnessTextureInfo = new() {
                        Index = _gltf.AddTexture($"{name}_mr_gltf.png", metallicRoughnessTexture, PngColorType.Rgb),
                    };
                }

                _materialNameToIndex[name] = _gltf.Root.Materials.AddAndGetIndex(
                    new() {
                        Name = name,
                        AlphaCutoff = cryMaterial.AlphaTest == 0 ? null : cryMaterial.AlphaTest,
                        AlphaMode = cryMaterial.AlphaTest == 0
                            ? GltfMaterialAlphaMode.Opaque
                            : GltfMaterialAlphaMode.Mask,
                        DoubleSided = cryMaterial.MaterialFlags.HasFlag(MaterialFlags.TwoSided),
                        NormalTexture = normalTextureInfo,
                        PbrMetallicRoughness = new() {
                            BaseColorTexture = diffuseTextureInfo,
                            BaseColorFactor = new[] {
                                (cryMaterial.DiffuseColor ?? Vector3.One).X,
                                (cryMaterial.DiffuseColor ?? Vector3.One).Y,
                                (cryMaterial.DiffuseColor ?? Vector3.One).Z,
                                float.Clamp(cryMaterial.Opacity, 0f, 1f),
                            },
                            MetallicFactor = float.Clamp(cryMaterial.PublicParams?.Metalness ?? 1, 0, 1),
                            RoughnessFactor = float.Clamp((255 - cryMaterial.Shininess) / 255f, 0, 1),
                            MetallicRoughnessTexture = metallicRoughnessTextureInfo,
                        },
                        EmissiveFactor = new[] {
                            float.Clamp(cryMaterial.GlowAmount, 0f, 1f),
                            float.Clamp(cryMaterial.GlowAmount, 0f, 1f),
                            float.Clamp(cryMaterial.GlowAmount, 0f, 1f)
                        },
                        EmissiveTexture = diffuseTextureInfo,
                        Extensions = new() {
                            KhrMaterialsSpecular = new() {
                                SpecularTexture = specularTextureInfo,
                                SpecularFactor = float.Clamp(cryMaterial.Shininess / 255f, 0f, 1f),
                                SpecularColorFactor = new[] {
                                    (cryMaterial.SpecularColor ?? Vector3.One).X,
                                    (cryMaterial.SpecularColor ?? Vector3.One).Y,
                                    (cryMaterial.SpecularColor ?? Vector3.One).Z,
                                },
                                SpecularColorTexture = specularTextureInfo,
                            },
                            KhrMaterialsPbrSpecularGlossiness = new() {
                                DiffuseFactor = new[] {
                                    (cryMaterial.DiffuseColor ?? Vector3.One).X,
                                    (cryMaterial.DiffuseColor ?? Vector3.One).Y,
                                    (cryMaterial.DiffuseColor ?? Vector3.One).Z,
                                    float.Clamp(cryMaterial.Opacity, 0f, 1f),
                                },
                                DiffuseTexture = diffuseTextureInfo,
                                SpecularFactor = new[] {
                                    (cryMaterial.SpecularColor ?? Vector3.One).X,
                                    (cryMaterial.SpecularColor ?? Vector3.One).Y,
                                    (cryMaterial.SpecularColor ?? Vector3.One).Z,
                                },
                                GlossinessFactor = float.Clamp(cryMaterial.Shininess / 255f, 0f, 1f),
                                SpecularGlossinessTexture = specularGlossinessTextureInfo,
                            },
                            KhrMaterialsEmissiveStrength = cryMaterial.GlowAmount <= 0
                                ? null
                                : new() {
                                    EmissiveStrength = cryMaterial.GlowAmount,
                                },
                        },
                    });
            }
        }

        private void Step03WriteMeshes() {
            var mesh = new GltfMesh();
            foreach (var cryMesh in _character.Model.Meshes) {
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
                            Color0 = _gltf.AddAccessor(
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
                        Material = _materialNameToIndex[cryMesh.MaterialName],
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

            var names = StripCommonParentPaths(animations.Keys.Select(x => Path.ChangeExtension(x, null)).ToList());
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
