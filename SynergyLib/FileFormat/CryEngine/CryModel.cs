using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Chunks;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Enums;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;
using SynergyLib.FileFormat.CryEngine.CryModelElements;
using SynergyLib.FileFormat.CryEngine.CryXml;
using SynergyLib.Util.MathExtras;

namespace SynergyLib.FileFormat.CryEngine;

public class CryModel {
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
        NotSupportedIfFalse(meshChunk.PhysicsDataChunkId1 == 0);
        NotSupportedIfFalse(meshChunk.PhysicsDataChunkId2 == 0);
        NotSupportedIfFalse(meshChunk.PhysicsDataChunkId3 == 0);

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
        PhysicsData = meshChunk.PhysicsDataChunkId0 == 0
            ? Array.Empty<byte[]>()
            : new[] {
                ((MeshPhysicsDataChunk) Chunks[meshChunk.PhysicsDataChunkId0]).Data
            };

        // Test: ShapeDeformation.Index are all set to 0xFF
        foreach (var sd in shapeDeformations)
            NotSupportedIfFalse(sd.Index.All(x => x == 0xFF));

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
                if (boneBox.AaBb != aabb)
                    throw new InvalidDataException();
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
                    //     .Where(x => x.x.FirstVertId <= index && index < x.x.FirstVertId + x.x.NumVerts)
                    //     .Select(x => x.i)
                    //     .ToArray();
                    // var bm = BoneMappings[index].BoneIds
                    //     .Select(x => subsetIndex.Select(y => subsetsChunk.BoneIds[y][x]).Distinct().Single())
                    //     .Select((x, i) => BoneMappings[index].Weights[i] == 0 ? (ushort)0 : x)
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

        Debugger.Break();
    }

    private static void NotSupportedIfFalse(bool test, string? message = null) {
        if (!test)
            throw new NotSupportedException(message);
    }

    private static T NotSupportedIfNull<T>(object? a, string? message = null) =>
        a is T x ? x : throw new NotSupportedException(message);
}
