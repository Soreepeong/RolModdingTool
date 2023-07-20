using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using BCnEncoder.Encoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SynergyLib.FileFormat.CryEngine.CryAnimationDatabaseElements;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Enums;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;
using SynergyLib.FileFormat.CryEngine.CryModelElements;
using SynergyLib.FileFormat.CryEngine.CryXml;
using SynergyLib.FileFormat.CryEngine.CryXml.MaterialElements;
using SynergyLib.FileFormat.GltfInterop;
using SynergyLib.FileFormat.GltfInterop.Models;
using SynergyLib.Util;
using SynergyLib.Util.MathExtras;

namespace SynergyLib.FileFormat.CryEngine;

public partial class CryCharacter {
    public class GltfImporter {
        private readonly GltfTuple _gltf;
        private readonly CancellationToken _cancellationToken;
        private readonly GltfNode _rootNode;

        private readonly Dictionary<int, uint> _boneIdToControllerId = new();
        private readonly Dictionary<int, uint> _nodeIdToControllerId = new();

        private readonly CryModel _model = new();
        private CryAnimationDatabase? _animations;

        public GltfImporter(GltfTuple gltf, CancellationToken cancellationToken) {
            _gltf = gltf;
            _cancellationToken = cancellationToken;
            _rootNode = _gltf.Root.Nodes[_gltf.Root.Scenes[_gltf.Root.Scene].Nodes.Single()];
        }

        public CryCharacter Process() {
            Step01ReadSkin();
            Step02GenerateMaterials();
            Step03GenerateMesh();
            Step04ImportAnimations();

            return new(_model) {CryAnimationDatabase = _animations};
        }

        private void Step01ReadSkin() {
            if (_rootNode.Skin is null)
                return;

            if (_gltf.Root.Skins.Count <= _rootNode.Skin.Value || _rootNode.Skin.Value < 0)
                throw new InvalidDataException("RootNode.Skin is out of range");

            if (_gltf.Root.Skins[_rootNode.Skin.Value].Joints is not { } joints)
                throw new InvalidDataException("skin.Joints is null");

            var pendingTraversal = _rootNode.Children.Select(x => Tuple.Create(x, (Controller?) null)).ToList();
            if (pendingTraversal.Count != 1)
                throw new NotSupportedException("Currently a root node with 2+ child nodes are unsupported.");
            while (pendingTraversal.Any()) {
                _cancellationToken.ThrowIfCancellationRequested();

                var (nodeIndex, parentController) = pendingTraversal.First();
                pendingTraversal.RemoveAt(0);

                var boneIndex = joints.IndexOf(nodeIndex);
                if (boneIndex == -1)
                    throw new InvalidDataException();

                var node = _gltf.Root.Nodes[nodeIndex];
                string controllerName;
                uint controllerId;
                if (node.Name?.IndexOf("$$") is { } sharp && sharp != -1) {
                    controllerName = node.Name[..sharp];
                    controllerId = Convert.ToUInt32(node.Name[(sharp + 2)..], 16);
                } else {
                    controllerName = node.Name ?? $"Bone_{boneIndex}";
                    controllerId = Crc32.CryE.Get(controllerName);
                }

                var matrix = Matrix4x4.Identity;
                if (node.Scale is not null)
                    matrix *= Matrix4x4.CreateScale(SwapAxesScale(node.Scale.ToVector3()));
                if (node.Rotation is not null)
                    matrix *= Matrix4x4.CreateFromQuaternion(SwapAxes(node.Rotation.ToQuaternion()));
                if (node.Translation is not null)
                    matrix *= Matrix4x4.CreateTranslation(SwapAxes(node.Translation.ToVector3()));

                var controller = new Controller(controllerId, controllerName) {
                    Decomposed = Tuple.Create(
                        node.Translation is null ? Vector3.Zero : SwapAxes(node.Translation.ToVector3()),
                        node.Rotation is null ? Quaternion.Identity : SwapAxes(node.Rotation.ToQuaternion())),
                };
                if (parentController is not null)
                    parentController.Children.Add(controller);
                else if (_model.RootController is null)
                    _model.RootController = controller;
                else
                    throw new InvalidDataException("Multiple roots are not supported.");

                _nodeIdToControllerId[nodeIndex] = _boneIdToControllerId[boneIndex] = controller.Id;
                pendingTraversal.AddRange(node.Children.Select(child => Tuple.Create(child, controller))!);
            }
        }

