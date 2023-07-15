using System.Numerics;
using SynergyLib.Util.MathExtras;

namespace SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;

public struct MeshShapeDeformation {
    public Vector3 Thin;
    public Vector3 Fat;
    public Vector4<byte> Index;
}
