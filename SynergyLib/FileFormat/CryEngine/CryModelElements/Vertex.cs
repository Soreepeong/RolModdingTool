using System;
using System.Numerics;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;
using SynergyLib.Util.MathExtras;

namespace SynergyLib.FileFormat.CryEngine.CryModelElements;

public struct Vertex : IEquatable<Vertex> {
    public Vector3 Position;
    public Vector3 Normal;
    public Vector2 TexCoord;
    public Vector4<byte> Color;
    public MeshTangent Tangent;
    public Vector4<uint> ControllerIds;
    public Vector4<float> Weights;

    // public MeshShapeDeformation ShapeDeformation;
    // public MeshBoneMapping BoneMapping;

    public bool Equals(Vertex other) =>
        Position.Equals(other.Position)
        && Normal.Equals(other.Normal)
        && TexCoord.Equals(other.TexCoord)
        && Color.Equals(other.Color)
        && Tangent.Equals(other.Tangent)
        && ControllerIds.Equals(other.ControllerIds)
        && Weights.Equals(other.Weights);

    public override bool Equals(object? obj) => obj is Vertex other && Equals(other);

    public override int GetHashCode() =>
        HashCode.Combine(Position, Normal, TexCoord, Color, Tangent, ControllerIds, Weights);

    public static float CalculateArea(in Vertex v0, in Vertex v1, in Vertex v2) =>
        MathF.Abs(
            v0.Position.X * (v1.Position.Y - v2.Position.Y)
            + v1.Position.X * (v2.Position.Y - v0.Position.Y)
            + v2.Position.X * (v0.Position.Y - v1.Position.Y));

    public static void CalculateNormalTangentBinormals(
        in Vertex v0,
        in Vertex v1,
        in Vertex v2,
        out Vector3 normal,
        out Vector3 tangent,
        out Vector3 binormal) {
        // See:
        // https://gamedev.net/forums/topic/571707-how-to-calculate-tangent-binormal-normal-vectors-thanks/4650612/
        // https://stackoverflow.com/questions/5255806/how-to-calculate-tangent-and-binormal
        
        var p = v1.Position - v0.Position;
        var q = v2.Position - v0.Position;
        var s1 = v1.TexCoord.X - v0.TexCoord.X;
        var t1 = v1.TexCoord.Y - v0.TexCoord.Y;
        var s2 = v2.TexCoord.X - v0.TexCoord.X;
        var t2 = v2.TexCoord.Y - v0.TexCoord.Y;
        var tmp = float.Abs(s1 * t2 - s2 * t1) <= 1e-4 ? 1f : 1.0f / (s1 * t2 - s2 * t1);

        normal = Vector3.Normalize(Vector3.Cross(p, q));
        tangent = Vector3.Normalize((t2 * p - t1 * q) * tmp);
        binormal = Vector3.Normalize((s1 * q - s2 * p) * tmp);
    }
}
