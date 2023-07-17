using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using BCnEncoder.Encoder;
using BCnEncoder.ImageSharp;
using BCnEncoder.Shared;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SynergyLib.FileFormat.CryEngine.CryAnimationDatabaseElements;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;
using SynergyLib.FileFormat.CryEngine.CryModelElements;
using SynergyLib.FileFormat.CryEngine.CryXml;
using SynergyLib.FileFormat.CryEngine.CryXml.CharacterDefinitionElements;
using SynergyLib.FileFormat.CryEngine.CryXml.MaterialElements;
using SynergyLib.FileFormat.GltfInterop;
using SynergyLib.FileFormat.GltfInterop.Models;
using SynergyLib.Util;
using SynergyLib.Util.MathExtras;

namespace SynergyLib.FileFormat.CryEngine;

public partial class CryCharacter {
    public CharacterDefinition? Definition;
    public CryModel Model;
    public CharacterParameters? CharacterParameters;
    public CryAnimationDatabase? CryAnimationDatabase;
    public List<CryModel> Attachments = new();

    public CryCharacter(CryModel model) {
        Model = model;
    }

    public Task<GltfTuple> ToGltf(
        Func<string, CancellationToken, Task<Stream>> getStream,
        CancellationToken cancellationToken) =>
        new GltfExporter(this, getStream, cancellationToken).Convert();

    public static async Task<CryCharacter> FromCryEngineFiles(
        Func<string, CancellationToken, Task<Stream>> streamOpener,
        string baseName,
        CancellationToken cancellationToken) {
        if (Path.GetExtension(baseName).ToLowerInvariant() is ".cdf" or ".cgf" or ".chr")
            baseName = Path.ChangeExtension(baseName, null);
        CharacterDefinition? definition = null;
        CryModel model;
        try {
            await using (var definitionStream = await streamOpener($"{baseName}.cdf", cancellationToken))
                definition = PbxmlFile.FromStream(definitionStream).DeserializeAs<CharacterDefinition>();
            if (definition.Model is null)
                throw new InvalidDataException("Definition.Model should not be null");
            if (definition.Model.File is null)
                throw new InvalidDataException("Definition.Model.File should not be null");
            if (definition.Model.Material is null)
                throw new InvalidDataException("Definition.Model.Material should not be null");

            model = CryModel.FromCryEngineFiles(
                await streamOpener(definition.Model.File, cancellationToken),
                await streamOpener(definition.Model.Material, cancellationToken));
        } catch (FileNotFoundException) {
            model = CryModel.FromCryEngineFiles(
                await streamOpener($"{baseName}.chr", cancellationToken),
                await streamOpener($"{baseName}.mtl", cancellationToken));
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
                res.Attachments.Add(
                    CryModel.FromCryEngineFiles(
                        await streamOpener(d.Binding, cancellationToken),
                        await streamOpener(d.Material, cancellationToken)));
            }
        }

        try {
            await using (var s = await streamOpener($"{baseName}.chrparams", cancellationToken))
                res.CharacterParameters = PbxmlFile.FromStream(s).DeserializeAs<CharacterParameters>();
            if (res.CharacterParameters.TracksDatabasePath is not null)
                await using (var s = await streamOpener(res.CharacterParameters.TracksDatabasePath, cancellationToken))
                    res.CryAnimationDatabase = CryAnimationDatabase.FromStream(s);
        } catch (FileNotFoundException) { }

