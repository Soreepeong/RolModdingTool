using System.Numerics;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;
using SynergyLib.Util.MathExtras;

namespace SynergyLib.FileFormat.CryEngine.CryModelElements;

public struct Vertex {
    public Vector3 Position;
    public Vector3 Normal;
    public Vector2 TexCoord;
    public Vector4<byte> Color;
    public MeshTangent Tangent;
    public Vector4<uint> ControllerIds;
    public Vector4<float> Weights;

    // public MeshShapeDeformation ShapeDeformation;
    // public MeshBoneMapping BoneMapping;
}
