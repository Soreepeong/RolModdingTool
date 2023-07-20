using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SynergyLib.FileFormat.CryEngine.CryXml.MaterialElements;

[JsonConverter(typeof(StringEnumConverter))]
public enum ETexGenType {
    Stream,
    World,
    Camera,
    WorldEnvMap,
    CameraEnvMap,
    NormalMap,
    SphereMap,
    Max
}
