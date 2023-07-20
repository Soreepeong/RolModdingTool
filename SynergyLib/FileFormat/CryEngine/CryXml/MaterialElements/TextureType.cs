using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SynergyLib.FileFormat.CryEngine.CryXml.MaterialElements;

[JsonConverter(typeof(StringEnumConverter))]
public enum TextureType {
    [XmlEnum("0")]
    Default = 0,

    [XmlEnum("3")]
    Environment = 3,

    [XmlEnum("5")]
    Interface = 5,

    [XmlEnum("7")]
    CubeMap = 7,

    [XmlEnum("Nearest Cube-Map probe for alpha blended")]
    NearestCubeMap = 8
}