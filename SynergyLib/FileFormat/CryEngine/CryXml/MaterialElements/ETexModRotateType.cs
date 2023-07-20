using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SynergyLib.FileFormat.CryEngine.CryXml.MaterialElements;

[JsonConverter(typeof(StringEnumConverter))]
public enum ETexModRotateType {
    NoChange,
    Fixed,
    Constant,
    Oscillated,
    Max
}