        private bool GetGltfTextureDds(
            GltfTextureInfo? gltfTextureInfo,
            [MaybeNullWhen(false)] out DirectDrawSurface.DdsFile ddsFile) {
            ddsFile = null!;

            _cancellationToken.ThrowIfCancellationRequested();

            if (gltfTextureInfo is null)
                return false;

            if (gltfTextureInfo.Index is not { } textureInfoIndex
                || textureInfoIndex < 0
                || textureInfoIndex >= _gltf.Root.Textures.Count)
                throw new InvalidDataException("TextureInfo does not point to a Texture");

            var gltfTexture = _gltf.Root.Textures[textureInfoIndex];
            if (gltfTexture.Extensions?.MsftTextureDds?.Source is not { } imageIndex
                || imageIndex < 0
                || imageIndex >= _gltf.Root.Images.Count)
                throw new InvalidDataException("Texture does not point to a DDS image");

            var gltfImage = _gltf.Root.Images[imageIndex];
            if (gltfImage.BufferView is not { } bufferViewIndex
                || bufferViewIndex < 0
                || bufferViewIndex >= _gltf.Root.BufferViews.Count)
                throw new InvalidDataException("Image does not point to a BufferView");

            ddsFile = new(gltfImage.Name!, _gltf.ReadBufferView(bufferViewIndex));
            return true;
        }

        private bool GetGltfTexture<TPixel>(
            GltfTextureInfo? gltfTextureInfo,
            [MaybeNullWhen(false)] out Image<TPixel> image)
            where TPixel : unmanaged, IPixel<TPixel> {
            image = null!;

            _cancellationToken.ThrowIfCancellationRequested();

            if (gltfTextureInfo is null)
                return false;

            if (gltfTextureInfo.Index is not { } textureInfoIndex
                || textureInfoIndex < 0
                || textureInfoIndex >= _gltf.Root.Textures.Count)
                throw new InvalidDataException("TextureInfo does not point to a Texture");

            var gltfTexture = _gltf.Root.Textures[textureInfoIndex];
            if (gltfTexture.Source is not { } imageIndex
                || imageIndex < 0
                || imageIndex >= _gltf.Root.Images.Count)
                throw new InvalidDataException("Texture does not point to an Image");

            var gltfImage = _gltf.Root.Images[imageIndex];
            if (gltfImage.BufferView is not { } bufferViewIndex
                || bufferViewIndex < 0
                || bufferViewIndex >= _gltf.Root.BufferViews.Count)
                throw new InvalidDataException("Image does not point to a BufferView");

            image = Image.Load<TPixel>(_gltf.ReadBufferView(bufferViewIndex));
            return true;
        }

        private void AddCryTexture(Material material, TextureMapType map, Image<Rgba32> image) {
            _cancellationToken.ThrowIfCancellationRequested();

            var suffix = map switch {
                TextureMapType.Diffuse => "dif",
                TextureMapType.Normals when material.GenMask.UseScatterInNormalMap => "bsg",
                TextureMapType.Normals when material.GenMask.UseHeightInNormalMap => "bhg",
                TextureMapType.Normals => "nrm",
                TextureMapType.Specular => "spec",
                TextureMapType.Env => "env",
                TextureMapType.Detail when material.GenMask.UseNormalMapInDetailMap => "n_detail",
                TextureMapType.Detail => "detail",
                TextureMapType.Opacity when material.GenMask.UseInvertedBlendMap => "invblend",
                TextureMapType.Opacity => "blend",
                TextureMapType.Decal when material.GenMask.UseGlowDecalMap => "glow_decal",
                TextureMapType.Decal => "decal",
                TextureMapType.SubSurface when material.GenMask.UseBlendSpecularInSubSurfaceMap => "blendspec_sss",
                TextureMapType.SubSurface => "sss",
                TextureMapType.Custom when material.GenMask.UseBlendDiffuseInCustomMap => "blenddif_cust",
                TextureMapType.Custom => "cust",
                TextureMapType.Custom2 when material.GenMask.UseDirtLayerInCustomMap2 => "dirt_cust2",
                TextureMapType.Custom2 when material.GenMask.UseAddNormalInCustomMap2 => "addnrm_cust2",
                TextureMapType.Custom2 when material.GenMask.UseBlurRefractionInCustomMap2 => "refraction_cust2",
                TextureMapType.Custom2 => "cust2",
                _ => throw new ArgumentOutOfRangeException(nameof(map), map, null),
            };

            var ms = new MemoryStream();
            var ddsEncoder = new BcEncoder {
                OutputOptions = {
                    GenerateMipMaps = false,
                    Quality = CompressionQuality.BestQuality,
                    Format = CompressionFormat.Bc3,
                    FileFormat = OutputFileFormat.Dds,
                },
            };
            ddsEncoder.EncodeToStream(image, ms);

            var baseName = $"mod/{_rootNode.Name}/{material.Name}_{suffix}";
            ms.Position = 0;
            _model.ExtraTextures[$"{baseName}.dds"] = ms;
            material.Textures ??= new();
            material.Textures.Add(
                new() {
                    File = $"{baseName}.tif",
                    Map = TextureMapType.Normals,
                });
        }

