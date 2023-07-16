using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using BCnEncoder.Decoder;
using BCnEncoder.ImageSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;
using SynergyLib.FileFormat.CryEngine.CryXml;
using SynergyLib.FileFormat.CryEngine.CryXml.CharacterDefinitionElements;
using SynergyLib.FileFormat.CryEngine.CryXml.MaterialElements;
using SynergyLib.FileFormat.GltfInterop;
using SynergyLib.FileFormat.GltfInterop.Models;
using SynergyLib.Util;
using SynergyLib.Util.MathExtras;

namespace SynergyLib.FileFormat.CryEngine;

public class CryCharacter {
    public CharacterDefinition? Definition;
    public CryModel Model;
    public CharacterParameters? CharacterParameters;
    public CryAnimationDatabase? CryAnimationDatabase;
    public List<CryModel> Attachments = new();
    
    public CryCharacter(CryModel model) {
        Model = model;
    }

    public static CryCharacter FromCryEngineFiles(Func<string, Stream> streamOpener, string baseName) {
        CharacterDefinition? definition = null;
        CryModel model;
        try {
            definition = PbxmlFile.FromStream(streamOpener($"{baseName}.cdf")).DeserializeAs<CharacterDefinition>();
            if (definition.Model is null)
                throw new InvalidDataException("Definition.Model should not be null");
            if (definition.Model.File is null)
                throw new InvalidDataException("Definition.Model.File should not be null");
            if (definition.Model.Material is null)
                throw new InvalidDataException("Definition.Model.Material should not be null");

            model = CryModel.FromCryEngineFiles(
                streamOpener(definition.Model.File),
                streamOpener(definition.Model.Material));
        } catch (FileNotFoundException) {
            model = CryModel.FromCryEngineFiles(streamOpener($"{baseName}.chr"), streamOpener($"{baseName}.mtl"));
        }
        
        var res = new CryCharacter(model) {
            Definition = definition
        };
        if (definition is not null) {
            foreach (var d in (IEnumerable<Attachment>?) definition.Attachments ?? Array.Empty<Attachment>()) {
                if (d.Binding is null)
                    throw new InvalidDataException("Attachment.Binding should not be null");
                if (d.Material is null)
                    throw new InvalidDataException("Attachment.Material should not be null");
                res.Attachments.Add(CryModel.FromCryEngineFiles(streamOpener(d.Binding), streamOpener(d.Material)));
            }
        }

        try {
            res.CharacterParameters = PbxmlFile.FromStream(streamOpener($"{baseName}.chrparams"))
                .DeserializeAs<CharacterParameters>();
            if (res.CharacterParameters.TracksDatabasePath is not null)
                res.CryAnimationDatabase = new(streamOpener(res.CharacterParameters.TracksDatabasePath));
        } catch (FileNotFoundException) { }

        return res;
    }

