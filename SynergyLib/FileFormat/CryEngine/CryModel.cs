using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Chunks;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Enums;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;
using SynergyLib.FileFormat.CryEngine.CryModelElements;
using SynergyLib.FileFormat.CryEngine.CryXml;
using SynergyLib.Util.BinaryRW;
using SynergyLib.Util.MathExtras;

namespace SynergyLib.FileFormat.CryEngine;

public class CryModel {
    public readonly Material? Material;
    public readonly List<PseudoMaterial> PseudoMaterials = new();
    public readonly List<Node> Nodes = new();
    public readonly Dictionary<string, MemoryStream> ExtraTextures = new();
    public Controller? RootController;
    public bool MergeAllNodes;
    public bool HaveAutoLods;
    public bool UseCustomNormals;

    public CryModel(Material? material) {
        Material = material;
    }

    public void ChangeScale(float scale) {
        foreach (var node in Nodes)
            node.ChangeScale(scale);

        if (RootController is not null)
            foreach (var c in RootController.GetEnumeratorDepthFirst()) {
                var (tra, rot) = c.Decomposed;
                c.Decomposed = Tuple.Create(tra * scale, rot);
            }
    }

    public void WriteGeometryTo(NativeWriter writer) {
        var chunks = new CryChunks {
            Type = CryFileType.Geometry,
            Version = CryFileVersion.CryTek3
        };

        var pseudoMaterials = new Dictionary<PseudoMaterial, int>();
        foreach (var rootPseudoMaterial in PseudoMaterials) {
            foreach (var (pseudoMaterial, parent) in rootPseudoMaterial.EnumerateHierarchy()) {
                var chunk = chunks.AddChunkLE(
                    ChunkType.MtlName,
                    0x800,
                    new MtlNameChunk {
                        Flags = pseudoMaterial.Flags,
                        Name = pseudoMaterial.Name,
                        ShOpacity = pseudoMaterial.ShOpacity,
                        SubMaterialChunkIds = new(pseudoMaterial.Children.Count)
                    });
                if (parent is not null)
                    ((MtlNameChunk) chunks[pseudoMaterials[parent]]).SubMaterialChunkIds.Add(chunk.Header.Id);
                pseudoMaterials[pseudoMaterial] = chunk.Header.Id;
            }
        }

        Dictionary<uint, ushort>? controllerIdToBoneId = null;
        List<ushort>? extToInt = null;
        CompiledIntSkinVerticesChunk? intSkinVertices = null;
        if (RootController is not null) {
            var compiledBones = chunks.AddChunkBE(
                ChunkType.CompiledBones,
                0x800,
                new CompiledBonesChunk {Bones = RootController.ToCompiledBonesList().ToList()});

            controllerIdToBoneId = compiledBones.Bones
                .Select((x, i) => (x, i))
                .ToDictionary(x => x.x.ControllerId, x => (ushort) x.i);

            // TODO
            // chunks.AddChunkBE(
            //     ChunkType.CompiledPhysicalBones,
            //     0x800,
            //     new CompiledPhysicalBonesChunk {Bones = Controller.ToBoneEntityList(Controllers)});
            //
            // chunks.AddChunkBE(ChunkType.CompiledPhysicalProxies, 0x800, new CompiledPhysicalProxyChunk());
            //
            // chunks.AddChunkBE(ChunkType.CompiledMorphTargets, 0x800, new CompiledMorphTargetsChunk());
            //
            // intSkinVertices = chunks.AddChunkBE(
            //     ChunkType.CompiledIntSkinVertices,
            //     0x800,
            //     new CompiledIntSkinVerticesChunk {
            //         Vertices = Mesh.ToIntSkinVertices(RootNode.Meshes, controllerIdToBoneId, out extToInt),
            //     });
            //
            // chunks.AddChunkBE(
            //     ChunkType.CompiledIntFaces,
            //     0x800,
            //     new CompiledIntFacesChunk {Faces = Mesh.ToIntFaces(RootNode.Meshes, extToInt)});
            //
            // chunks.AddChunkBE(
            //     ChunkType.CompiledExt2IntMap,
            //     0x800,
            //     new CompiledExtToIntMapChunk {Map = extToInt});
            //
            // foreach (var controller in Controllers) {
            //     var boneId = controllerIdToBoneId[controller.Id];
            //     var boneVertexIndices = new List<ushort>();
            //     var aabb = AaBb.NegativeExtreme;
            //     foreach (var extIndex in Enumerable.Range(0, extToInt.Count)) {
            //         var intIndex = extToInt[extIndex];
            //         if (!intSkinVertices.Vertices[intIndex].BoneIds.Contains(boneId))
            //             continue;
            //
            //         boneVertexIndices.Add((ushort) extIndex);
            //         aabb.Expand(intSkinVertices.Vertices[intIndex].Position1);
            //     }
            //
            //     if (!boneVertexIndices.Any())
            //         continue;
            //
            //     chunks.AddChunkBE(
            //         ChunkType.BonesBoxes,
            //         0x801,
            //         new BonesBoxesChunk {BoneId = boneId, Indices = boneVertexIndices, AaBb = aabb});
            // }
        }

        chunks.AddChunkLE(
            ChunkType.ExportFlags,
            1,
            new ExportFlagsChunk {
                Flags = ExportFlags.UseCustomNormals,
                RcVersion = new(4123, 1, 4, 3),
                RcVersionString = " RCVer:4.1 ",
            });

        // (ChunkType.MeshPhysicsData, 0x800) => new MeshPhysicsDataChunk(),

        var nodes = new Dictionary<Node, int>();
        foreach (var rootNode in Nodes) {
            foreach (var (node, parent) in rootNode.EnumerateHierarchy()) {
                var meshSubsetsChunk = chunks.AddChunkBE(
                    ChunkType.MeshSubsets,
                    0x800,
                    new MeshSubsetsChunk {
                        BoneIds = new(),
                        Flags = controllerIdToBoneId is null ? 0 : MeshSubsetsFlags.BoneIndices,
                        Subsets = new(),
                    });

                var totalVertices = node.Meshes.Sum(x => x.Vertices.Length);
                var positionsChunk = chunks.AddChunkBE(
                    ChunkType.DataStream,
                    0x800,
                    new DataChunk {Type = CgfStreamType.Positions});
                var normalsChunk = chunks.AddChunkBE(
                    ChunkType.DataStream,
                    0x800,
                    new DataChunk {Type = CgfStreamType.Normals});
                var texCoordsChunk = chunks.AddChunkBE(
                    ChunkType.DataStream,
                    0x800,
                    new DataChunk {Type = CgfStreamType.TexCoords});
                var colorsChunk = chunks.AddChunkBE(
                    ChunkType.DataStream,
                    0x800,
                    new DataChunk {Type = CgfStreamType.Colors});
                var tangentsChunk = chunks.AddChunkBE(
                    ChunkType.DataStream,
                    0x800,
                    new DataChunk {Type = CgfStreamType.Tangents});
                positionsChunk.FromEnumerable(
                    node.Meshes.SelectMany(x => x.Vertices.Select(y => y.Position)),
                    totalVertices);
                normalsChunk.FromEnumerable(
                    node.Meshes.SelectMany(x => x.Vertices.Select(y => y.Normal)),
                    totalVertices);
                texCoordsChunk.FromEnumerable(
                    node.Meshes.SelectMany(x => x.Vertices.Select(y => y.TexCoord)),
                    totalVertices);
                colorsChunk.FromEnumerable(
                    node.Meshes.SelectMany(x => x.Vertices.Select(y => y.Color)),
                    totalVertices);
                tangentsChunk.FromEnumerable(
                    node.Meshes.SelectMany(x => x.Vertices.Select(y => y.Tangent)),
                    totalVertices);

                DataChunk? boneMappingsChunk = null;
                if (controllerIdToBoneId is not null) {
                    Debug.Assert(extToInt is not null);
                    Debug.Assert(intSkinVertices is not null);

                    boneMappingsChunk = chunks.AddChunkBE(
                        ChunkType.DataStream,
                        0x800,
                        new DataChunk {
                            Type = CgfStreamType.BoneMapping,
                            ElementSize = Unsafe.SizeOf<MeshBoneMapping>(),
                            NativeData = new byte[Unsafe.SizeOf<MeshBoneMapping>() * totalVertices],
                        }
                    );
                }

                var totalIndices = node.Meshes.Sum(x => x.Indices.Length);
                var indicesChunk = chunks.AddChunkBE(
                    ChunkType.DataStream,
                    0x800,
                    new DataChunk {
                        Type = CgfStreamType.Indices,
                        ElementSize = Unsafe.SizeOf<ushort>(),
                        NativeData = new byte[Unsafe.SizeOf<ushort>() * totalIndices],
                    });
                {
                    var baseVertexIndex = 0;
                    var baseIndexIndex = 0;

                    var boneUseRange = new ushort[controllerIdToBoneId?.Count ?? 0, 2];
                    for (var i = 0; i < boneUseRange.GetLength(0); i++)
                    for (var j = 0; j < boneUseRange.GetLength(1); j++)
                        boneUseRange[i, j] = ushort.MaxValue;

                    var boneSlots = new ushort[MeshSubsetsChunk.MaxBoneIdPerSubset];
                    boneSlots.AsSpan().Fill(ushort.MaxValue);

                    var usedBoneIndices = new HashSet<ushort>(MeshSubsetsChunk.MaxBoneIdPerSubset);
                    var currentBoneIndices = new HashSet<ushort>(12);
                    foreach (var mesh in node.Meshes) {
                        for (var i = 0; i < mesh.Indices.Length; i++)
                            indicesChunk.SetItem(baseIndexIndex + i, (ushort) (baseVertexIndex + mesh.Indices[i]));

                        var matId = Material?.SubMaterials!
                            .Select((x, i) => (x, i))
                            .Single(x => x.x?.Name == mesh.MaterialName)
                            .i ?? 0;
                        if (controllerIdToBoneId is not null) {
                            Debug.Assert(extToInt is not null);
                            Debug.Assert(intSkinVertices is not null);
                            Debug.Assert(boneMappingsChunk is not null);

                            for (var i = 0; i < mesh.Indices.Length; i += 3) {
                                for (var j = 0; j < 3; j++) {
                                    var v = mesh.Vertices[mesh.Indices[i + j]];
                                    for (var k = 0; k < 4; k++) {
                                        if (v.Weights[k] == 0)
                                            continue;
                                        var boneId = controllerIdToBoneId[v.ControllerIds[k]];
                                        if (boneUseRange[boneId, 0] == ushort.MaxValue)
                                            boneUseRange[boneId, 0] = (ushort) (baseIndexIndex + i);
                                        boneUseRange[boneId, 1] = (ushort) (baseIndexIndex + i + 3);
                                    }
                                }
                            }

                            var firstIndexId = 0;

                            void FlushSubset(int nextIndexId) {
                                meshSubsetsChunk.BoneIds.Add(
                                    boneSlots
                                        .Reverse()
                                        .SkipWhile(x => x == ushort.MaxValue)
                                        .Reverse()
                                        .Select(x => x == ushort.MaxValue ? (ushort) 0 : x)
                                        .ToArray());
                                var firstVertId = Enumerable.Range(firstIndexId, nextIndexId - firstIndexId)
                                    .Select(x => mesh.Indices[x]).Min();
                                var lastVertId = Enumerable.Range(firstIndexId, nextIndexId - firstIndexId)
                                    .Select(x => mesh.Indices[x]).Max();
                                var aabb = AaBb.NegativeExtreme;
                                for (var i = firstIndexId; i < nextIndexId; i++)
                                    aabb.Expand(mesh.Vertices[mesh.Indices[i]].Position);

                                meshSubsetsChunk.Subsets.Add(
                                    new() {
                                        FirstIndexId = baseIndexIndex + firstIndexId,
                                        NumIndices = nextIndexId - firstIndexId,
                                        FirstVertId = baseVertexIndex + firstVertId,
                                        NumVerts = lastVertId - firstVertId + 1,
                                        MatId = matId,
                                        Center = aabb.Center,
                                        Radius = aabb.Radius,
                                    });

                                firstIndexId = nextIndexId;
                                usedBoneIndices.RemoveWhere(
                                    x => {
                                        if (baseIndexIndex + nextIndexId < boneUseRange[x, 1])
                                            return false;

                                        boneSlots[Array.IndexOf(boneSlots, x)] = ushort.MaxValue;
                                        return true;
                                    });
                            }

                            for (var i = 0; i < mesh.Indices.Length; i += 3) {
                                currentBoneIndices.Clear();
                                for (var j = 0; j < 3; j++) {
                                    var v = mesh.Vertices[mesh.Indices[i + j]];
                                    for (var k = 0; k < 4; k++) {
                                        if (v.Weights[k] == 0)
                                            continue;
                                        currentBoneIndices.Add(controllerIdToBoneId[v.ControllerIds[k]]);
                                    }
                                }

                                if (usedBoneIndices.Concat(currentBoneIndices).Distinct().Count() > boneSlots.Length)
                                    FlushSubset(i);

                                foreach (var x in currentBoneIndices.Where(x => usedBoneIndices.Add(x)))
                                    boneSlots[Array.IndexOf(boneSlots, ushort.MaxValue)] = x;
                            }

                            FlushSubset(mesh.Indices.Length);
                        } else {
                            var firstVertId = mesh.Indices.Min();
                            var lastVertId = mesh.Indices.Max();
                            var aabb = AaBb.NegativeExtreme;
                            foreach (var index in mesh.Indices)
                                aabb.Expand(mesh.Vertices[index].Position);

                            meshSubsetsChunk.Subsets.Add(
                                new() {
                                    FirstIndexId = baseIndexIndex,
                                    NumIndices = mesh.Indices.Length,
                                    FirstVertId = baseVertexIndex + firstVertId,
                                    NumVerts = lastVertId - firstVertId + 1,
                                    MatId = matId,
                                    Center = aabb.Center,
                                    Radius = aabb.Radius,
                                });
                        }

                        baseVertexIndex += mesh.Vertices.Length;
                        baseIndexIndex += mesh.Indices.Length;
                    }
                }

                // var shapeDeformation = chunks.AddChunkBE(
                //     ChunkType.DataStream,
                //     0x800,
                //     Chunks.Values.OfType<DataChunk>().Single(x => x.Type == CgfStreamType.ShapeDeformation));

                if (controllerIdToBoneId is not null) {
                    Debug.Assert(extToInt is not null);
                    Debug.Assert(intSkinVertices is not null);
                    Debug.Assert(boneMappingsChunk is not null);
                    foreach (var (subset, subsetBoneIds) in meshSubsetsChunk.Subsets.Zip(meshSubsetsChunk.BoneIds)) {
                        for (var i = subset.FirstIndexId; i < subset.FirstIndexId + subset.NumIndices; i++) {
                            var vertexIndex = indicesChunk.GetItem<ushort>(i);
                            var intIndex = extToInt[vertexIndex];
                            var weights = new Vector4<byte>(
                                intSkinVertices.Vertices[intIndex].Weights.Select(x => (byte) Math.Round(255 * x)));
                            var boneIds = new Vector4<byte>(
                                intSkinVertices.Vertices[intIndex].BoneIds.Select(
                                    (x, y) => (byte) (weights[y] == 0 ? 0 : Array.IndexOf(subsetBoneIds, x))));

                            var item = boneMappingsChunk.GetItem<MeshBoneMapping>(vertexIndex);
                            item.BoneIds = boneIds;
                            item.Weights = weights;

                            var weightSum = item.Weights.Sum(x => x);
                            if (weightSum != 255) {
                                for (var j = 0; j < 4; j++) {
                                    if (item.Weights[j] != 0) {
                                        item.Weights[j] += (byte) (255 - weightSum);
                                        break;
                                    }
                                }
                            }

                            boneMappingsChunk.SetItem(vertexIndex, item);
                        }
                    }
                }

                var meshChunk = chunks.AddChunkBE(
                    ChunkType.Mesh,
                    0x800,
                    new MeshChunk {
                        Bbox = AaBb.FromVectorEnumerable(
                            node.Meshes.SelectMany(x => x.Vertices.Select(y => y.Position))),
                        BoneMappingChunkId = boneMappingsChunk?.Header.Id ?? 0,
                        ColorsChunkId = colorsChunk.Header.Id,
                        Flags = MeshChunkFlags.HasTexMappingDensity,
                        Flags2 = PhysicalizeFlags.MeshNotNeeded | PhysicalizeFlags.NoBreaking,
                        IndexCount = totalIndices,
                        IndicesChunkId = indicesChunk.Header.Id,
                        NormalsChunkId = normalsChunk.Header.Id,
                        PositionsChunkId = positionsChunk.Header.Id,
                        // ShapeDeformationChunkId = shapeDeformation.Header.Id,
                        SubsetsChunkId = meshSubsetsChunk.Header.Id,
                        SubsetsCount = meshSubsetsChunk.Subsets.Count,
                        TangentsChunkId = tangentsChunk.Header.Id,
                        TexCoordsChunkId = texCoordsChunk.Header.Id,
                        TexMappingDensity = 2.61f, // value not used
                        VertexCount = totalVertices,
                    });

                var nodeChunk = chunks.AddChunkBE(
                    ChunkType.Node,
                    0x823,
                    new NodeChunk {
                        MaterialId = pseudoMaterials.Last(x => x.Key.Name == node.MaterialName).Value,
                        Name = node.Name,
                        ObjectId = meshChunk.Header.Id,
                        ParentId = parent is null ? -1 : nodes[parent],
                        ChildCount = node.Children.Count,
                    });

                nodes[node] = nodeChunk.Header.Id;
            }
        }

        // (ChunkType.FoliageInfo, 1) => new FoliageInfoChunk(),

        chunks.WriteTo(writer);
    }

