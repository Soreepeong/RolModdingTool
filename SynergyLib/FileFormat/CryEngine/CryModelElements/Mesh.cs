using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;

namespace SynergyLib.FileFormat.CryEngine.CryModelElements;

public class Mesh {
    public string? MaterialName;
    public bool IsProxy;
    public Vertex[] Vertices;
    public ushort[] Indices;

    public Mesh(string? materialName, bool isProxy, Vertex[] vertices, ushort[] indices) {
        MaterialName = materialName;
        IsProxy = isProxy;
        Vertices = vertices;
        Indices = indices;
    }

    public Mesh Clone() => new(MaterialName, IsProxy, (Vertex[]) Vertices.Clone(), (ushort[]) Indices.Clone());

    public override string ToString() =>
        $"{nameof(Mesh)}: {MaterialName} (vert={Vertices.Length} ind={Indices.Length})";

    public static List<IntSkinVertex> ToIntSkinVertices(
        IReadOnlyList<Mesh> meshes,
        IReadOnlyDictionary<uint, ushort> controllerIdToBoneId,
        out List<ushort> extToInt) {
        var totalVertices = meshes.Sum(x => x.Vertices.Length);
        var res = new List<IntSkinVertex>(totalVertices);
        var resToInt = new Dictionary<IntSkinVertex, ushort>(totalVertices);
        extToInt = new(totalVertices);

        foreach (var mesh in meshes) {
            foreach (var vertex in mesh.Vertices) {
                var isv = new IntSkinVertex {
                    BoneIds = new(
                        vertex.ControllerIds.Select(x => controllerIdToBoneId.GetValueOrDefault(x, (ushort) 0))),
                    Color = vertex.Color,
                    Position0 = vertex.Position,
                    Position1 = vertex.Position,
                    Position2 = vertex.Position,
                    Weights = vertex.Weights,
                };

                if (!resToInt.TryGetValue(isv, out var intIndex)) {
                    resToInt.Add(isv, intIndex = checked((ushort) res.Count));
                    res.Add(isv);
                }

                extToInt.Add(intIndex);
            }
        }

        Debug.Assert(extToInt.Count == totalVertices);
        return res;
    }

    public static List<CompiledIntFace> ToIntFaces(IReadOnlyList<Mesh> meshes, IReadOnlyList<ushort> extToInt) {
        var totalFaces = meshes.Sum(x => x.Indices.Length) / 3;
        var res = new List<CompiledIntFace>(totalFaces);

        var baseVertexIndex = 0;
        foreach (var mesh in meshes) {
            for (var i = 0; i < mesh.Indices.Length; i += 3) {
                res.Add(
                    new(
                        extToInt[baseVertexIndex + mesh.Indices[i + 0]],
                        extToInt[baseVertexIndex + mesh.Indices[i + 1]],
                        extToInt[baseVertexIndex + mesh.Indices[i + 2]]));
            }

            baseVertexIndex += mesh.Vertices.Length;
        }

        Debug.Assert(res.Count == totalFaces);
        return res;
    }
}
