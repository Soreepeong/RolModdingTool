using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SynergyLib.FileFormat.CryEngine.CryAnimationDatabaseElements;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Enums;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;
using SynergyLib.FileFormat.CryEngine.CryModelElements;
using SynergyLib.FileFormat.CryEngine.CryXml;
using SynergyLib.FileFormat.CryEngine.CryXml.MaterialElements;
using SynergyLib.FileFormat.DirectDrawSurface;
using SynergyLib.FileFormat.DotSquish;
using SynergyLib.FileFormat.GltfInterop;
using SynergyLib.FileFormat.GltfInterop.Models;
using SynergyLib.Util;
using SynergyLib.Util.MathExtras;

namespace SynergyLib.FileFormat.CryEngine;

public partial class CryCharacter {
    public class GltfImporter {
        private readonly GltfTuple _gltf;
        private readonly CancellationToken _cancellationToken;

        private readonly Dictionary<int, uint> _boneIdToControllerId = new();
        private readonly Dictionary<int, uint> _nodeIdToControllerId = new();

        private readonly CryModel _model = new();
        private readonly string _name;
        private CryAnimationDatabase? _animations;

        public GltfImporter(GltfTuple gltf, string? name, CancellationToken cancellationToken) {
            _gltf = gltf;
            _cancellationToken = cancellationToken;
            _name = name ?? RootNodes.FirstOrDefault(x => x.Name is not null)?.Name ?? "untitled";
        }

        private IEnumerable<GltfNode> RootNodes =>
            _gltf.Root.Scenes[_gltf.Root.Scene].Nodes.Select(x => _gltf.Root.Nodes[x]);

        public CryCharacter Process() {
            Step01ReadSkin();
            Step02GenerateMaterials();
            Step03GenerateMesh();
            Step04ImportAnimations();

            return new(_model) {CryAnimationDatabase = _animations};
        }

