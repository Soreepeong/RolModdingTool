using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SynergyLib.FileFormat.CryEngine.CryXml.MaterialElements;

[JsonConverter(typeof(StringEnumConverter))]
public enum TextureFilter {
    None = -1,
    Point,
    Linear,
    Bilinear,
    Trilinear,
    Aniso2X,
    Aniso4X,
    Aniso8X,
    Anosi16X,
}