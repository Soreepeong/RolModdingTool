using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Chunks;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Enums;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;
using SynergyLib.FileFormat.CryEngine.CryModelElements;
using SynergyLib.FileFormat.CryEngine.CryXml;
using SynergyLib.Util.BinaryRW;
using SynergyLib.Util.MathExtras;

namespace SynergyLib.FileFormat.CryEngine;

public class CryModel {
    public string Name;
    public CryChunks Chunks;
    public Material Material;
    public List<Controller> Controllers;
    public List<Mesh> Meshes = new();

    public byte[][] PhysicsData;

    public CryModel(Stream geom, Stream mtrl) {
        Material = PbxmlFile.FromStream(mtrl).DeserializeAs<Material>();
        Chunks = CryChunks.FromStream(geom);

        // Test: Ensure exportFlagsChunk is what we know
        NotSupportedIfFalse(Chunks.Values.OfType<ExportFlagsChunk>().Single().Flags == ExportFlags.UseCustomNormals);

        var nodeChunk = Chunks.Values.OfType<NodeChunk>().Single(x => x.ParentId == -1);
        Name = nodeChunk.Name;
        NotSupportedIfFalse(!nodeChunk.IsGroupHead);
        NotSupportedIfFalse(!nodeChunk.IsGroupMember);
        NotSupportedIfFalse(nodeChunk.Properties.Length == 0);
        NotSupportedIfFalse(nodeChunk.Position == Vector3.Zero);
        NotSupportedIfFalse((nodeChunk.Rotation - Quaternion.Identity).Length() < 1e-6);
        NotSupportedIfFalse(nodeChunk.Scale == Vector3.One);
        NotSupportedIfFalse(nodeChunk.Transform.M44 == 0 && (nodeChunk.Transform with {M44 = 1}).IsIdentity);
        NotSupportedIfFalse(nodeChunk.ChildCount == 0);
        NotSupportedIfFalse(Chunks[nodeChunk.MaterialId] is MtlNameChunk);
        var meshChunk = NotSupportedIfNull<MeshChunk>(Chunks[nodeChunk.ObjectId]);
        var subsetsChunk = NotSupportedIfNull<MeshSubsetsChunk>(Chunks[meshChunk.SubsetsChunkId]);

        // Test: Ensure MtlNameChunks are what we expect
        var mtlNameChunks = Chunks.Values.OfType<MtlNameChunk>().ToArray();
        foreach (var chunk in mtlNameChunks) {
            NotSupportedIfFalse(chunk.AdvancedDataChunkId == 0); // not implemented by us
            NotSupportedIfFalse(chunk.PhysicsType == MtlNamePhysicsType.None); // not implemented by us
            NotSupportedIfFalse(chunk.Flags2 == 0); // not implemented by CryEngine
            if (chunk.Header.Id == nodeChunk.MaterialId) {
                NotSupportedIfFalse(chunk.Flags == MtlNameFlags.MultiMaterial);
                NotSupportedIfFalse(Equals(chunk.ShOpacity, 1f)); // not implemented by us
                NotSupportedIfFalse(chunk.SubMaterialChunkIds.Count(x => x != 0) == mtlNameChunks.Length - 1);
            } else {
                NotSupportedIfFalse(chunk.Flags == MtlNameFlags.SubMaterial);
                NotSupportedIfFalse(Equals(chunk.ShOpacity, 0f)); // not implemented by us
                NotSupportedIfFalse(!chunk.SubMaterialChunkIds.Any());
            }
        }

        // Test: Ensure consistency in bone existence
        var hasBones = meshChunk.BoneMappingChunkId != 0;
        NotSupportedIfFalse(hasBones == subsetsChunk.Flags.HasFlag(MeshSubsetsFlags.BoneIndices));

        foreach (var ds in Chunks.Values.OfType<DataChunk>())
            NotSupportedIfFalse(ds.Flags == 0);

        // Test: Ensure that there isn't anything we don't care defined
        NotSupportedIfFalse(meshChunk.VertAnimId == 0);
        NotSupportedIfFalse(meshChunk.Colors2ChunkId == 0);
        NotSupportedIfFalse(meshChunk.ShCoeffsChunkId == 0);
        NotSupportedIfFalse(meshChunk.FaceMapChunkId == 0);
        NotSupportedIfFalse(meshChunk.VertMatsChunkId == 0);
        NotSupportedIfFalse(meshChunk.QTangentsChunkId == 0);
        NotSupportedIfFalse(meshChunk.SkinDataChunkId == 0);
        NotSupportedIfFalse(meshChunk.Ps3EdgeDataChunkId == 0);
        NotSupportedIfFalse(meshChunk.Reserved15ChunkId == 0);
        NotSupportedIfFalse(meshChunk.PhysicsDataChunkId[1] == 0);
        NotSupportedIfFalse(meshChunk.PhysicsDataChunkId[2] == 0);
        NotSupportedIfFalse(meshChunk.PhysicsDataChunkId[3] == 0);

        var vertices = new Vertex[meshChunk.VertexCount];
        foreach (var i in Enumerable.Range(0, meshChunk.VertexCount)) {
            vertices[i] = new() {
                Position = ((DataChunk) Chunks[meshChunk.PositionsChunkId]).GetItem<Vector3>(i),
                Normal = ((DataChunk) Chunks[meshChunk.NormalsChunkId]).GetItem<Vector3>(i),
                TexCoord = ((DataChunk) Chunks[meshChunk.TexCoordsChunkId]).GetItem<Vector2>(i),
                Color = ((DataChunk) Chunks[meshChunk.ColorsChunkId]).GetItem<Vector4<byte>>(i),
                Tangent = ((DataChunk) Chunks[meshChunk.TangentsChunkId]).GetItem<MeshTangent>(i),
            };
        }

        var shapeDeformations = meshChunk.ShapeDeformationChunkId == 0
            ? Array.Empty<MeshShapeDeformation>()
            : ((DataChunk) Chunks[meshChunk.ShapeDeformationChunkId]).AsArray<MeshShapeDeformation>();
        var boneMappings = !hasBones
            ? Array.Empty<MeshBoneMapping>()
            : ((DataChunk) Chunks[meshChunk.BoneMappingChunkId]).AsArray<MeshBoneMapping>();

        var indices = ((DataChunk) Chunks[meshChunk.IndicesChunkId]).AsArray<ushort>();
        PhysicsData = meshChunk.PhysicsDataChunkId[0] == 0
            ? Array.Empty<byte[]>()
            : new[] {
                ((MeshPhysicsDataChunk) Chunks[meshChunk.PhysicsDataChunkId[0]]).Data
            };

        // Test: ShapeDeformation.Index are all set to 0xFF
        foreach (var sd in shapeDeformations)
            NotSupportedIfFalse(sd.Index.All(x => x is 0xFF or 1));

        // Test: Ensure subset counts match
        NotSupportedIfFalse(subsetsChunk.Subsets.Count == meshChunk.SubsetsCount);

        if (hasBones) {
            // Test: Bones are simple enough
            var bones = Chunks.Values.OfType<CompiledBonesChunk>().Single().Bones;
            foreach (var bone in bones) {
                var wtb = bone.LocalTransformMatrix.Transformation;
                NotSupportedIfFalse(Matrix4x4.Invert(wtb, out var wtbi));
                var btw = bone.WorldTransformMatrix.Transformation;
                var diff = wtbi - btw;
                foreach (var i in Enumerable.Range(0, 4))
                foreach (var j in Enumerable.Range(0, 4))
                    NotSupportedIfFalse(Math.Abs(diff[i, j]) < 1e-6);
                NotSupportedIfFalse(bone.LimbId == uint.MaxValue);
                NotSupportedIfFalse(bone.Mass == 0);
                NotSupportedIfFalse(bone.PhysicsLive.IsDefault);
                NotSupportedIfFalse(bone.PhysicsDead.IsEmpty);
            }

            Controllers = Controller.ListFromCompiledBones(bones);

            // Test: PhysicalBones are ordered as expected and all items have the default value
            var bonesPhysical = Chunks.Values.OfType<CompiledPhysicalBonesChunk>().Single().Bones;
            NotSupportedIfFalse(bonesPhysical.Select(x => x.Physics.IsDefault).All(x => x));

            // bones: BFS
            // bonesPhysical: DFS
            var bonesPhysicalTest = new List<uint>();

            void TestBonesPhysical(in CompiledBone bone, int index) {
                bonesPhysicalTest.Add(bone.ControllerId);
                for (var i = index + bone.ChildOffset; i < index + bone.ChildOffset + bone.ChildCount; i++)
                    TestBonesPhysical(bones[i], i);
            }

            TestBonesPhysical(bones[0], 0);
            NotSupportedIfFalse(bonesPhysicalTest.SequenceEqual(bonesPhysical.Select(x => x.ControllerId)));

            if (Chunks.Values.OfType<CompiledPhysicalProxyChunk>().Single().Proxies.Any())
                throw new NotSupportedException();
            if (Chunks.Values.OfType<CompiledMorphTargetsChunk>().Single().Targets.Any())
                throw new NotSupportedException();
            var intSkinVertices = Chunks.Values.OfType<CompiledIntSkinVerticesChunk>().Single();
            var intFaces = Chunks.Values.OfType<CompiledIntFacesChunk>().Single();
            var extToInt = Chunks.Values.OfType<CompiledExtToIntMapChunk>().Single();

            NotSupportedIfFalse(extToInt.Map.Count == meshChunk.VertexCount);
            NotSupportedIfFalse(intSkinVertices.Vertices.Count - 1 == extToInt.Map.Max());
            NotSupportedIfFalse(
                intSkinVertices.Vertices.Count - 1 ==
                intFaces.Faces.Max(x => Math.Max(Math.Max(x.Vertex0, x.Vertex1), x.Vertex2)));
            NotSupportedIfFalse(intFaces.Faces.Count * 3 == meshChunk.IndexCount);
            var boneBoxChunks = Chunks.Values.OfType<BonesBoxesChunk>().ToArray();

            // some bones might have no directly attached vertices
            NotSupportedIfFalse(boneBoxChunks.Length <= Controllers.Count);

            foreach (var boneBox in boneBoxChunks) {
                foreach (var extIndex in Enumerable.Range(0, extToInt.Map.Count)) {
                    var intIndex = extToInt.Map[extIndex];
                    var iv = intSkinVertices.Vertices[intIndex].BoneIds;
                    if (boneBox.Indices.Contains((ushort) extIndex)) {
                        if (!iv.Contains((ushort) boneBox.BoneId))
                            throw new InvalidDataException();
                    } else {
                        if (iv.Contains((ushort) boneBox.BoneId))
                            throw new InvalidDataException();
                    }
                }

                var aabb = AaBb.FromVectorEnumerable(boneBox.Indices.Select(x => vertices[x].Position).ToArray());
                NotSupportedIfFalse(Math.Abs(boneBox.AaBb.Min.X - aabb.Min.X) < 1e-3);
                NotSupportedIfFalse(Math.Abs(boneBox.AaBb.Min.Y - aabb.Min.Y) < 1e-3);
                NotSupportedIfFalse(Math.Abs(boneBox.AaBb.Min.Z - aabb.Min.Z) < 1e-3);
                NotSupportedIfFalse(Math.Abs(boneBox.AaBb.Max.X - aabb.Max.X) < 1e-3);
                NotSupportedIfFalse(Math.Abs(boneBox.AaBb.Max.Y - aabb.Max.Y) < 1e-3);
                NotSupportedIfFalse(Math.Abs(boneBox.AaBb.Max.Z - aabb.Max.Z) < 1e-3);
                foreach (var index in boneBox.Indices) {
                    var bw = intSkinVertices.Vertices[extToInt.Map[index]].Weights;
                    NotSupportedIfFalse(Math.Abs(bw.Sum(x => x) - 1f) < 1e-6);
                    var bw2 = boneMappings[index].Weights;
                    NotSupportedIfFalse(bw2.Sum(x => x) == 0xFF);
                    foreach (var (a, b) in bw.Zip(bw2))
                        NotSupportedIfFalse(Math.Abs(a - b / 255f) < 2f / 255f);

                    vertices[index].Weights = bw;
                    vertices[index].ControllerIds = new(
                        bw.Zip(intSkinVertices.Vertices[extToInt.Map[index]].BoneIds)
                            .Select(x => x.First == 0f ? 0u : bones[x.Second].ControllerId)
                    );

                    // var iv = intSkinVertices.Vertices[extToInt.Map[index]].BoneIds;
                    // var subsetIndex = subsetsChunk.Subsets.Select((x, i) => (x, i))
                    //     .Where(x => Enumerable.Range(x.x.FirstIndexId, x.x.NumIndices).Any(y => indices[y] == index))
                    //     .Select(x => x.i)
                    //     .ToArray();
                    // var bm = boneMappings[index].BoneIds
                    //     .Select(x => subsetIndex.Select(y => subsetsChunk.BoneIds[y][x]).Distinct().Single())
                    //     .Select((x, i) => boneMappings[index].Weights[i] == 0 ? (ushort) 0 : x)
                    //     .ToArray();
                    // if (!bm.SequenceEqual(iv))
                    //     throw new InvalidDataException();
                }
            }

            foreach (var intIndex in Enumerable.Range(0, intSkinVertices.Vertices.Count)) {
                NotSupportedIfFalse(
                    extToInt.Map.Select((x, i) => (x, i))
                        .Where(x => x.x == intIndex)
                        .Select(x => vertices[x.i].Position)
                        .Distinct()
                        .Count() == 1);
            }

            var ind1 = indices.Chunk(3).Select(x => x.Select(y => extToInt.Map[y]).ToArray())
                .Select(x => new CompiledIntFace(x[0], x[1], x[2])).Order().ToArray();
            var ind2 = intFaces.Faces.Order().ToArray();
            NotSupportedIfFalse(ind1.SequenceEqual(ind2));
        } else
            Controllers = new();

        for (var i = 0; i < subsetsChunk.Subsets.Count;) {
            var to = i + 1;
            while (to < subsetsChunk.Subsets.Count
                   && subsetsChunk.Subsets[to].MatId == subsetsChunk.Subsets[i].MatId)
                to++;
            for (var j = i; j < to - 1; j++) {
                var expectedEndIndexId = i == subsetsChunk.Subsets.Count
                    ? indices.Length
                    : subsetsChunk.Subsets[i + 1].FirstIndexId;
                NotSupportedIfFalse(
                    subsetsChunk.Subsets[i].FirstIndexId + subsetsChunk.Subsets[i].NumIndices == expectedEndIndexId);
            }

            var vertexIdFrom =
                checked((ushort) Enumerable.Range(i, to - i).Min(x => subsetsChunk.Subsets[x].FirstVertId));
            var vertexIdTo = checked((ushort) Enumerable.Range(i, to - i)
                .Max(x => subsetsChunk.Subsets[x].FirstVertId + subsetsChunk.Subsets[x].NumVerts));
            var indexIdFrom = checked((ushort) subsetsChunk.Subsets[i].FirstIndexId);
            var indexIdTo = checked((ushort) subsetsChunk.Subsets[to - 1].FirstIndexId +
                subsetsChunk.Subsets[to - 1].NumIndices);

            // Test: Ensure corresponding material is defined in the mtl file
            var matName = Material.SubMaterials![subsetsChunk.Subsets[i].MatId].Name!;
            NotSupportedIfFalse(mtlNameChunks.Any(x => x.Name == matName));

            var slicedIndices = new ushort[indexIdTo - indexIdFrom];
            for (var j = indexIdFrom; j < indexIdTo; j++)
                slicedIndices[j - indexIdFrom] = checked((ushort) (indices[j] - vertexIdFrom));

            Meshes.Add(new(matName, vertices[vertexIdFrom..vertexIdTo], slicedIndices));
            i = to;
        }

        foreach (var foliageInfo in Chunks.Values.OfType<FoliageInfoChunk>()) {
            NotSupportedIfFalse(!foliageInfo.Spines.Any());
            NotSupportedIfFalse(!foliageInfo.SpineVertices.Any());
            NotSupportedIfFalse(!foliageInfo.SpineVertexSegDim.Any());
            NotSupportedIfFalse(!foliageInfo.BoneMappings.Any());
            NotSupportedIfFalse(!foliageInfo.BoneIds.Any());
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
    }

    public void WriteGeometryTo(NativeWriter writer) {
        var chunks = new CryChunks {
            Type = CryFileType.Geometry,
            Version = CryFileVersion.CryTek3
        };

        var rootMaterialChunk = chunks.AddChunkLE(
            ChunkType.MtlName,
            0x800,
            new MtlNameChunk {
                Flags = Meshes.Any() ? MtlNameFlags.MultiMaterial : 0,
                Name = Name,
                SubMaterialChunkIds = new(Meshes.Count),
            });

        foreach (var mesh in Meshes) {
            rootMaterialChunk.SubMaterialChunkIds.Add(
                chunks.AddChunkLE(
                    ChunkType.MtlName,
                    0x800,
                    new MtlNameChunk {
                        Flags = MtlNameFlags.SubMaterial,
                        Name = mesh.MaterialName,
                        ShOpacity = 0f,
                    }).Header.Id);
        }

        Dictionary<uint, ushort>? controllerIdToBoneId = null;
        List<ushort>? extToInt = null;
        CompiledIntSkinVerticesChunk? intSkinVertices = null;
        if (Controllers.Any()) {
            var compiledBones = chunks.AddChunkBE(
                ChunkType.CompiledBones,
                0x800,
                new CompiledBonesChunk {Bones = Controller.ToCompiledBonesList(Controllers)});

            controllerIdToBoneId = compiledBones.Bones
                .Select((x, i) => (x, i))
                .ToDictionary(x => x.x.ControllerId, x => (ushort) x.i);

            chunks.AddChunkBE(
                ChunkType.CompiledPhysicalBones,
                0x800,
                new CompiledPhysicalBonesChunk {Bones = Controller.ToBoneEntityList(Controllers)});

            chunks.AddChunkBE(ChunkType.CompiledPhysicalProxies, 0x800, new CompiledPhysicalProxyChunk());

            chunks.AddChunkBE(ChunkType.CompiledMorphTargets, 0x800, new CompiledMorphTargetsChunk());

            intSkinVertices = chunks.AddChunkBE(
                ChunkType.CompiledIntSkinVertices,
                0x800,
                new CompiledIntSkinVerticesChunk {
                    Vertices = Mesh.ToIntSkinVertices(Meshes, controllerIdToBoneId, out extToInt),
                });

            chunks.AddChunkBE(
                ChunkType.CompiledIntFaces,
                0x800,
                new CompiledIntFacesChunk {Faces = Mesh.ToIntFaces(Meshes, extToInt)});

            chunks.AddChunkBE(
                ChunkType.CompiledExt2IntMap,
                0x800,
                new CompiledExtToIntMapChunk {Map = extToInt});

            foreach (var controller in Controllers) {
                var boneId = controllerIdToBoneId[controller.Id];
                var boneVertexIndices = new List<ushort>();
                var aabb = AaBb.NegativeExtreme;
                foreach (var extIndex in Enumerable.Range(0, extToInt.Count)) {
                    var intIndex = extToInt[extIndex];
                    if (!intSkinVertices.Vertices[intIndex].BoneIds.Contains(boneId))
                        continue;

                    boneVertexIndices.Add((ushort) extIndex);
                    aabb.Expand(intSkinVertices.Vertices[intIndex].Position1);
                }

                if (!boneVertexIndices.Any())
                    continue;

                chunks.AddChunkBE(
                    ChunkType.BonesBoxes,
                    0x801,
                    new BonesBoxesChunk {BoneId = boneId, Indices = boneVertexIndices, AaBb = aabb});
            }
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

        var meshSubsetsChunk = chunks.AddChunkBE(
            ChunkType.MeshSubsets,
            0x800,
            new MeshSubsetsChunk {
                BoneIds = new(),
                Flags = controllerIdToBoneId is null ? 0 : MeshSubsetsFlags.BoneIndices,
                Subsets = new(),
            });

        var totalVertices = Meshes.Sum(x => x.Vertices.Length);
        var positionsChunk = chunks.AddChunkBE(
            ChunkType.DataStream,
            0x800,
            new DataChunk {Type = CgfStreamType.Positions});
        var normalsChunk = chunks.AddChunkBE(ChunkType.DataStream, 0x800, new DataChunk {Type = CgfStreamType.Normals});
        var texCoordsChunk = chunks.AddChunkBE(
            ChunkType.DataStream,
            0x800,
            new DataChunk {Type = CgfStreamType.TexCoords});
        var colorsChunk = chunks.AddChunkBE(ChunkType.DataStream, 0x800, new DataChunk {Type = CgfStreamType.Colors});
        var tangentsChunk = chunks.AddChunkBE(
            ChunkType.DataStream,
            0x800,
            new DataChunk {Type = CgfStreamType.Tangents});
        positionsChunk.FromEnumerable(Meshes.SelectMany(x => x.Vertices.Select(y => y.Position)), totalVertices);
        normalsChunk.FromEnumerable(Meshes.SelectMany(x => x.Vertices.Select(y => y.Normal)), totalVertices);
        texCoordsChunk.FromEnumerable(Meshes.SelectMany(x => x.Vertices.Select(y => y.TexCoord)), totalVertices);
        colorsChunk.FromEnumerable(Meshes.SelectMany(x => x.Vertices.Select(y => y.Color)), totalVertices);
        tangentsChunk.FromEnumerable(Meshes.SelectMany(x => x.Vertices.Select(y => y.Tangent)), totalVertices);

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

        var totalIndices = Meshes.Sum(x => x.Indices.Length);
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
            foreach (var mesh in Meshes) {
                for (var i = 0; i < mesh.Indices.Length; i++)
                    indicesChunk.SetItem(baseIndexIndex + i, (ushort) (baseVertexIndex + mesh.Indices[i]));

                var matId = Material.SubMaterials!
                    .Select((x, i) => (x, i))
                    .Single(x => x.x.Name == mesh.MaterialName)
                    .i;
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
                Bbox = AaBb.FromVectorEnumerable(Meshes.SelectMany(x => x.Vertices.Select(y => y.Position))),
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

        chunks.AddChunkBE(
            ChunkType.Node,
            0x823,
            new NodeChunk {
                MaterialId = rootMaterialChunk.Header.Id,
                Name = Name,
                ObjectId = meshChunk.Header.Id,
                ParentId = -1,
            });

        // (ChunkType.FoliageInfo, 1) => new FoliageInfoChunk(),

        chunks.WriteTo(writer);
    }

    public void WriteGeometryTo(Stream stream) => WriteGeometryTo(new NativeWriter(stream, Encoding.UTF8, true));
    
    public byte[] GetGeometryBytes() {
        var ms = new MemoryStream();
        WriteGeometryTo(ms);
        return ms.ToArray();
    }

    public void WriteMaterialTo(MemoryStream ms) => PbxmlFile.FromObject(Material).WriteBinary(ms);

    public byte[] GetMaterialBytes() {
        var ms = new MemoryStream();
        WriteMaterialTo(ms);
        return ms.ToArray();
    }

    private static void NotSupportedIfFalse(bool test, string? message = null) {
        if (!test)
            throw new NotSupportedException(message);
    }

    private static T NotSupportedIfNull<T>(object? a, string? message = null) =>
        a is T x ? x : throw new NotSupportedException(message);
}