    public void WriteGeometryTo(Stream stream) => WriteGeometryTo(new NativeWriter(stream, Encoding.UTF8, true));

    public byte[] GetGeometryBytes() {
        var ms = new MemoryStream();
        WriteGeometryTo(ms);
        return ms.ToArray();
    }

    public bool HasMaterial => Material is not null;

    public void WriteMaterialTo(MemoryStream ms) {
        if (Material is null)
            throw new InvalidOperationException();
        PbxmlFile.FromObject(Material).WriteBinary(ms);
    }

    public byte[] GetMaterialBytes() {
        if (Material is null)
            throw new InvalidOperationException();
        var ms = new MemoryStream();
        WriteMaterialTo(ms);
        return ms.ToArray();
    }

    public static async Task<CryModel> FromCryEngineFiles(
        Func<string, CancellationToken, Task<Stream>> streamOpener,
        string geometryPath,
        string? materialPath,
        CancellationToken cancellationToken) {
        var geometryStream = await streamOpener(geometryPath, cancellationToken);
        var geometryChunks = CryChunks.FromStream(geometryStream);

        materialPath = materialPath?.Replace("\\", "/");
        if (materialPath?.EndsWith(".mtl", StringComparison.OrdinalIgnoreCase) is false)
            materialPath += ".mtl";

        if (geometryChunks.Values.All(x => x is not MtlNameChunk))
            return FromCryEngineFiles(geometryChunks, null);

        var embeddedMaterialPath = MtlNameChunk.FindChunkForMainMesh(geometryChunks).Name + ".mtl";

        Stream stream;
        try {
            if (materialPath is null)
                throw new FileNotFoundException();
            stream = await streamOpener(materialPath, cancellationToken);
        } catch (FileNotFoundException) {
            try {
                stream = await streamOpener(materialPath = embeddedMaterialPath, cancellationToken);
            } catch (FileNotFoundException) {
                try {
                    stream = await streamOpener(
                        materialPath = Path.Join(Path.GetDirectoryName(geometryPath), embeddedMaterialPath),
                        cancellationToken);
                } catch (FileNotFoundException) {
                    stream = await streamOpener(
                        materialPath = Path.Join(
                            Path.GetDirectoryName(geometryPath),
                            Path.GetFileName(embeddedMaterialPath)),
                        cancellationToken);
                }
            }
        }

        Material material;
        try {
            material = PbxmlFile.FromStream(stream).DeserializeAs<Material>();
        } catch (NotSupportedException e) {
            throw new NotSupportedException(materialPath + ": " + e.Message);
        }

        return FromCryEngineFiles(geometryChunks, material);
    }