        private void Step02GenerateMaterials() {
            _model.Material = new() {SubMaterialsAndRefs = new()};
            foreach (var material in _gltf.Root.Materials) {
                _cancellationToken.ThrowIfCancellationRequested();

                var materialName = material.Name;
                var cryMaterial = material.Extensions?.SynergyToolsCryMaterial ?? new Material {
                    Name = materialName,
                    AlphaTest = material.AlphaCutoff ?? 0f,
                    Flags = (material.DoubleSided is true ? MaterialFlags.TwoSided : 0) |
                        MaterialFlags.ShaderGenMask64Bit,
                    DiffuseColor = material.PbrMetallicRoughness?.BaseColorFactor?.ToVector3() ?? Vector3.One,
                    SpecularColor =
                        material.Extensions?.KhrMaterialsSpecular?.SpecularColorFactor?.ToVector3() ?? Vector3.One,
                    EmissiveColor = Vector3.Zero,
                    Shininess = MathF.Round(
                        material.Extensions?.KhrMaterialsPbrSpecularGlossiness?.GlossinessFactor * 255 ?? 200),
                    Opacity = material.PbrMetallicRoughness?.BaseColorFactor?[3] ?? 1f,
                    GlowAmount = material.Extensions?.KhrMaterialsEmissiveStrength?.EmissiveStrength ?? 0f,
                    Shader = "Brb_Illum",
                    StringGenMask = string.Empty,
                };
                _model.Material!.SubMaterialsAndRefs!.Add(cryMaterial);

                cryMaterial.Textures ??= new();
                foreach (var c in cryMaterial.Textures) {
                    if (!GetGltfTextureDds(c.GltfTextureInfo, out var dds))
                        continue;

                    c.File ??= $"mod/{_rootNode.Name}/{dds.Name}";
                    _model.ExtraTextures[c.File] = dds.CreateStream();
                }

                GetGltfTexture<Rgba32>(material.NormalTexture, out var normalImage);
                if (!GetGltfTexture<Rgba32>(material.PbrMetallicRoughness?.BaseColorTexture, out var diffuseImage))
                    GetGltfTexture(
                        material.Extensions?.KhrMaterialsPbrSpecularGlossiness?.DiffuseTexture,
                        out diffuseImage);
                GetGltfTexture<Rgba32>(
                    material.PbrMetallicRoughness?.MetallicRoughnessTexture,
                    out var metallicRoughnessImage);
                GetGltfTexture<Rgba32>(
                    material.Extensions?.KhrMaterialsSpecular?.SpecularTexture,
                    out var specularImage);
                GetGltfTexture<Rgba32>(
                    material.Extensions?.KhrMaterialsPbrSpecularGlossiness?.SpecularGlossinessTexture,
                    out var specularGlossImage);

                if (cryMaterial.Textures.Any(x => x.Map == TextureMapType.Normals))
                    cryMaterial.GenMask.UseBumpMap = true;
                else if (normalImage is not null) {
                    cryMaterial.GenMask.UseBumpMap = true;
                    AddCryTexture(cryMaterial, TextureMapType.Normals, normalImage);
                } else
                    cryMaterial.GenMask.UseBumpMap = false;

                if (cryMaterial.Textures.All(x => x.Map != TextureMapType.Diffuse) && diffuseImage is not null)
                    AddCryTexture(cryMaterial, TextureMapType.Diffuse, diffuseImage);

                if (cryMaterial.Textures.Any(x => x.Map == TextureMapType.Specular))
                    cryMaterial.GenMask.UseSpecularMap = true;
                else if (specularGlossImage is not null) {
                    cryMaterial.GenMask.UseSpecularMap = true;
                    cryMaterial.GenMask.UseGlossInSpecularMap = true;
                    AddCryTexture(cryMaterial, TextureMapType.Specular, specularGlossImage);
                } else if (specularImage is not null) {
                    cryMaterial.GenMask.UseSpecularMap = true;
                    cryMaterial.GenMask.UseGlossInSpecularMap = false;
                    AddCryTexture(cryMaterial, TextureMapType.Specular, specularImage);
                } else if (diffuseImage is not null) {
                    cryMaterial.GenMask.UseSpecularMap = true;
                    cryMaterial.GenMask.UseGlossInSpecularMap = false;
                    cryMaterial.Textures.Add(
                        new() {
                            File = cryMaterial.Textures.Single(x => x.Map == TextureMapType.Diffuse).File,
                            Map = TextureMapType.Diffuse,
                        });
                } else {
                    cryMaterial.GenMask.UseSpecularMap = false;
                    cryMaterial.GenMask.UseGlossInSpecularMap = false;
                }
            }
        }

