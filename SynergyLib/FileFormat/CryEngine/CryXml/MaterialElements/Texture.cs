using System.ComponentModel;
using System.Xml.Serialization;
using Newtonsoft.Json;
using SynergyLib.FileFormat.GltfInterop.Models;

namespace SynergyLib.FileFormat.CryEngine.CryXml.MaterialElements;

[XmlRoot("Texture")]
public class Texture {
    [JsonIgnore]
    [XmlAttribute("Map")]
    public string? MapString { get; set; }

    [JsonProperty]
    [XmlIgnore]
    public TextureMapType Map {
        get => MapString switch {
            "Diffuse" => TextureMapType.Diffuse,
            "Specular" => TextureMapType.Specular,
            "Bumpmap" => TextureMapType.Normals,
            "Environment" => TextureMapType.Env,
            "Detail" => TextureMapType.Detail,
            "Opacity" => TextureMapType.Opacity,
            "Decal" => TextureMapType.Decal,
            "SubSurface" => TextureMapType.SubSurface,
            "Custom" => TextureMapType.Custom,
            "[1] Custom" => TextureMapType.Custom2,
            _ => 0,
        };
        set => MapString = value switch {
            TextureMapType.Diffuse => "Diffuse",
            TextureMapType.Specular => "Specular",
            TextureMapType.Normals => "Bumpmap",
            TextureMapType.Env => "Environment",
            TextureMapType.Detail => "Detail",
            TextureMapType.Opacity => "Opacity",
            TextureMapType.Decal => "Decal",
            TextureMapType.SubSurface => "SubSurface",
            TextureMapType.Custom => "Custom",
            TextureMapType.Custom2 => "[1] Custom",
            _ => $"({value})",
        };
    }

    [JsonProperty]
    [XmlAttribute("File")]
    public string? File { get; set; }

    [JsonProperty]
    [XmlAttribute("TexType")]
    [DefaultValue(TextureType.Default)]
    public TextureType TexType;

    [JsonIgnore]
    [XmlAttribute("IsTileU")]
    [DefaultValue(1)]
    public int IsTileUInt { get; set; } = 1;

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlIgnore]
    [DefaultValue(true)]
    public bool IsTileU {
        get => IsTileUInt != 0;
        set => IsTileUInt = value ? 1 : 0;
    }

    [JsonIgnore]
    [XmlAttribute("IsTileV")]
    [DefaultValue(1)]
    public int IsTileVInt { get; set; } = 1;

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlIgnore]
    [DefaultValue(true)]
    public bool IsTileV {
        get => IsTileVInt != 0;
        set => IsTileVInt = value ? 1 : 0;
    }

    [JsonIgnore]
    [XmlAttribute("Filter")]
    [DefaultValue(-1)]
    public int FilterInt { get; set; } = -1;

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlIgnore]
    public TextureFilter Filter {
        get => (TextureFilter) FilterInt;
        set => FilterInt = (int) value;
    }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlElement("TexMod")]
    public TextureModifier? Modifier;

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlIgnore]
    public GltfTextureInfo? GltfTextureInfo;
}
