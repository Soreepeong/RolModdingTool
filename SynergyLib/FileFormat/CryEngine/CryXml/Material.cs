using System.Collections.Generic;
using System.Numerics;
using System.Xml.Serialization;
using SynergyLib.FileFormat.CryEngine.CryXml.MaterialElements;

namespace SynergyLib.FileFormat.CryEngine.CryXml;

[XmlRoot(ElementName = "Material")]
public class Material {
    public Vector3? DiffuseColor;
    public Vector3? SpecularColor;
    public Vector3? EmissiveColor;
    public MaterialFlags MaterialFlags;

    [XmlIgnore]
    internal string? SourceFileName { get; set; }

    [XmlAttribute(AttributeName = "Name")]
    public string? Name { get; set; }

    [XmlAttribute(AttributeName = "MtlFlags")]
    public string? MtlFlags {
        get => MaterialFlags == 0 ? null : ((int) MaterialFlags).ToString();
        set => MaterialFlags = value is null ? 0 : (MaterialFlags) int.Parse(value);
    }

    [XmlAttribute(AttributeName = "Shader")]
    public string? Shader { get; set; }

    [XmlAttribute(AttributeName = "GenMask")]
    public string? GenMask { get; set; }

    [XmlAttribute(AttributeName = "StringGenMask")]
    public string? StringGenMask { get; set; }

    [XmlAttribute(AttributeName = "SurfaceType")]
    public string? SurfaceType { get; set; }

    [XmlAttribute(AttributeName = "MatTemplate")]
    public string? MatTemplate { get; set; }

    [XmlAttribute(AttributeName = "Diffuse")]
    public string? Diffuse {
        get => DiffuseColor.ToXmlValue();
        set => DiffuseColor = value.XmlToVector3();
    }

    [XmlAttribute(AttributeName = "Specular")]
    public string? Specular {
        get => SpecularColor.ToXmlValue();
        set => SpecularColor = value.XmlToVector3();
    }

    [XmlAttribute(AttributeName = "Emissive")]
    public string? Emissive {
        get => EmissiveColor.ToXmlValue();
        set => EmissiveColor = value.XmlToVector3();
    }

    [XmlAttribute(AttributeName = "Shininess")]
    public float Shininess { get; set; }

    [XmlAttribute(AttributeName = "Opacity")]
    public float Opacity { get; set; } = 1f;

    [XmlAttribute(AttributeName = "Glossiness")]
    public float Glossiness { get; set; }

    [XmlAttribute(AttributeName = "GlowAmount")]
    public float GlowAmount { get; set; }

    [XmlAttribute(AttributeName = "AlphaTest")]
    public float AlphaTest { get; set; }

    [XmlArray(ElementName = "SubMaterials")]
    [XmlArrayItem(ElementName = "Material")]
    public List<Material>? SubMaterials { get; set; }

    [XmlElement(ElementName = "PublicParams")]
    public PublicParams? PublicParams { get; set; }

    [XmlArray(ElementName = "Textures")]
    [XmlArrayItem(ElementName = "Texture")]
    public List<Texture>? Textures { get; set; }

    public override string ToString() => $"Name: {Name}, Shader: {Shader}, Submaterials: {SubMaterials?.Count ?? 0}";
}