        private void Step03GenerateMesh() {
            if (_rootNode.Mesh is null)
                return;

            var mesh = _gltf.Root.Meshes[_rootNode.Mesh.Value];
            var cryMeshNode = new Node(_rootNode.Name ?? "unnamed", _rootNode.Name) {
                HasColors = _gltf.Root.Meshes.SelectMany(x => x.Primitives).All(x => x.Attributes.Color0 is not null),
            };
            _model.Nodes.Add(cryMeshNode);
            foreach (var materialIndex in Enumerable.Range(-1, 1 + _gltf.Root.Materials.Count)) {
                var primitives = mesh.Primitives
                    .Where(x => (x.Material is null && materialIndex == -1) || x.Material == materialIndex)
                    .ToArray();
                if (!primitives.Any())
                    continue;

                var vertices = new List<Vertex>();
                var indices = new List<ushort>();
                foreach (var primitive in primitives) {
                    if (primitive.Indices is null
                        || primitive.Attributes.Position is null
                        || primitive.Attributes.Normal is null
                        || primitive.Attributes.Tangent is null
                        || primitive.Attributes.TexCoord0 is null)
                        throw new InvalidDataException();

                    var externalIndices = _gltf.ReadUInt16Array(primitive.Indices.Value);
                    var usedIndices = externalIndices.Distinct().Order().Select((x, i) => (x, i))
                        .ToDictionary(x => x.x, x => checked((ushort) (vertices.Count + x.i)));

                    var positions = _gltf.ReadVector3Array(primitive.Attributes.Position.Value);
                    var normals = _gltf.ReadVector3Array(primitive.Attributes.Normal.Value);
                    var tangents = _gltf.ReadVector4Array(primitive.Attributes.Tangent.Value);
                    var texCoords = _gltf.ReadVector2Array(primitive.Attributes.TexCoord0.Value);
                    if (positions.Length != normals.Length
                        || positions.Length != tangents.Length
                        || positions.Length != texCoords.Length)
                        throw new InvalidDataException();

                    Vector4<float>[]? weights = null;
                    Vector4<ushort>[]? joints = null;
                    if (_model.RootController is not null) {
                        if (primitive.Attributes.Weights0 is null
                            || primitive.Attributes.Joints0 is null)
                            throw new InvalidDataException();

                        weights = _gltf.ReadVector4SingleArray(primitive.Attributes.Weights0.Value);
                        joints = _gltf.ReadVector4UInt16Array(primitive.Attributes.Joints0.Value);
                    }

                    indices.EnsureCapacity(indices.Count + externalIndices.Length);
                    indices.AddRange(externalIndices.Select(i => usedIndices[i]));

                    var verticesRangeStart = vertices.Count;
                    vertices.EnsureCapacity(vertices.Count + usedIndices.Count);
                    vertices.AddRange(
                        usedIndices.Keys.Order()
                            .Select(
                                i => new Vertex {
                                    Position = SwapAxes(positions[i]),
                                    Normal = SwapAxes(normals[i]),
                                    Tangent = MeshTangent.FromNormalAndTangent(
                                        SwapAxes(normals[i]),
                                        SwapAxesTangent(tangents[i])),
                                    TexCoord = texCoords[i],
                                    Weights = weights?[i] ?? default(Vector4<float>),
                                    ControllerIds = joints is null
                                        ? default
                                        : new(
                                            joints[i]
                                                .Zip(weights![i])
                                                .Select(x => x.Second > 0 ? _boneIdToControllerId[x.First] : 0)),
                                }));

                    if (primitive.Attributes.Color0 is not null) {
                        var colors = _gltf.ReadVector4Array(primitive.Attributes.Color0.Value);
                        if (positions.Length != colors.Length)
                            throw new InvalidDataException();
                        var verticesSpan = CollectionsMarshal.AsSpan(vertices)[verticesRangeStart..];
                        foreach (var i in usedIndices.Keys.Order()) {
                            verticesSpan[i].Color = new(
                                Enumerable.Range(0, 4).Select(j => (byte) (colors[i][j] * 255f)));
                        }
                    }
                }

                cryMeshNode.Meshes.Add(
                    new(
                        _model.Material?.SubMaterials?.ElementAtOrDefault(materialIndex)?.Name,
                        false,
                        vertices.ToArray(),
                        indices.ToArray()));
            }

            _model.PseudoMaterials.Add(new(cryMeshNode.MaterialName ?? "test") {Flags = MtlNameFlags.MultiMaterial});
            foreach (var m in _model.Material!.SubMaterials!) {
                var npm = new PseudoMaterial(m!.Name!) {Flags = MtlNameFlags.SubMaterial};
                _model.PseudoMaterials[0].Children.Add(npm);
            }
        }