        private void Step01ReadSkin() {
            var skinnedNodes = RootNodes.Where(x => x.Skin.HasValue).ToArray();
            if (skinnedNodes.Length != 1)
                return;

            var skinnedRootNode = skinnedNodes[0];

            if (_gltf.Root.Skins.Count <= skinnedRootNode.Skin!.Value || skinnedRootNode.Skin.Value < 0)
                throw new InvalidDataException("RootNode.Skin is out of range");

            if (_gltf.Root.Skins[skinnedRootNode.Skin.Value].Joints is not { } joints)
                throw new InvalidDataException("skin.Joints is null");

            var pendingTraversal = skinnedRootNode.Children.Select(x => Tuple.Create(x, (Controller?) null)).ToList();
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
            [MaybeNullWhen(false)] out DdsFile ddsFile) {
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

        private void AddCryTexture(
            int materialIndex,
            Material material,
            TextureMapType map,
            Image<Bgra32> image,
            bool useAlpha) {
            _cancellationToken.ThrowIfCancellationRequested();

            var suffix = map switch {
                TextureMapType.Diffuse => "dif",
                TextureMapType.Normals when material.GenMask.UseScatterGlossInNormalMap => "bsg",
                TextureMapType.Normals when material.GenMask.UseHeightGlossInNormalMap => "bhg",
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

            var baseName = $"mod/{_name}/{material.Name ?? materialIndex.ToString()}_{suffix}";
            var counter = 2;
            while (material.Textures?.Any(x => x.File == $"{baseName}.tif") is true) {
                baseName = $"mod/{_name}/{material.Name ?? materialIndex.ToString()}_{counter}_{suffix}";
                counter++;
            }

            var dds = image.ToDdsFile2D(
                baseName,
                new() {
                    Method = useAlpha ? SquishMethod.Dxt5 : SquishMethod.Dxt1,
                    Weights = Vector3.Normalize(new(0.3f, 0.3f, 1f)),
                },
                "CExtCEnd"u8.ToArray(),
                9);
            if (map is TextureMapType.Diffuse or TextureMapType.Specular or TextureMapType.SubSurface
                or TextureMapType.Decal or TextureMapType.Custom) {
                unsafe {
                    fixed (byte* p = dds.Data)
                        ((DdsHeaderLegacy*) p)->Header.SetCryFlags(CryDdsFlags.SrgbRead);
                }
            }

            _model.ExtraTextures[$"{baseName}.dds"] = dds.CreateStream();
            material.Textures ??= new();
            material.Textures.Add(
                new() {
                    File = $"{baseName}.tif",
                    Map = map,
                });
        }

        private void Step02GenerateMaterials() {
            _model.Material = new() {
                Flags = MaterialFlags.ShaderGenMask64Bit | MaterialFlags.MultiSubmtl,
                SubMaterialsAndRefs = new(),
            };
            foreach (var (material, gltfMaterialIndex) in _gltf.Root.Materials.Select((x, i) => (x, i))) {
                _cancellationToken.ThrowIfCancellationRequested();

                float? glossiness = null;
                if (material.PbrMetallicRoughness?.RoughnessFactor is not null)
                    glossiness = 1 - material.PbrMetallicRoughness?.RoughnessFactor.Value;
                glossiness ??= material.Extensions?.KhrMaterialsPbrSpecularGlossiness?.GlossinessFactor;

                var cryMaterial = material.Extensions?.SynergyToolsCryMaterial ?? new Material {
                    Name = material.Name ?? "unnamed_material_" + gltfMaterialIndex,
                    AlphaTest = material.AlphaCutoff ?? 0f,
                    Flags = (material.DoubleSided is true ? MaterialFlags.TwoSided : 0) |
                        MaterialFlags.ShaderGenMask64Bit | MaterialFlags.PureChild,
                    DiffuseColor = material.PbrMetallicRoughness?.BaseColorFactor?.ToVector3() ?? Vector3.One,
                    SpecularColor = material.Extensions?.KhrMaterialsSpecular?.SpecularColorFactor?.ToVector3() ??
                        Vector3.One,
                    EmissiveColor = Vector3.Zero,
                    Shininess = MathF.Round(glossiness * 255 ?? 0),
                    Opacity = material.PbrMetallicRoughness?.BaseColorFactor?[3] ?? 1f,
                    GlowAmount = material.Extensions?.KhrMaterialsEmissiveStrength?.EmissiveStrength ?? 0f,
                    Shader = "Brb_Illum",
                    StringGenMask = string.Empty,
                    PublicParams = new() {
                        SilhouetteColor = Vector3.Zero,
                        SilhouetteIntensity = 0,
                        FresnelPower = 4,
                        FresnelScale = 0,
                        FresnelBias = 0.5f,
                        IndirectColor = Vector3.Zero,
                        WrapColor = Vector3.Zero,
                        // Metalness = material.PbrMetallicRoughness?.MetallicFactor,
                    },
                };
                var cryMaterialIndex = _model.Material!.SubMaterialsAndRefs!.AddAndGetIndex(cryMaterial);

                cryMaterial.Textures ??= new();
                foreach (var c in cryMaterial.Textures) {
                    if (!GetGltfTextureDds(c.GltfTextureInfo, out var dds))
                        continue;

                    c.File ??= $"mod/{_name}/{dds.Name}";
                    _model.ExtraTextures[c.File] = dds.CreateStream();
                }

                GetGltfTexture<Bgra32>(material.NormalTexture, out var normalImage);
                if (!GetGltfTexture<Bgra32>(material.PbrMetallicRoughness?.BaseColorTexture, out var diffuseImage))
                    GetGltfTexture(
                        material.Extensions?.KhrMaterialsPbrSpecularGlossiness?.DiffuseTexture,
                        out diffuseImage);
                GetGltfTexture<Bgra32>(
                    material.PbrMetallicRoughness?.MetallicRoughnessTexture,
                    out var metallicRoughnessImage);
                if (!GetGltfTexture<Bgra32>(
                        material.Extensions?.KhrMaterialsSpecular?.SpecularColorTexture,
                        out var specularImage))
                    GetGltfTexture(
                        material.Extensions?.KhrMaterialsSpecular?.SpecularTexture,
                        out specularImage);
                GetGltfTexture<Bgra32>(
                    material.Extensions?.KhrMaterialsPbrSpecularGlossiness?.SpecularGlossinessTexture,
                    out var specularGlossImage);

                if (cryMaterial.Textures.All(x => x.Map != TextureMapType.Diffuse) && diffuseImage is not null) {
                    AddCryTexture(
                        cryMaterialIndex,
                        cryMaterial,
                        TextureMapType.Diffuse,
                        diffuseImage,
                        material.AlphaMode == GltfMaterialAlphaMode.Mask);
                }

                if (cryMaterial.Textures.Any(x => x.Map == TextureMapType.Specular))
                    cryMaterial.GenMask.UseSpecularMap = true;
                else if (specularGlossImage is not null) {
                    cryMaterial.GenMask.UseSpecularMap = true;
                    cryMaterial.GenMask.UseGlossInSpecularMap = true;
                    AddCryTexture(cryMaterialIndex, cryMaterial, TextureMapType.Specular, specularGlossImage, true);
                } else {
                    specularImage ??= diffuseImage?.Clone();
                    if (specularImage is not null && metallicRoughnessImage is not null) {
                        cryMaterial.GenMask.UseSpecularMap = true;
                        cryMaterial.GenMask.UseGlossInSpecularMap = true;
                        specularGlossImage = specularImage;
                        specularGlossImage.ProcessPixelRows(
                            metallicRoughnessImage,
                            (sg, mr) => {
                                for (var i = 0; i < sg.Height; i++) {
                                    var sgSpan = sg.GetRowSpan(i);
                                    var mrSpan = mr.GetRowSpan(i * mr.Height / sg.Height);
                                    for (var j = 0; j < sgSpan.Length; j++)
                                        sgSpan[j].A = unchecked((byte) (255 - mrSpan[j * mr.Width / sg.Width].G));
                                }
                            });
                        AddCryTexture(cryMaterialIndex, cryMaterial, TextureMapType.Specular, specularGlossImage, true);
                    } else if (specularImage is not null) {
                        cryMaterial.GenMask.UseSpecularMap = true;
                        cryMaterial.GenMask.UseGlossInSpecularMap = false;
                        AddCryTexture(cryMaterialIndex, cryMaterial, TextureMapType.Specular, specularImage, false);
                    } else {
                        cryMaterial.GenMask.UseSpecularMap = false;
                        cryMaterial.GenMask.UseGlossInSpecularMap = false;
                    }
                }

                if (cryMaterial.Textures.Any(x => x.Map == TextureMapType.Normals))
                    cryMaterial.GenMask.UseBumpMap = true;
                else if (normalImage is not null && (specularGlossImage ?? metallicRoughnessImage) is not null) {
                    cryMaterial.GenMask.UseBumpMap = true;
                    cryMaterial.GenMask.UseScatterGlossInNormalMap = true;
                    var bsgImage = normalImage.Clone();
                    bsgImage.ProcessPixelRows(
                        bsg => {
                            for (var i = 0; i < bsg.Height; i++) {
                                var bsgSpan = bsg.GetRowSpan(i);
                                for (var j = 0; j < bsgSpan.Length; j++)
                                    // scatter, normal.x, gloss, normal.y
                                    bsgSpan[j] = new(0, bsgSpan[j].R, 0, bsgSpan[j].G);
                            }
                        });
                    if (specularGlossImage is not null) {
                        bsgImage.ProcessPixelRows(
                            specularGlossImage,
                            (bsg, sg) => {
                                for (var i = 0; i < bsg.Height; i++) {
                                    var bsgSpan = bsg.GetRowSpan(i);
                                    var sgSpan = sg.GetRowSpan(i * sg.Height / bsg.Height);
                                    for (var j = 0; j < bsgSpan.Length; j++)
                                        bsgSpan[j].B = sgSpan[j * sg.Width / bsg.Width].A;
                                }
                            });
                    } else if (metallicRoughnessImage is not null) {
                        bsgImage.ProcessPixelRows(
                            metallicRoughnessImage,
                            (bsg, mr) => {
                                for (var i = 0; i < bsg.Height; i++) {
                                    var bsgSpan = bsg.GetRowSpan(i);
                                    var mrSpan = mr.GetRowSpan(i * mr.Height / bsg.Height);
                                    for (var j = 0; j < bsgSpan.Length; j++)
                                        bsgSpan[j].B = (byte) (255 - mrSpan[j * mr.Width / bsg.Width].G);
                                }
                            });
                    } else
                        throw new InvalidOperationException();

                    AddCryTexture(cryMaterialIndex, cryMaterial, TextureMapType.Normals, bsgImage, true);
                } else if (normalImage is not null) {
                    cryMaterial.GenMask.UseBumpMap = true;
                    cryMaterial.GenMask.UseScatterGlossInNormalMap = false;
                    AddCryTexture(cryMaterialIndex, cryMaterial, TextureMapType.Normals, normalImage, false);
                // } else {
                //     normalImage = new(8, 8);
                //     for (var i = 0; i < 8; i++)
                //     for (var j = 0; j < 8; j++)
                //         normalImage[i, j] = new(127, 127, 255, 255);
                //
                //     cryMaterial.GenMask.UseBumpMap = true;
                //     cryMaterial.GenMask.UseScatterGlossInNormalMap = false;
                //     AddCryTexture(cryMaterialIndex, cryMaterial, TextureMapType.Normals, normalImage, false);
                }
            }
        }

        private void Step03GenerateMesh() {
            foreach (var gltfNode in RootNodes.Where(x => x.Mesh.HasValue)) {
                var mesh = _gltf.Root.Meshes[gltfNode.Mesh!.Value];
                var cryMeshNode = new Node(gltfNode.Name ?? $"unnamed_{_model.Nodes.Count}", _name) {
                    HasColors = _gltf.Root.Meshes.SelectMany(x => x.Primitives)
                        .All(x => x.Attributes.Color0 is not null),
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
                            || primitive.Attributes.TexCoord0 is null)
                            throw new InvalidDataException();

                        var externalIndices = _gltf.ReadUInt16Array(primitive.Indices.Value);
                        var usedIndices = externalIndices.Distinct().Order().Select((x, i) => (x, i))
                            .ToDictionary(x => x.x, x => checked((ushort) (vertices.Count + x.i)));

                        var positions = _gltf.ReadVector3Array(primitive.Attributes.Position.Value);
                        var normals = _gltf.ReadVector3Array(primitive.Attributes.Normal.Value);
                        var texCoords = _gltf.ReadVector2Array(primitive.Attributes.TexCoord0.Value);
                        if (positions.Length != normals.Length || positions.Length != texCoords.Length)
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

                        vertices.EnsureCapacity(vertices.Count + usedIndices.Count);
                        vertices.AddRange(
                            usedIndices.Keys.Order()
                                .Select(
                                    i => new Vertex {
                                        Position = SwapAxes(positions[i]),
                                        Normal = SwapAxes(normals[i]),
                                        TexCoord = texCoords[i],
                                        Weights = weights?[i] ?? default(Vector4<float>),
                                        ControllerIds = joints is null
                                            ? default
                                            : new(
                                                joints[i]
                                                    .Zip(weights![i])
                                                    .Select(x => x.Second > 0 ? _boneIdToControllerId[x.First] : 0)),
                                    }));

                        var indicesSpan = CollectionsMarshal.AsSpan(indices)[^externalIndices.Length..];
                        var verticesSpan = CollectionsMarshal.AsSpan(vertices)[^usedIndices.Count..];

                        if (primitive.Attributes.Color0 is not null) {
                            var colors = _gltf.ReadVector4Array(primitive.Attributes.Color0.Value);
                            if (positions.Length != colors.Length)
                                throw new InvalidDataException();
                            foreach (var i in usedIndices.Keys.Order()) {
                                verticesSpan[i].Color = new(
                                    Enumerable.Range(0, 4).Select(j => (byte) (colors[i][j] * 255f)));
                            }
                        }

                        if (primitive.Attributes.Tangent is not null) {
                            var tangents = _gltf.ReadVector4Array(primitive.Attributes.Tangent.Value);
                            if (positions.Length != tangents.Length)
                                throw new InvalidDataException();
                            foreach (var i in usedIndices.Keys.Order()) {
                                verticesSpan[i].Tangent = MeshTangent.FromNormalAndBinormal(
                                    verticesSpan[i].Normal,
                                    SwapAxesTangent(tangents[i]));
                                // ^ note: See GltfExporter for why is it putting tangents into binormal
                            }
                        } else {
                            var accAreas = new float[verticesSpan.Length];
                            var accNormals = new Vector3[verticesSpan.Length];
                            var accTangents = new Vector3[verticesSpan.Length];
                            var accBinormals = new Vector3[verticesSpan.Length];
                            for (var i = 0; i < indicesSpan.Length; i += 3) {
                                ref var v0 = ref verticesSpan[indicesSpan[i + 0]];
                                ref var v1 = ref verticesSpan[indicesSpan[i + 1]];
                                ref var v2 = ref verticesSpan[indicesSpan[i + 2]];
                                var area = Vertex.CalculateArea(v0, v1, v2);
                                Vertex.CalculateNormalTangentBinormals(v0, v1, v2, out var n, out var t, out var b);
                                accAreas[indicesSpan[i + 0]] += area;
                                accAreas[indicesSpan[i + 1]] += area;
                                accAreas[indicesSpan[i + 2]] += area;
                                accNormals[indicesSpan[i + 0]] += area * n;
                                accNormals[indicesSpan[i + 1]] += area * n;
                                accNormals[indicesSpan[i + 2]] += area * n;
                                accTangents[indicesSpan[i + 0]] += area * t;
                                accTangents[indicesSpan[i + 1]] += area * t;
                                accTangents[indicesSpan[i + 2]] += area * t;
                                accBinormals[indicesSpan[i + 0]] += area * b;
                                accBinormals[indicesSpan[i + 1]] += area * b;
                                accBinormals[indicesSpan[i + 2]] += area * b;
                            }

                            for (var i = 0; i < verticesSpan.Length; i++) {
                                var area = accAreas[i];
                                if (area <= 0f)
                                    continue;
                                verticesSpan[i].Normal = Vector3.Normalize(accNormals[i] / area);
                                verticesSpan[i].Tangent.Tangent = new(Vector3.Normalize(accTangents[i] / area), -1);
                                verticesSpan[i].Tangent.Binormal = new(Vector3.Normalize(accBinormals[i] / area), -1);
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
            }

            if (_model.Nodes.Count > 1) {
                var realRootNode = new Node(_name, _name);
                realRootNode.Children.AddRange(_model.Nodes);
                _model.Nodes.Clear();
                _model.Nodes.Add(realRootNode);
            }

            _model.PseudoMaterials.Add(new(_name) {Flags = MtlNameFlags.MultiMaterial});
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
