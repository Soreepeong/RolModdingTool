﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Xml.Serialization;
using JetBrains.Annotations;
using Newtonsoft.Json;
using SynergyLib.FileFormat.CryEngine.CryXml.MaterialElements;
using SynergyLib.Util.CustomJsonConverters;

namespace SynergyLib.FileFormat.CryEngine.CryXml;

[XmlRoot("Material")]
public class Material : MaterialOrRef {
    [JsonConverter(typeof(Vector3JsonConverter))]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
    [XmlIgnore]
    public Vector3 DiffuseColor;

    [JsonConverter(typeof(Vector3JsonConverter))]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
    [XmlIgnore]
    public Vector3 SpecularColor;

    [JsonConverter(typeof(Vector3JsonConverter))]
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Include)]
    [XmlIgnore]
    public Vector3 EmissiveColor;

    [JsonProperty]
    [XmlIgnore]
    public MaterialFlags Flags;

    [JsonProperty]
    [XmlIgnore]
    public ParsedGenMask GenMask = new();

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlAttribute("Shininess")]
    [DefaultValue(0f)]
    public float Shininess { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlAttribute("Opacity")]
    [DefaultValue(0f)]
    public float Opacity { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("GlowAmount")]
    [DefaultValue(0f)]
    public float GlowAmount { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("AlphaTest")]
    [DefaultValue(0f)]
    public float AlphaTest { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("vertModifType")]
    [DefaultValue(0)]
    public int VertModifType { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("HeatAmountScaled")]
    [DefaultValue(0f)]
    public float HeatAmountScaled { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("SpecularLevel")]
    [DefaultValue(0f)]
    public float SpecularLevel { get; set; }

    [JsonProperty]
    [XmlAttribute("Name")]
    public string? Name { get; set; }

    [JsonIgnore]
    [XmlAttribute("MtlFlags")]
    public string? FlagsString {
        get => Flags == 0 ? null : ((int) Flags).ToString();
        set => Flags = value is null ? 0 : (MaterialFlags) int.Parse(value);
    }

    [JsonProperty]
    [XmlAttribute("Shader")]
    public string? Shader { get; set; }

    [JsonIgnore]
    [XmlAttribute("GenMask")]
    public string? HexGenMask { get; set; }

    [JsonIgnore]
    [XmlAttribute("StringGenMask")]
    public string StringGenMask {
        get => GenMask.ToString();
        set => GenMask = new(value);
    }

    public bool ShouldSerializeStringGenMask() => !Flags.HasFlag(MaterialFlags.MultiSubmtl);

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("SurfaceType")]
    [DefaultValue(SurfaceType.Empty)]
    public SurfaceType SurfaceType { get; set; } = SurfaceType.Empty;

    public bool ShouldSerializeSurfaceType() => !Flags.HasFlag(MaterialFlags.MultiSubmtl);

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("MatTemplate")]
    public string MatTemplate { get; set; } = string.Empty;

    public bool ShouldSerializeMatTemplate() => !Flags.HasFlag(MaterialFlags.MultiSubmtl);

    [JsonIgnore]
    [XmlAttribute("Diffuse")]
    public string Diffuse {
        get => DiffuseColor.ToXmlValue();
        set => DiffuseColor = value.XmlToVector3() ?? Vector3.One;
    }

    public bool ShouldSerializeDiffuse() => !Flags.HasFlag(MaterialFlags.MultiSubmtl);

    [JsonIgnore]
    [XmlAttribute("Specular")]
    public string Specular {
        get => SpecularColor.ToXmlValue();
        set => SpecularColor = value.XmlToVector3() ?? Vector3.One;
    }

    public bool ShouldSerializeSpecular() => !Flags.HasFlag(MaterialFlags.MultiSubmtl);

    [JsonIgnore]
    [XmlAttribute("Emissive")]
    public string Emissive {
        get => EmissiveColor.ToXmlValue();
        set => EmissiveColor = value.XmlToVector3() ?? Vector3.Zero;
    }

    public bool ShouldSerializeEmissive() => !Flags.HasFlag(MaterialFlags.MultiSubmtl);

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlArray("SubMaterials")]
    [XmlArrayItem("Material", Type = typeof(Material))]
    [XmlArrayItem("MaterialRef", Type = typeof(MaterialRef))]
    public List<MaterialOrRef>? SubMaterialsAndRefs { get; set; }

    [UsedImplicitly]
    public bool ShouldSerializeSubMaterialsAndRefs() => SubMaterialsAndRefs?.Any() is true;

    [JsonIgnore]
    [XmlIgnore]
    public IEnumerable<Material?>? SubMaterials => SubMaterialsAndRefs?.Select(x => x as Material);

    [JsonIgnore]
    [XmlIgnore]
    public IEnumerable<MaterialRef?>? SubMaterialRefs => SubMaterialsAndRefs?.Select(x => x as MaterialRef);

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlElement("PublicParams")]
    public PublicParams? PublicParams { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlElement("VertexDeform")]
    public VertexDeform? VertexDeform { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    [XmlArray("Textures")]
    [XmlArrayItem("Texture")]
    public List<Texture>? Textures { get; set; }

    public bool ShouldSerializeTextures() => Textures?.Any() is true;

    public override string ToString() =>
        $"Name: {Name}, Shader: {Shader}, Submaterials: {SubMaterialsAndRefs?.Count ?? 0}";

    public override object Clone() {
        var res = (Material) MemberwiseClone();
        res.GenMask = (ParsedGenMask) res.GenMask.Clone();
        res.SubMaterialsAndRefs = SubMaterialsAndRefs?.Select(x => x.Clone()).Cast<MaterialOrRef>().ToList();
        res.PublicParams = (PublicParams?) res.PublicParams?.Clone();
        res.VertexDeform = (VertexDeform?) res.VertexDeform?.Clone();
        res.Textures = res.Textures?.Select(x => x.Clone()).Cast<Texture>().ToList();
        return res;
    }

    public IEnumerable<Material> EnumerateHierarchy() {
        yield return this;
        if (SubMaterials is { } sme)
            foreach (var m in sme.Where(x => x is not null))
                yield return m!;
    }

    public Texture? FindTexture(TextureMapType mapType) => Textures?.SingleOrDefault(x => x.Map == mapType);

    public void AddOrReplaceSubmaterialsOfSameName(IEnumerable<Material?>? submats) {
        if (submats is null)
            return;
        
        foreach (var m in submats) {
            if (m is null)
                continue;
            
            SubMaterialsAndRefs ??= new();
            var found = false;
            for (var i = 0; i < SubMaterialsAndRefs.Count; i++) {
                if (SubMaterialsAndRefs[i] is not Material m2)
                    continue;
                if (m2.Name != m.Name)
                    continue;

                SubMaterialsAndRefs[i] = m;
                found = true;
                break;
            }

            if (!found)
                SubMaterialsAndRefs.Add(m);
        }
    }
}