        private void Step04ImportAnimations() {
            _animations = new();
            foreach (var animation in _gltf.Root.Animations) {
                var anim = new Animation {
                    MotionParams = new() {
                        Start = int.MaxValue,
                        End = int.MinValue,
                    }
                };
                foreach (var channel in animation.Channels) {
                    if (channel.Target.Node is not { } nodeIndex)
                        throw new InvalidDataException();

                    var controllerId = _nodeIdToControllerId[nodeIndex];
                    switch (channel.Target.Path) {
                        case GltfAnimationChannelTargetPath.Translation: {
                            var time = _gltf.ReadSingleArray(animation.Samplers[channel.Sampler].Input)
                                .Select(x => 30 * x).ToArray();
                            var pos = _gltf.ReadVector3Array(animation.Samplers[channel.Sampler].Output)
                                .Select(SwapAxes).ToArray();
                            anim.MotionParams.Start = Math.Min(anim.MotionParams.Start, (int) time[0]);
                            anim.MotionParams.End = Math.Max(anim.MotionParams.End, (int) time[^1]);
                            if (!anim.Tracks.TryGetValue(controllerId, out var track))
                                anim.Tracks[controllerId] = track = new();
                            track.Position = ControllerKeyPosition.FromArray(pos);
                            track.PositionTime = ControllerKeyTime.FromArray(time);
                            break;
                        }
                        case GltfAnimationChannelTargetPath.Rotation: {
                            var time = _gltf.ReadSingleArray(animation.Samplers[channel.Sampler].Input)
                                .Select(x => 30 * x).ToArray();
                            var rot = _gltf.ReadQuaternionArray(animation.Samplers[channel.Sampler].Output)
                                .Select(SwapAxes).ToArray();
                            anim.MotionParams.Start = Math.Min(anim.MotionParams.Start, (int) time[0]);
                            anim.MotionParams.End = Math.Max(anim.MotionParams.End, (int) time[^1]);
                            if (!anim.Tracks.TryGetValue(controllerId, out var track))
                                anim.Tracks[controllerId] = track = new();
                            track.Rotation = ControllerKeyRotation.FromArray(rot);
                            track.RotationTime = ControllerKeyTime.FromArray(time);
                            break;
                        }
                    }
                }

                if (anim.Tracks.Any())
                    _animations.Animations[animation.Name ?? $"Animation_{_animations.Animations.Count}"] = anim;
            }
        }
    }
}