    public static CryModel FromCryEngineFiles(CryChunks chunks, Material? material) {
        var exportFlags = chunks.Values.OfType<ExportFlagsChunk>().SingleOrDefault()?.Flags ?? 0;
        var cryModel = new CryModel(material) {
            MergeAllNodes = exportFlags.HasFlag(ExportFlags.MergeAllNodes),
            HaveAutoLods = exportFlags.HasFlag(ExportFlags.HaveAutoLods),
            UseCustomNormals = exportFlags.HasFlag(ExportFlags.UseCustomNormals)
        };

        if (chunks.Values.OfType<CompiledBonesChunk>().SingleOrDefault() is { } compiledBonesChunk) {
            // Test: Bones are simple enough
            var bones = compiledBonesChunk.Bones;
            foreach (var bone in bones) {
                var wtb = bone.LocalTransformMatrix.Transformation;
                NotSupportedIfFalse(
                    Matrix4x4.Invert(wtb, out var wtbi),
                    "LocalTransformMatrix is not invertible: {0}",
                    bone.Name);
                var btw = bone.WorldTransformMatrix.Transformation;
                var diff = wtbi - btw;
                foreach (var i in Enumerable.Range(0, 4))
                foreach (var j in Enumerable.Range(0, 4))
                    NotSupportedIfFalse(
                        Math.Abs(diff[i, j]) < 1e-4,
                        "WorldTransformMatrix is significantly different from inverted LocalTransformMatrix");
                NotSupportedIfFalse(bone.LimbId == uint.MaxValue, "LimbId is set: {0} in {1}", bone.LimbId, bone.Name);
                NotSupportedIfFalse(bone.Mass == 0, "Mass is set: {0}", bone.Mass);
                NotSupportedIfFalse(bone.PhysicsLive.IsDefault, "PhysicsLive is not default: {0}", bone.Name);
                NotSupportedIfFalse(bone.PhysicsDead.IsEmpty, "PhysicsDead is not empty: {0}", bone.Name);
            }

            cryModel.RootController = new(bones);

            // Test: PhysicalBones are ordered as expected and all items have the default value
            var bonesPhysical = chunks.Values.OfType<CompiledPhysicalBonesChunk>().Single().Bones;
            foreach (var b in bonesPhysical)
                NotSupportedIfFalse(b.Physics.IsDefault, "PhysicalBones is not default: {0:X08}", b.ControllerId);

            // bones: BFS
            // bonesPhysical: DFS
            var bonesPhysicalTest = new List<uint>();

            void TestBonesPhysical(in CompiledBone bone, int index) {
                bonesPhysicalTest.Add(bone.ControllerId);
                for (var i = index + bone.ChildOffset; i < index + bone.ChildOffset + bone.ChildCount; i++)
                    TestBonesPhysical(bones[i], i);
            }

            TestBonesPhysical(bones[0], 0);
            NotSupportedIfFalse(
                bonesPhysicalTest.SequenceEqual(bonesPhysical.Select(x => x.ControllerId)),
                "Stored PhysicalBones order does not match our ordering");
        }

        // Test: Ensure MtlNameChunks are what we expect
        var mtlNameChunks = chunks.Values.OfType<MtlNameChunk>().ToArray();
        var pseudoMaterials = new Dictionary<int, PseudoMaterial>();
        foreach (var chunk in mtlNameChunks) {
            NotSupportedIfFalse(
                chunk.AdvancedDataChunkId == 0,
                "MtlNameChunk.AdvancedDataChunkId is set: {0}",
                chunk.AdvancedDataChunkId);
            NotSupportedIfFalse(chunk.Flags2 == 0, "MtlNameChunk.Flags2 is set: {0}", chunk.Flags2);

            if (!pseudoMaterials.TryGetValue(chunk.Header.Id, out var mat))
                pseudoMaterials.Add(chunk.Header.Id, mat = new(chunk));

            foreach (var submatChunk in mtlNameChunks.Where(x => chunk.SubMaterialChunkIds.Contains(x.Header.Id))) {
                if (!pseudoMaterials.TryGetValue(chunk.Header.Id, out var submat))
                    pseudoMaterials.Add(chunk.Header.Id, submat = new(submatChunk));
                mat.Children.Add(submat);
            }

            var isMultiMat = chunk.Flags.HasFlag(MtlNameFlags.MultiMaterial);
            NotSupportedIfFalse(
                isMultiMat == chunk.SubMaterialChunkIds.Any(),
                "SubMaterialChunkIds is not empty but MultiMaterial is not set");
        }

        cryModel.PseudoMaterials.AddRange(
            pseudoMaterials
                .Where(x => pseudoMaterials.All(y => !y.Value.Children.Contains(x.Value)))
                .Select(x => x.Value));

        var nodeChunks = chunks.Values.OfType<NodeChunk>().ToArray();
        var nodes = new Dictionary<int, Node>();
        var orderedControllers = cryModel.RootController?.GetEnumeratorBreadthFirst().ToArray();
        foreach (var chunk in nodeChunks) {
            if (chunks[chunk.ObjectId] is not MeshChunk mc)
                continue;
            if (mc.SubsetsChunkId == 0)
                continue;

            if (!nodes.TryGetValue(chunk.Header.Id, out var node))
                nodes.Add(
                    chunk.Header.Id,
                    node = new(
                        chunks,
                        chunk,
                        material,
                        chunk.MaterialId == 0 ? null : pseudoMaterials[chunk.MaterialId].Name,
                        orderedControllers));

            if (chunk.ParentId != -1) {
                if (!nodes.TryGetValue(chunk.ParentId, out var parent)) {
                    var parentChunk = (NodeChunk) chunks[chunk.ParentId];
                    nodes.Add(
                        chunk.ParentId,
                        parent = new(
                            chunks,
                            parentChunk,
                            material,
                            chunk.MaterialId == 0 ? null : pseudoMaterials[chunk.MaterialId].Name,
                            orderedControllers));
                }

                parent.Children.Add(node);
            }
        }

        cryModel.Nodes.AddRange(
            nodes
                .Where(x => nodes.All(y => !y.Value.Children.Contains(x.Value)))
                .Select(x => x.Value));

        foreach (var ds in chunks.Values.OfType<DataChunk>())
            NotSupportedIfFalse(ds.Flags == 0, "DataChunk.Flags is nonzero: {0}", ds.Flags);

        foreach (var foliageInfo in chunks.Values.OfType<FoliageInfoChunk>()) {
            NotSupportedIfFalse(!foliageInfo.Spines.Any(), "FoliageInfo.Spines not supported");
            NotSupportedIfFalse(!foliageInfo.SpineVertices.Any(), "FoliageInfo.SpineVertices not supported");
            NotSupportedIfFalse(!foliageInfo.SpineVertexSegDim.Any(), "FoliageInfo.SpineVertexSegDim not supported");
            NotSupportedIfFalse(!foliageInfo.BoneMappings.Any(), "FoliageInfo.BoneMappings not supported");
            NotSupportedIfFalse(!foliageInfo.BoneIds.Any(), "FoliageInfo.BoneIds not supported");
        }

        // foreach (var subset in subsetsChunk.Subsets) {
        //     // Test: Center/Radius is correctly defined
        //     var aabb = new AaBb(Positions[Indices[subset.FirstIndexId]]);
        //     foreach (var index in Enumerable.Range(subset.FirstIndexId + 1, subset.FirstIndexId + subset.NumIndices))
        //         aabb.Expand(Positions[Indices[index]]);
        //     
        //     var diff = aabb.Center - subset.Center;
        //     NotSupportedIfFalse(Math.Abs(diff.X) < 1e-6);
        //     NotSupportedIfFalse(Math.Abs(diff.Y) < 1e-6);
        //     NotSupportedIfFalse(Math.Abs(diff.Z) < 1e-6);
        //     NotSupportedIfFalse(Math.Abs((aabb.Center - aabb.Min).Length() - subset.Radius) < 1e-6);
        // }
        return cryModel;
    }

    private static void NotSupportedIfFalse(bool test, string? message = null) {
        if (!test)
            throw new NotSupportedException(message);
    }

    private static void NotSupportedIfFalse(
        bool test,
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
        string message,
        params object?[] extra) {
        if (!test)
            throw new NotSupportedException(
                string.Format(message, extra.Select(y => (object?) (y is null ? "(null)" : y.ToString())).ToArray()));
    }

    private static T NotSupportedIfNull<T>(object? a, string? message = null) =>
        a is T x ? x : throw new NotSupportedException(message);

    private static T NotSupportedIfNull<T>(
        object? a,
        [StringSyntax(StringSyntaxAttribute.CompositeFormat)]
        string message,
        params object?[] extra) =>
        a is T x
            ? x
            : throw new NotSupportedException(
                string.Format(message, extra.Select(y => (object?) (y is null ? "(null)" : y.ToString())).ToArray()));
}
