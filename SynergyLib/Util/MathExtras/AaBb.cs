using System.Numerics;

namespace SynergyLib.Util.MathExtras;

public struct AaBb {
    public Vector3 Min;
    public Vector3 Max;

    public AaBb() { }

    public AaBb(Vector3 min, Vector3 max) {
        Min = min;
        Max = max;
    }
}
