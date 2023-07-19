using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Xml.Serialization;
using SynergyLib.FileFormat.CryEngine.CryXml.MaterialElements;

namespace SynergyLib.FileFormat.CryEngine.CryXml;

[XmlRoot("Material")]
public class Material {
    public Vector3? DiffuseColor;
    public Vector3? SpecularColor;
    public Vector3? EmissiveColor;
    public MaterialFlags MaterialFlags;

    [XmlAttribute("Name")]
    public string? Name { get; set; }

    [XmlAttribute("MtlFlags")]
    public string? MtlFlags {
        get => MaterialFlags == 0 ? null : ((int) MaterialFlags).ToString();
        set => MaterialFlags = value is null ? 0 : (MaterialFlags) int.Parse(value);
    }

    [XmlAttribute("Shader")]
    public string? Shader { get; set; }

    [XmlAttribute("GenMask")]
    public string? HexGenMask { get; set; }

    [XmlAttribute("StringGenMask")]
    public string? StringGenMask { get; set; }

    public HashSet<string> GenMaskSet {
        get => (StringGenMask ?? "").Split("%", StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        set => StringGenMask = value.Any() ? "%" + string.Join('%', value) : null;
    }

    public bool ContainsGenMask(string name) {
        if (StringGenMask is null)
            return false;
        var i = StringGenMask.IndexOf("%" + name, StringComparison.OrdinalIgnoreCase);
        if (i < 0)
            return false;
        if (i + 1 + name.Length != StringGenMask.Length && StringGenMask[i + 1 + name.Length] != '%')
            return false;
        return true;
    }

    [XmlAttribute("SurfaceType")]
    public string? SurfaceType { get; set; }

    [XmlAttribute("MatTemplate")]
    public string? MatTemplate { get; set; }

    [XmlAttribute("Diffuse")]
    public string? Diffuse {
        get => DiffuseColor.ToXmlValue();
        set => DiffuseColor = value?.XmlToVector3();
    }

    [XmlAttribute("Specular")]
    public string? Specular {
        get => SpecularColor.ToXmlValue();
        set => SpecularColor = value?.XmlToVector3();
    }

    [XmlAttribute("Emissive")]
    public string? Emissive {
        get => EmissiveColor.ToXmlValue();
        set => EmissiveColor = value?.XmlToVector3();
    }

    [XmlAttribute("Shininess")]
    public float Shininess { get; set; }

    [XmlAttribute("Opacity")]
    public float Opacity { get; set; } = 1f;

    [XmlAttribute("Glossiness")]
    public float Glossiness { get; set; }

    [XmlAttribute("GlowAmount")]
    public float GlowAmount { get; set; }

    [XmlAttribute("AlphaTest")]
    public float AlphaTest { get; set; }

    [XmlAttribute("vertModifType")]
    public int VertModifType { get; set; }

    [XmlAttribute("HeatAmountScaled")]
    public float HeatAmountScaled { get; set; }

    [XmlAttribute("SpecularLevel")]
    public float SpecularLevel { get; set; }

    [XmlArray("SubMaterials")]
    [XmlArrayItem("Material", Type = typeof(Material))]
    [XmlArrayItem("MaterialRef", Type = typeof(MaterialRef))]
    public List<object>? SubMaterialsAndRefs { get; set; }

    [XmlIgnore]
    public IEnumerable<Material?>? SubMaterials => SubMaterialsAndRefs?.Select(x => x as Material);

    [XmlIgnore]
    public IEnumerable<MaterialRef?>? SubMaterialRefs => SubMaterialsAndRefs?.Select(x => x as MaterialRef);

    [XmlElement("PublicParams")]
    public PublicParams? PublicParams { get; set; }

    [XmlElement("VertexDeform")]
    public VertexDeform? VertexDeform { get; set; }

    [XmlArray("Textures")]
    [XmlArrayItem("Texture")]
    public List<Texture>? Textures { get; set; }

    public override string ToString() =>
        $"Name: {Name}, Shader: {Shader}, Submaterials: {SubMaterialsAndRefs?.Count ?? 0}";
}

[XmlRoot("MaterialRef")]
public class MaterialRef { }
