using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Numerics;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Chunks;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Enums;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;
using SynergyLib.FileFormat.CryEngine.CryXml;
using SynergyLib.Util.MathExtras;

namespace SynergyLib.FileFormat.CryEngine.CryModelElements;

public class Node {
    public readonly List<Mesh> Meshes = new();
    public readonly List<Node> Children = new();
    public string Name;
    public string? MaterialName;
    public bool HasColors;
    public bool DoNotMerge;
    public Vector3 Position = Vector3.Zero;
    public Quaternion Rotation = Quaternion.Identity;
    public Vector3 Scale = Vector3.One;

    public Node(string name, string? material) {
        Name = name;
        MaterialName = material;
    }
    
    public Node(
        CryChunks chunks,
        NodeChunk nodeChunk,
        Material? material,
        string? materialName,
        IReadOnlyList<Controller>? controllers) {
        Name = nodeChunk.Name;
        MaterialName = materialName;

        var properties = nodeChunk.Properties.Split(
                "\n",
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => x.Split("=", 2))
            .ToDictionary(x => x[0], x => x[1]);
        properties.Remove("fileType");
        DoNotMerge = properties.GetValueOrDefault("DoNotMerge") == "1";
        var writeVertexColours = properties.GetValueOrDefault("WriteVertexColours") == "1";
        properties.Remove("DoNotMerge");
        properties.Remove("WriteVertexColours");
        NotSupportedIfFalse(
            !properties.Any(),
            "NodeChunk.Property value has unknown item(s): {0}",
            string.Join(", ", properties.Select(x => $"{x.Key}={x.Value}")));

        NotSupportedIfFalse(
            !nodeChunk.IsGroupHead,
            "NodeChunk.IsGroupHead is set");
        NotSupportedIfFalse(
            !nodeChunk.IsGroupMember,
            "NodeChunk.IsGroupMember is set");
        Position = nodeChunk.Position / 100f;
        Rotation = nodeChunk.Rotation;
        Scale = nodeChunk.Scale;
        var meshChunk = NotSupportedIfNull<MeshChunk>(
            chunks.GetValueOrDefault(nodeChunk.ObjectId),
            "NodeChunk.ObjectId points to {0}",
            chunks.GetValueOrDefault(nodeChunk.ObjectId));
        var materialChunk = nodeChunk.MaterialId == 0
            ? null
            : NotSupportedIfNull<MtlNameChunk>(
                chunks.GetValueOrDefault(nodeChunk.MaterialId),
                "NodeChunk.MaterialId points to {0}",
                chunks.GetValueOrDefault(nodeChunk.MaterialId));
        var subsetsChunk = NotSupportedIfNull<MeshSubsetsChunk>(
            chunks.GetValueOrDefault(meshChunk.SubsetsChunkId),
            "MeshChunk.SubsetsChunkId points to {0}",
            chunks.GetValueOrDefault(meshChunk.SubsetsChunkId));

        // Test: Ensure consistency in bone existence
        var hasBones = meshChunk.BoneMappingChunkId != 0;
        var hasBonesTest = subsetsChunk.Flags.HasFlag(MeshSubsetsFlags.BoneIndices);
        NotSupportedIfFalse(
            hasBones == hasBonesTest,
            "meshChunk.BoneMappingChunkId({0}) is inconsistent with MeshSubsetFlags.BoneIndices({1})",
            meshChunk.BoneMappingChunkId,
            hasBonesTest);

        // Test: Ensure that there isn't anything we don't care defined
        NotSupportedIfFalse(meshChunk.VertAnimId == 0, "MeshChunk.VertAnimId is nonzero: {0}", meshChunk.VertAnimId);
        NotSupportedIfFalse(
            meshChunk.Colors2ChunkId == 0,
            "MeshChunk.Colors2ChunkId is nonzero: {0}",
            meshChunk.Colors2ChunkId);
        NotSupportedIfFalse(
            meshChunk.ShCoeffsChunkId == 0,
            "MeshChunk.ShCoeffsChunkId is nonzero: {0}",
            meshChunk.ShCoeffsChunkId);
        NotSupportedIfFalse(
            meshChunk.FaceMapChunkId == 0,
            "MeshChunk.FaceMapChunkId is nonzero: {0}",
            meshChunk.FaceMapChunkId);
        NotSupportedIfFalse(
            meshChunk.VertMatsChunkId == 0,
            "MeshChunk.VertMatsChunkId is nonzero: {0}",
            meshChunk.VertMatsChunkId);
        NotSupportedIfFalse(
            meshChunk.QTangentsChunkId == 0,
            "MeshChunk.QTangentsChunkId is nonzero: {0}",
            meshChunk.QTangentsChunkId);
        NotSupportedIfFalse(
            meshChunk.SkinDataChunkId == 0,
            "MeshChunk.SkinDataChunkId is nonzero: {0}",
            meshChunk.SkinDataChunkId);
        NotSupportedIfFalse(
            meshChunk.Ps3EdgeDataChunkId == 0,
            "MeshChunk.Ps3EdgeDataChunkId is nonzero: {0}",
            meshChunk.Ps3EdgeDataChunkId);
        NotSupportedIfFalse(
            meshChunk.Reserved15ChunkId == 0,
            "MeshChunk.Reserved15ChunkId is nonzero: {0}",
            meshChunk.Reserved15ChunkId);

        var vertices = new Vertex[meshChunk.VertexCount];
        foreach (var i in Enumerable.Range(0, meshChunk.VertexCount)) {
            vertices[i] = new() {
                Position = ((DataChunk) chunks[meshChunk.PositionsChunkId]).GetItem<Vector3>(i),
                Normal = ((DataChunk) chunks[meshChunk.NormalsChunkId]).GetItem<Vector3>(i),
                TexCoord = ((DataChunk) chunks[meshChunk.TexCoordsChunkId]).GetItem<Vector2>(i),
                Tangent = ((DataChunk) chunks[meshChunk.TangentsChunkId]).GetItem<MeshTangent>(i),
            };
        }

        HasColors = meshChunk.ColorsChunkId != 0;
        if (HasColors)
            foreach (var i in Enumerable.Range(0, meshChunk.VertexCount))
                vertices[i].Color = ((DataChunk) chunks[meshChunk.ColorsChunkId]).GetItem<Vector4<byte>>(i);
        else if (writeVertexColours)
            throw new InvalidDataException("WriteVertexColours=1 but ColorsChunkId=0");
        properties.Remove("WriteVertexColors");

        // TODO: figure this out if necessary
        // var shapeDeformations = meshChunk.ShapeDeformationChunkId == 0
        //     ? Array.Empty<MeshShapeDeformation>()
        //     : ((DataChunk) chunks[meshChunk.ShapeDeformationChunkId]).AsArray<MeshShapeDeformation>();
        // var physicsData = meshChunk.PhysicsDataChunkId[0] == 0
        //     ? Array.Empty<byte[]>()
        //     : new[] {
        //         ((MeshPhysicsDataChunk) Chunks[meshChunk.PhysicsDataChunkId[0]]).Data
        //     };

        var boneMappings = !hasBones
            ? Array.Empty<MeshBoneMapping>()
            : ((DataChunk) chunks[meshChunk.BoneMappingChunkId]).AsArray<MeshBoneMapping>();

        var indices = ((DataChunk) chunks[meshChunk.IndicesChunkId]).AsArray<ushort>();

        // Test: Ensure subset counts match
        NotSupportedIfFalse(
            subsetsChunk.Subsets.Count == meshChunk.SubsetsCount,
            "SubsetsChunk.Subsets.Count({0}) != MeshChunk.SubsetsCount({1})",
            subsetsChunk.Subsets.Count,
            meshChunk.SubsetsCount);

        // if (!useCustomNormals) {
        //     for (var i = 0; i < indices.Length; i += 3) {
        //         var v1 = vertices[indices[i + 0]].Position - vertices[indices[i + 1]].Position;
        //         var v2 = vertices[indices[i + 0]].Position - vertices[indices[i + 2]].Position;
        //         var n = Vector3.Normalize(Vector3.Cross(v1, v2));
        //         vertices[indices[i + 0]].Normal = n;
        //         vertices[indices[i + 1]].Normal = n;
        //         vertices[indices[i + 2]].Normal = n;
        //     }
        // }

        if (controllers?.Any() is true) {
            if (chunks.Values.OfType<CompiledPhysicalProxyChunk>().Single().Proxies.Any())
                throw new NotSupportedException("CompiledPhysicalProxyChunk is not empty");
            if (chunks.Values.OfType<CompiledMorphTargetsChunk>().Single().Targets.Any())
                throw new NotSupportedException("CompiledMorphTargetsChunk is not empty");
            var intSkinVertices = chunks.Values.OfType<CompiledIntSkinVerticesChunk>().Single();
            var intFaces = chunks.Values.OfType<CompiledIntFacesChunk>().Single();
            var extToInt = chunks.Values.OfType<CompiledExtToIntMapChunk>().Single();

            NotSupportedIfFalse(
                extToInt.Map.Count == meshChunk.VertexCount,
                "ExtToInt.Map.Count != MeshChunk.VertexCount");
            NotSupportedIfFalse(
                intSkinVertices.Vertices.Count - 1 == extToInt.Map.Max(),
                "IntSkinVertices.Vertices.Count - 1 != extToInt.Map.Max()");
            NotSupportedIfFalse(
                intSkinVertices.Vertices.Count - 1 ==
                intFaces.Faces.Max(x => Math.Max(Math.Max(x.Vertex0, x.Vertex1), x.Vertex2)),
                "IntSkinVertices.Vertices.Count - 1 != Max(Faces.Vertices)");
            NotSupportedIfFalse(
                intFaces.Faces.Count * 3 == meshChunk.IndexCount,
                "IntFaces.Faces.Count * 3 != MeshChunk.IndexCount");
            var boneBoxChunks = chunks.Values.OfType<BonesBoxesChunk>().ToArray();

            // some bones might have no directly attached vertices
            NotSupportedIfFalse(
                boneBoxChunks.Length <= controllers.Count,
                "BoneBoxesChunks.Length({0}) > Controllers.Length{1}",
                boneBoxChunks.Length,
                controllers.Count);

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

                var aabb = AaBb.FromEnumerable(boneBox.Indices.Select(x => vertices[x].Position).ToArray());
                NotSupportedIfFalse(Math.Abs(boneBox.AaBb.Min.X - aabb.Min.X) < 1e-3, "BoneBox.AaBb");
                NotSupportedIfFalse(Math.Abs(boneBox.AaBb.Min.Y - aabb.Min.Y) < 1e-3, "BoneBox.AaBb");
                NotSupportedIfFalse(Math.Abs(boneBox.AaBb.Min.Z - aabb.Min.Z) < 1e-3, "BoneBox.AaBb");
                NotSupportedIfFalse(Math.Abs(boneBox.AaBb.Max.X - aabb.Max.X) < 1e-3, "BoneBox.AaBb");
                NotSupportedIfFalse(Math.Abs(boneBox.AaBb.Max.Y - aabb.Max.Y) < 1e-3, "BoneBox.AaBb");
                NotSupportedIfFalse(Math.Abs(boneBox.AaBb.Max.Z - aabb.Max.Z) < 1e-3, "BoneBox.AaBb");
                foreach (var index in boneBox.Indices) {
                    var bw = intSkinVertices.Vertices[extToInt.Map[index]].Weights;
                    NotSupportedIfFalse(Math.Abs(bw.Sum(x => x) - 1f) < 1e-6, "Sum(IntSkinVertex.Weights) != 1");
                    var bw2 = boneMappings[index].Weights;
                    NotSupportedIfFalse(bw2.Sum(x => x) == 0xFF, "Sum(BoneMapping.Weights) != 255");
                    foreach (var (a, b) in bw.Zip(bw2))
                        NotSupportedIfFalse(
                            Math.Abs(a - b / 255f) < 2f / 255f,
                            "IntSkinVertex.Weights != BoneMapping.Weights");

                    vertices[index].Weights = bw;
                    vertices[index].ControllerIds = new(
                        bw.Zip(intSkinVertices.Vertices[extToInt.Map[index]].BoneIds)
                            .Select(x => x.First == 0f ? 0u : controllers[x.Second].Id));

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
                        .Count() == 1,
                    "One InternalVertex maps to multiple ExternalVertices with different positions");
            }

            var ind1 = indices.Chunk(3).Select(x => x.Select(y => extToInt.Map[y]).ToArray())
                .Select(x => new CompiledIntFace(x[0], x[1], x[2])).Order().ToArray();
            var ind2 = intFaces.Faces.Order().ToArray();
            NotSupportedIfFalse(ind1.SequenceEqual(ind2), "CompiledIntFaces does not match Indices");
        }

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
                    subsetsChunk.Subsets[i].FirstIndexId + subsetsChunk.Subsets[i].NumIndices == expectedEndIndexId,
                    "Subsets are not continuous");
            }

            var vertexIdFrom =
                checked((ushort) Enumerable.Range(i, to - i).Min(x => subsetsChunk.Subsets[x].FirstVertId));
            var vertexIdTo = checked((ushort) Enumerable.Range(i, to - i)
                .Max(x => subsetsChunk.Subsets[x].FirstVertId + subsetsChunk.Subsets[x].NumVerts));
            var indexIdFrom = subsetsChunk.Subsets[i].FirstIndexId;
            var indexIdTo = subsetsChunk.Subsets[to - 1].FirstIndexId + subsetsChunk.Subsets[to - 1].NumIndices;

            // I have no idea how materials are supposed to work
            string? matName;
            if (material?.SubMaterials?.Any() is true) {
                if (subsetsChunk.Subsets[i].MatId >= material.SubMaterialsAndRefs!.Count)
                    matName = null;
                else
                    matName = (material.SubMaterialsAndRefs![subsetsChunk.Subsets[i].MatId] as Material)?.Name;
            } else if (subsetsChunk.Subsets[i].MatId == 0)
                matName = materialChunk?.Name;
            else
                throw new NotSupportedException();

            var slicedIndices = new ushort[indexIdTo - indexIdFrom];
            for (var j = indexIdFrom; j < indexIdTo; j++)
                slicedIndices[j - indexIdFrom] = checked((ushort) (indices[j] - vertexIdFrom));

            Meshes.Add(new(matName, false, vertices[vertexIdFrom..vertexIdTo], slicedIndices));
            i = to;
        }
    }

    public void ApplyScaleTransformation(float scale) {
        foreach (var m in Meshes)
            m.ApplyScaleTransformation(scale);
        foreach (var c in Children)
            c.ApplyScaleTransformation(scale);
    }

    public Node Clone() {
        var res = new Node(Name, MaterialName);
        res.Meshes.AddRange(Meshes.Select(x => x.Clone()));
        res.Children.AddRange(Children.Select(x => x.Clone()));
        return res;
    }
    
    public AaBb CalculateBoundingBox() => 
        AaBb.FromEnumerable(Children.Select(x => x.CalculateBoundingBox()))
            .GetExpanded(AaBb.FromEnumerable(Meshes.Select(x => x.CalculateBoundingBox())));

    public IEnumerable<Tuple<Node, Node?>> EnumerateHierarchy() {
        yield return new(this, null);
        foreach (var e in Children.SelectMany(c => c.EnumerateHierarchy()))
            yield return new(e.Item1, this);
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
