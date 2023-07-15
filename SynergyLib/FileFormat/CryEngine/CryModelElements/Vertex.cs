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
    public MeshBoneMapping BoneMapping;

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
}
