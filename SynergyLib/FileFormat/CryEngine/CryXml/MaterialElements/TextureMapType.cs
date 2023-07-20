using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SynergyLib.FileFormat.CryEngine.CryXml.MaterialElements;

[JsonConverter(typeof(StringEnumConverter))]
public enum TextureMapType {
    Diffuse,
    Normals,
    Specular,
    Env,
    Detail,
    Opacity,
    Decal,
    SubSurface,
    Custom,
    Custom2,
}