        return res;
    }

    public static CryCharacter FromGltf(GltfTuple gltf) {
        var root = gltf.Root;
        var rootNode = root.Nodes[root.Scenes[root.Scene].Nodes.Single()];

        var model = new CryModel(rootNode.Name ?? "Untitled");

        var boneIdToControllerId = new Dictionary<int, uint>();
        var nodeIdToControllerId = new Dictionary<int, uint>();

        if (rootNode.Skin is not null && root.Skins[rootNode.Skin.Value] is {Joints: not null} skin) {
            var ibm = skin.InverseBindMatrices is null
                ? throw new InvalidDataException()
                : gltf.ReadTypedArray<Matrix4x4>(skin.InverseBindMatrices.Value)
                    .Select(SwapAxes).ToArray();
            model.Controllers.EnsureCapacity(skin.Joints.Count);

            var pendingTraversal = rootNode.Children.Select(x => Tuple.Create(x, (Controller?) null)).ToList();
            while (pendingTraversal.Any()) {
                var (nodeIndex, parentController) = pendingTraversal.First();
                pendingTraversal.RemoveAt(0);

                var boneIndex = skin.Joints.IndexOf(nodeIndex);
                if (boneIndex == -1)
                    throw new InvalidDataException();

                var node = root.Nodes[nodeIndex];
                var controller =
                    node.Name?.IndexOf("$$") is { } sharp && sharp != -1
                        ? new(
                            Convert.ToUInt32(node.Name[(sharp + 2)..], 16),
                            node.Name[..sharp],
                            ibm[skin.Joints.IndexOf(nodeIndex)],
                            parentController)
                        : new Controller(
                            node.Name ?? $"Bone#{model.Controllers.Count}",
                            ibm[skin.Joints.IndexOf(nodeIndex)],
                            parentController);
                model.Controllers.Add(controller);
                nodeIdToControllerId[nodeIndex] = boneIdToControllerId[boneIndex] = controller.Id;
                pendingTraversal.AddRange(node.Children.Select(child => Tuple.Create(child, controller))!);
            }
        }

        var ddsEncoder = new BcEncoder {
            OutputOptions = {
                GenerateMipMaps = false,
                Quality = CompressionQuality.BestQuality,
                Format = CompressionFormat.Bc3,
                FileFormat = OutputFileFormat.Dds,
            },
        };

        if (rootNode.Mesh is not null) {
            var mesh = root.Meshes[rootNode.Mesh.Value];
            foreach (var materialIndex in Enumerable.Range(-1, 1 + root.Materials.Count)) {
                var primitives = mesh.Primitives
                    .Where(x => (x.Material is null && materialIndex == -1) || x.Material == materialIndex)
                    .ToArray();
                if (!primitives.Any())
                    continue;

                var material = materialIndex == -1 ? null : root.Materials[materialIndex];
                var materialName = materialIndex == -1
                    ? "_Empty"
                    : string.IsNullOrWhiteSpace(material?.Name)
                        ? $"_Unnamed_{model.Material.SubMaterials!.Count}"
                        : material.Name;
                var cryMaterial = new Material {
                    Name = materialName,
                    AlphaTest = material?.AlphaCutoff ?? 0f,
                    MaterialFlags = (material?.DoubleSided is true ? MaterialFlags.TwoSided : 0) |
                        MaterialFlags.ShaderGenMask64Bit,
                    DiffuseColor = material?.PbrMetallicRoughness?.BaseColorFactor?.ToVector3(),
                    SpecularColor = material?.Extensions?.KhrMaterialsSpecular?.SpecularColorFactor?.ToVector3(),
                    Shininess = material?.Extensions?.KhrMaterialsPbrSpecularGlossiness?.GlossinessFactor ?? 200,
                    Opacity = material?.PbrMetallicRoughness?.BaseColorFactor?[3] ?? 1f,
                    GlowAmount = material?.Extensions?.KhrMaterialsEmissiveStrength?.EmissiveStrength ?? 0f,
                    Shader = "Brb_Illum",
                    StringGenMask = string.Empty,
                    Textures = new(),
                };
                model.Material.SubMaterials!.Add(cryMaterial);

                if (material?.NormalTexture?.Index is { } normalTextureIndex) {
                    cryMaterial.StringGenMask += "%BUMP_MAP";
                    var image = Image.Load<Rgba32>(
                            gltf.ReadBufferView(
                                root.Images[
                                        root.Textures[normalTextureIndex].Source ??
                                        throw new InvalidDataException()]
                                    .BufferView ?? throw new InvalidDataException())) ??
                        throw new InvalidDataException();

                    var ms = new MemoryStream();
                    ddsEncoder.EncodeToStream(image, ms);
                    model.ExtraTextures[$"mod/{rootNode.Name}/{materialName}_normals.dds"] = ms;
                    cryMaterial.Textures.Add(
                        new() {
                            File = $"mod/{rootNode.Name}/{materialName}_normals.tif",
                            Map = Texture.MapTypeEnum.Normals,
                        });
                }

                if (material?.PbrMetallicRoughness?.BaseColorTexture?.Index is { } diffuseTextureIndex) {
                    var image = Image.Load<Rgba32>(
                            gltf.ReadBufferView(
                                root.Images[root.Textures[diffuseTextureIndex].Source ??
                                        throw new InvalidDataException()]
                                    .BufferView ?? throw new InvalidDataException())) ??
                        throw new InvalidDataException();

                    var ms = new MemoryStream();
                    ddsEncoder.EncodeToStream(image, ms);
                    model.ExtraTextures[$"mod/{rootNode.Name}/{materialName}_diffuse.dds"] = ms;
                    cryMaterial.Textures.Add(
                        new() {
                            File = $"mod/{rootNode.Name}/{materialName}_diffuse.tif",
                            Map = Texture.MapTypeEnum.Diffuse,
                        });
                }

                if (material?.Extensions?.KhrMaterialsSpecular?.SpecularTexture?.Index is
                    { } specularTextureIndex) {
                    cryMaterial.StringGenMask += "%GLOSS_MAP";
                    var image = Image.Load<Rgba32>(
                            gltf.ReadBufferView(
                                root.Images[root.Textures[specularTextureIndex].Source ??
                                        throw new InvalidDataException()]
                                    .BufferView ?? throw new InvalidDataException())) ??
                        throw new InvalidDataException();

                    var ms = new MemoryStream();
                    ddsEncoder.EncodeToStream(image, ms);
                    model.ExtraTextures[$"mod/{rootNode.Name}/{materialName}_specular.dds"] = ms;
                    cryMaterial.Textures.Add(
                        new() {
                            File = $"mod/{rootNode.Name}/{materialName}_specular.tif",
                            Map = Texture.MapTypeEnum.Specular,
                        });
                }

                if (cryMaterial.Textures.All(x => x.Map != Texture.MapTypeEnum.Specular)) {
                    if (cryMaterial.Textures.SingleOrDefault(x => x.Map == Texture.MapTypeEnum.Diffuse) is
                        { } diffuse) {
                        cryMaterial.Textures.Add(
                            new() {
                                File = diffuse.File,
                                Map = Texture.MapTypeEnum.Diffuse,
                            });
                    }
                }

                var vertices = new List<Vertex>();
                var indices = new List<ushort>();
                foreach (var primitive in primitives) {
                    if (primitive.Indices is null
                        || primitive.Attributes.Position is null
                        || primitive.Attributes.Normal is null
                        || primitive.Attributes.Tangent is null
                        || primitive.Attributes.Color0 is null
                        || primitive.Attributes.TexCoord0 is null)
                        throw new InvalidDataException();

                    var externalIndices = gltf.ReadUInt16Array(primitive.Indices.Value);
                    var usedIndices = externalIndices.Distinct().Order().Select((x, i) => (x, i))
                        .ToDictionary(x => x.x, x => checked((ushort) (vertices.Count + x.i)));

                    var positions = gltf.ReadVector3Array(primitive.Attributes.Position.Value);
                    var normals = gltf.ReadVector3Array(primitive.Attributes.Normal.Value);
                    var tangents = gltf.ReadVector4Array(primitive.Attributes.Tangent.Value);
                    var colors = gltf.ReadVector4Array(primitive.Attributes.Color0.Value);
                    var texCoords = gltf.ReadVector2Array(primitive.Attributes.TexCoord0.Value);
                    if (positions.Length != normals.Length
                        || positions.Length != tangents.Length
                        || positions.Length != colors.Length
                        || positions.Length != texCoords.Length)
                        throw new InvalidDataException();

                    Vector4<float>[]? weights = null;
                    Vector4<ushort>[]? joints = null;
                    if (model.Controllers.Any()) {
                        if (primitive.Attributes.Weights0 is null
                            || primitive.Attributes.Joints0 is null)
                            throw new InvalidDataException();

                        weights = gltf.ReadVector4SingleArray(primitive.Attributes.Weights0.Value);
                        joints = gltf.ReadVector4UInt16Array(primitive.Attributes.Joints0.Value);
                    }

                    indices.EnsureCapacity(indices.Count + externalIndices.Length);
                    foreach (var i in externalIndices)
                        indices.Add(usedIndices[i]);

                    vertices.EnsureCapacity(vertices.Count + usedIndices.Count);
                    foreach (var i in usedIndices.Keys.Order()) {
                        vertices.Add(
                            new() {
                                Position = SwapAxes(positions[i]),
                                Normal = SwapAxes(normals[i]),
                                Tangent = MeshTangent.FromNormalAndTangent(
                                    SwapAxes(normals[i]),
                                    SwapAxesTangent(tangents[i])),
                                Color = new(Enumerable.Range(0, 4).Select(j => (byte) (colors[i][j] * 255f))),
                                TexCoord = texCoords[i],
                                Weights = weights?[i] ?? default(Vector4<float>),
                                ControllerIds = joints is null
                                    ? default
                                    : new(
                                        joints[i].Zip(weights![i])
                                            .Select(x => x.Second > 0 ? boneIdToControllerId[x.First] : 0)),
                            });
                    }
                }

                model.Meshes.Add(new(materialName, vertices.ToArray(), indices.ToArray()));
            }
        }

        var animdb = new CryAnimationDatabase();
        foreach (var animation in root.Animations) {
            var anim = new Animation {
                MotionParams = new() {
                    Start = int.MaxValue,
                    End = int.MinValue,
                }
            };
            foreach (var channel in animation.Channels) {
                if (channel.Target.Node is not { } nodeIndex)
                    throw new InvalidDataException();

                var controllerId = nodeIdToControllerId[nodeIndex];
                switch (channel.Target.Path) {
                    case GltfAnimationChannelTargetPath.Translation: {
                        var time = gltf.ReadSingleArray(animation.Samplers[channel.Sampler].Input)
                            .Select(x => 30 * x).ToArray();
                        var pos = gltf.ReadVector3Array(animation.Samplers[channel.Sampler].Output)
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
                        var time = gltf.ReadSingleArray(animation.Samplers[channel.Sampler].Input)
                            .Select(x => 30 * x).ToArray();
                        var rot = gltf.ReadQuaternionArray(animation.Samplers[channel.Sampler].Output)
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
                animdb.Animations[animation.Name ?? $"Animation_{animdb.Animations.Count}"] = anim;
        }

        return new(model) {CryAnimationDatabase = animdb};
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
                namesDepths.Add(
                    fullNames.Select(x => string.Join('/', x.Split('/').TakeLast(nameDepth + 1))).ToList());

            names.Add(namesDepths[nameDepth][i]);
        }

        return names;
    }

    protected static Vector3 SwapAxes(Vector3 val) => new(-val.X, val.Z, val.Y);
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

    public void Scale(float scale) {
        Model.Scale(scale);
        CryAnimationDatabase?.Scale(scale);
        // todo: scale attachments
    }
}