    public GltfTuple ToGltf(Func<string, Stream> streamOpener) {
        var res = new GltfTuple();
        res.Root.ExtensionsUsed.Add("KHR_materials_specular");
        res.Root.ExtensionsUsed.Add("KHR_materials_pbrSpecularGlossiness");
        res.Root.ExtensionsUsed.Add("KHR_materials_emissive_strength");

        var rootNode = new GltfNode {
            Name = Model.Name,
            Skin = Model.Controllers.Any() ? res.Root.Skins.AddAndGetIndex(new() {Joints = new()}) : null
        };
        var controllerIdToNodeIndex = new Dictionary<uint, int>(Model.Controllers.Count);
        if (rootNode.Skin is { } skinIndex) {
            var controllersSorted = Model.Controllers.OrderBy(x => x.Depth).ToArray();
            var controllerIdToBindPoseMatrices = new Dictionary<uint, Matrix4x4>(controllersSorted.Length);
            foreach (var i in Enumerable.Range(0, controllersSorted.Length)) {
                ref var controller = ref controllersSorted[i];

                var bpm = controllerIdToBindPoseMatrices[controller.Id] = controller.BindPoseMatrix.Normalize();
                if (controller.Parent is not null) {
                    if (!Matrix4x4.Invert(controllerIdToBindPoseMatrices[controller.Parent.Id], out var parentMat))
                        throw new InvalidDataException();

                    bpm = parentMat * bpm;
                }

                if (!Matrix4x4.Invert(bpm, out var ibpm))
                    throw new InvalidDataException();

                if (!Matrix4x4.Decompose(ibpm, out var scale, out var rotation, out var translation))
                    throw new InvalidDataException();

                var nodeIndex = res.Root.Nodes.AddAndGetIndex(
                    new() {
                        Name = controller.Name,
                        Children = new(controller.Children.Count),
                        Translation = SwapAxes(translation).ToFloatList(Vector3.Zero, 1e-6f),
                        Rotation = SwapAxes(rotation).ToFloatList(Quaternion.Identity, 1e-6f),
                        Scale = SwapAxesScale(scale).ToFloatList(Vector3.One, 1e-6f),
                    });
                res.Root.Skins[skinIndex].Joints.Add(nodeIndex);
                controllerIdToNodeIndex.Add(controller.Id, nodeIndex);

                if (controller.Parent is null)
                    rootNode.Children.Add(nodeIndex);
                else
                    res.Root.Nodes[controllerIdToNodeIndex[controller.Parent.Id]].Children.Add(nodeIndex);
            }

            res.Root.Skins[skinIndex].InverseBindMatrices = res.AddAccessor(
                null,
                controllersSorted.Select(x => SwapAxes(x.BindPoseMatrix).Normalize()).ToArray().AsSpan());
        }

        var fullIndices = new ushort[Model.Meshes.Sum(x => x.Indices.Length)];
        for (int i = 0, baseIndex = 0, baseVertex = 0; i < Model.Meshes.Count; i++) {
            for (var j = 0; j < Model.Meshes[i].Indices.Length; j++)
                fullIndices[baseIndex + j] = (ushort) (baseVertex + Model.Meshes[i].Indices[j]);

            baseIndex += Model.Meshes[i].Indices.Length;
            baseVertex += Model.Meshes[i].Vertices.Length;
        }

        var mesh = new GltfMesh();
        var decoder = new BcDecoder();
        foreach (var cryMesh in Model.Meshes) {
            var cryMaterial = Model.Material.SubMaterials!.Single(x => x.Name == cryMesh.MaterialName);

            Image<Rgba32>? diffuseRaw = null;
            Image<Rgba32>? normalRaw = null;
            Image<Rgba32>? specularRaw = null;

            Image<Rgba32>? diffuseTexture = null;
            Image<Rgba32>? normalTexture = null;
            Image<Rgba32>? specularTexture = null;
            Image<Rgba32>? metallicRoughnessTexture = null;
            Image<Rgba32>? specularGlossinessTexture = null;

            foreach (var texture in (IEnumerable<Texture>?) cryMaterial.Textures ?? Array.Empty<Texture>()) {
                if (texture.File is null)
                    continue;

                switch (texture.Map) {
                    case Texture.MapTypeEnum.Diffuse:
                        diffuseRaw =
                            decoder.DecodeToImageRgba32(streamOpener(Path.ChangeExtension(texture.File, ".dds")));
                        break;
                    case Texture.MapTypeEnum.Normals:
                        normalRaw = decoder.DecodeToImageRgba32(
                            streamOpener(Path.ChangeExtension(texture.File, ".dds")));
                        break;
                    case Texture.MapTypeEnum.Specular:
                        specularRaw =
                            decoder.DecodeToImageRgba32(streamOpener(Path.ChangeExtension(texture.File, ".dds")));
                        break;
                }
            }

            if (diffuseRaw is not null) {
                diffuseTexture = new(diffuseRaw.Width, diffuseRaw.Height);
                diffuseTexture.ProcessPixelRows(
                    diffuseRaw,
                    (dst, src) => {
                        for (var y = 0; y < dst.Height; y++) {
                            var dstSpan = dst.GetRowSpan(y);
                            var srcSpan = src.GetRowSpan(y);
                            for (var x = 0; x < dst.Width; x++)
                                dstSpan[x] = new(srcSpan[x].R, srcSpan[x].G, srcSpan[x].B);
                        }
                    });
            }

            if (specularRaw is not null && cryMaterial.GenMaskIllum.HasFlag(BrbGenMask.GlossMap)) {
                specularGlossinessTexture = specularTexture = new(specularRaw.Width, specularRaw.Height);
                specularTexture.ProcessPixelRows(
                    specularRaw,
                    (dst, src) => {
                        for (var y = 0; y < dst.Height; y++) {
                            var dstSpan = dst.GetRowSpan(y);
                            var srcSpan = src.GetRowSpan(y);
                            for (var x = 0; x < dst.Width; x++)
                                dstSpan[x] = new(srcSpan[x].R, srcSpan[x].G, srcSpan[x].B);
                        }
                    });

                var alphaAsGlossiness = cryMaterial.GenMaskIllum.HasFlag(BrbGenMask.GlossinessInSpecularAlpha);
                specularGlossinessTexture.ProcessPixelRows(
                    specularRaw,
                    (dst, src) => {
                        for (var y = 0; y < dst.Height; y++) {
                            var dstSpan = dst.GetRowSpan(y);
                            var srcSpan = src.GetRowSpan(y);
                            if (alphaAsGlossiness) {
                                for (var x = 0; x < dst.Width; x++)
                                    dstSpan[x] = new(srcSpan[x].R, srcSpan[x].G, srcSpan[x].B, dstSpan[x].A);
                            } else {
                                for (var x = 0; x < dst.Width; x++)
                                    dstSpan[x] = new(srcSpan[x].R, srcSpan[x].G, srcSpan[x].B);
                            }
                        }
                    });

                if (alphaAsGlossiness) {
                    metallicRoughnessTexture = new(specularRaw.Width, specularRaw.Height);
                    metallicRoughnessTexture.ProcessPixelRows(
                        specularRaw,
                        (dst, src) => {
                            for (var y = 0; y < dst.Height; y++) {
                                var dstSpan = dst.GetRowSpan(y);
                                var srcSpan = src.GetRowSpan(y);
                                for (var x = 0; x < dst.Width; x++) {
                                    dstSpan[x].G = (byte) (255 - srcSpan[x].A);
                                    dstSpan[x].B = srcSpan[x].A;
                                }
                            }
                        });
                }
            }

            if (normalRaw is not null && cryMaterial.GenMaskIllum.HasFlag(BrbGenMask.BumpMap)) {
                normalTexture = new(normalRaw.Width, normalRaw.Height);
                if (cryMaterial.GenMaskIllum.HasFlag(BrbGenMask.TempSkin)) {
                    normalTexture.ProcessPixelRows(
                        normalRaw,
                        (dstNormal, src) => {
                            for (var y = 0; y < dstNormal.Height; y++) {
                                var dstNormalSpan = dstNormal.GetRowSpan(y);
                                var srcSpan = src.GetRowSpan(y);
                                for (var x = 0; x < dstNormal.Width; x++) {
                                    var nx = srcSpan[x].G / 255f;
                                    var ny = srcSpan[x].A / 255f;
                                    var nz = MathF.Sqrt(1 - MathF.Pow(nx * 2 - 1, 2) - MathF.Pow(ny * 2 - 1, 2)) / 2 +
                                        0.5f;
                                    dstNormalSpan[x] = new(nx, ny, nz);
                                }
                            }
                        });

                    specularGlossinessTexture?.ProcessPixelRows(
                        normalRaw,
                        (dstSpecGlos, src) => {
                            for (var y = 0; y < dstSpecGlos.Height; y++) {
                                var dstSpecGlosSpan = dstSpecGlos.GetRowSpan(y);
                                var srcSpan = src.GetRowSpan(y);
                                for (var x = 0; x < dstSpecGlos.Width; x++)
                                    dstSpecGlosSpan[x].A = srcSpan[x].B;
                            }
                        });

                    // TODO: r contains scatter map
                } else {
                    normalTexture.ProcessPixelRows(
                        normalRaw,
                        (dst, src) => {
                            for (var y = 0; y < dst.Height; y++) {
                                var dstSpan = dst.GetRowSpan(y);
                                var srcSpan = src.GetRowSpan(y);
                                for (var x = 0; x < dst.Width; x++)
                                    dstSpan[x] = new(srcSpan[x].R, srcSpan[x].G, srcSpan[x].B);
                            }
                        });
                }
            }

            var normalTextureInfo = normalTexture is null
                ? null
                : new GltfTextureInfo {Index = res.AddTexture($"{cryMaterial.Name}_normal.png", normalTexture)};
            var diffuseTextureInfo = diffuseTexture is null
                ? null
                : new GltfTextureInfo {Index = res.AddTexture($"{cryMaterial.Name}_diffuse.png", diffuseTexture)};
            var specularTextureInfo = specularTexture is null
                ? null
                : new GltfTextureInfo {Index = res.AddTexture($"{cryMaterial.Name}_specular.png", specularTexture)};
            var metallicRoughnessTextureInfo = metallicRoughnessTexture is null
                ? null
                : new GltfTextureInfo
                    {Index = res.AddTexture($"{cryMaterial.Name}_metallic_roughness.png", metallicRoughnessTexture)};
            var specularGlossinessTextureInfo = specularGlossinessTexture is null
                ? null
                : new GltfTextureInfo
                    {Index = res.AddTexture($"{cryMaterial.Name}_specular_glossiness.png", specularGlossinessTexture)};

            mesh.Primitives.Add(
                new() {
                    Attributes = new() {
                        Position = res.AddAccessor(
                            null,
                            cryMesh.Vertices.Select(x => SwapAxes(x.Position)).ToArray().AsSpan(),
                            target: GltfBufferViewTarget.ArrayBuffer),
                        Normal = res.AddAccessor(
                            null,
                            cryMesh.Vertices.Select(x => SwapAxes(x.Normal)).ToArray().AsSpan(),
                            target: GltfBufferViewTarget.ArrayBuffer),
                        Tangent = res.AddAccessor(
                            null,
                            cryMesh.Vertices.Select(x => SwapAxesTangent(x.Tangent.Tangent)).ToArray().AsSpan(),
                            target: GltfBufferViewTarget.ArrayBuffer),
                        Color0 = res.AddAccessor(
                            null,
                            cryMesh.Vertices.Select(x => new Vector4<float>(x.Color.Select(y => y / 255f))).ToArray()
                                .AsSpan(),
                            target: GltfBufferViewTarget.ArrayBuffer),
                        TexCoord0 = res.AddAccessor(
                            null,
                            cryMesh.Vertices.Select(x => x.TexCoord).ToArray().AsSpan(),
                            target: GltfBufferViewTarget.ArrayBuffer),
                        Weights0 = controllerIdToNodeIndex is null
                            ? null
                            : res.AddAccessor(
                                null,
                                cryMesh.Vertices.Select(x => x.Weights).ToArray().AsSpan(),
                                target: GltfBufferViewTarget.ArrayBuffer),
                        Joints0 = controllerIdToNodeIndex is null
                            ? null
                            : res.AddAccessor(
                                null,
                                cryMesh.Vertices.Select(
                                        x => new Vector4<ushort>(
                                            x.ControllerIds.Select(
                                                y => (ushort) controllerIdToNodeIndex.GetValueOrDefault(y))))
                                    .ToArray()
                                    .AsSpan(),
                                target: GltfBufferViewTarget.ArrayBuffer),
                    },
                    Indices = res.AddAccessor(
                        null,
                        cryMesh.Indices.AsSpan(),
                        target: GltfBufferViewTarget.ElementArrayBuffer),
                    Material = res.Root.Materials.AddAndGetIndex(
                        new() {
                            // Root.Materials.Select((x, i) => (x, i)).Single(x => x.x.Name == cryMesh.MaterialName).i
                            Name = cryMesh.MaterialName,
                            AlphaCutoff = cryMaterial.AlphaTest,
                            AlphaMode = cryMaterial.Opacity < 1f
                                ? GltfMaterialAlphaMode.Blend
                                : cryMaterial.AlphaTest == 0
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
                                MetallicFactor = 0f,
                                RoughnessFactor = 1f,
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
                                KhrMaterialsEmissiveStrength = new() {
                                    EmissiveStrength = cryMaterial.GlowAmount,
                                },
                            },
                        }),
                });
        }

        rootNode.Mesh = res.Root.Meshes.AddAndGetIndex(mesh);

        if (controllerIdToNodeIndex is not null
            && CryAnimationDatabase?.Animations is { } animations
            && animations.Any()) {
            var positionAccessors = animations.Values.SelectMany(x => x.Tracks.Values.Select(y => y.Position))
                .Where(x => x is not null)
                .Distinct()
                .ToDictionary(x => x!, x => res.AddAccessor(null, x!.Data.Select(SwapAxes).ToArray().AsSpan()));

            var rotationAccessors = animations.Values.SelectMany(x => x.Tracks.Values.Select(y => y.Rotation))
                .Where(x => x is not null)
                .Distinct()
                .ToDictionary(x => x!, x => res.AddAccessor(null, x!.Select(SwapAxes).ToArray().AsSpan()));

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

                    timeAccessor = res.AddAccessor(
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
                                    Node = controllerIdToNodeIndex[controllerId],
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
                                    Node = controllerIdToNodeIndex[controllerId],
                                    Path = GltfAnimationChannelTargetPath.Rotation,
                                },
                            });
                }

                if (!target.Channels.Any() || !target.Samplers.Any())
                    continue;

                res.Root.Animations.Add(target);
            }
        }

        res.AddToScene(res.Root.Nodes.AddAndGetIndex(rootNode));
        return res;
    }

    private static List<string> StripCommonParentPaths(ICollection<string> fullNames) {
        var namesDepths = new List<List<string>>();

        var names = new List<string>();
        for (var i = 0; i < fullNames.Count; i++) {
            var nameDepth = 0;
            for (; nameDepth < namesDepths.Count; nameDepth++) {
                if (namesDepths[nameDepth].Count(x => x == namesDepths[nameDepth][i]) < 2)
                    break;
            }

            if (nameDepth == namesDepths.Count)
                namesDepths.Add(fullNames.Select(x => string.Join('/', x.Split('/').TakeLast(nameDepth + 1))).ToList());

            names.Add(namesDepths[nameDepth][i]);
        }

        return names;
    }

    protected static Vector3 SwapAxes(Vector3 val) => new(-val.X, val.Z, val.Y);
    protected static Vector3 SwapAxesScale(Vector3 val) => new(val.X, val.Z, val.Y);
    protected static Vector4 SwapAxesTangent(Vector4 val) => new(-val.X, val.Z, val.Y, val.W);
    protected static Quaternion SwapAxes(Quaternion val) => new(-val.X, val.Z, val.Y, val.W);

    protected static Matrix4x4 SwapAxes(Matrix4x4 val) => new(
        val.M11,
        -val.M13,
        -val.M12,
        -val.M14,
        -val.M31,
        val.M33,
        val.M32,
        val.M34,
        -val.M21,
        val.M23,
        val.M22,
        val.M24,
        -val.M41,
        val.M43,
        val.M42,
        val.M44);
}
