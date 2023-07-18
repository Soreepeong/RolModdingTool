using System.ComponentModel;
using System.Numerics;
using System.Xml.Serialization;

namespace SynergyLib.FileFormat.CryEngine.CryXml.MaterialElements;

[XmlRoot("PublicParams")]
public class PublicParams {
    public Vector3? SilhouetteColor;
    public Vector3? IrisColor;
    public Vector3? IndirectColor;
    public Vector3? WrapColor;
    public Vector3? BackDiffuse;
    public Vector3? TintColor;

    [XmlAttribute("IrisAspectRatio")]
    [DefaultValue(1f)]
    public float IrisAspectRatio { get; set; } = 1f;

    [XmlAttribute("IrisColor")]
    public string? IrisColorString {
        get => IrisColor.ToXmlValue();
        set => IrisColor = value.XmlToVector3();
    }

    [XmlAttribute("IrisRotate")]
    [DefaultValue(0f)]
    public float IrisRotate { get; set; }

    [XmlAttribute("IrisSize")]
    [DefaultValue(1f)]
    public float IrisSize { get; set; } = 1f;

    [XmlAttribute("EyeSadFactor")]
    [DefaultValue(0f)]
    public float EyeSadFactor { get; set; }

    [XmlAttribute("PupilScale")]
    [DefaultValue(1f)]
    public float PupilScale { get; set; } = 1f;

    [XmlAttribute("SilhouetteColor")]
    public string? SilhouetteColorString {
        get => SilhouetteColor.ToXmlValue();
        set => SilhouetteColor = value.XmlToVector3();
    }

    [XmlAttribute("SilhouetteIntensity")]
    [DefaultValue(1f)]
    public float SilhouetteIntensity { get; set; } = 1f;

    [XmlAttribute("FresnelPower")]
    [DefaultValue(1f)]
    public float FresnelPower { get; set; } = 1f;

    [XmlAttribute("FresnelScale")]
    [DefaultValue(1f)]
    public float FresnelScale { get; set; } = 1f;

    [XmlAttribute("FresnelBias")]
    [DefaultValue(1f)]
    public float FresnelBias { get; set; } = 1f;

    [XmlAttribute("IrisParallaxOffset")]
    [DefaultValue(0f)]
    public float IrisParallaxOffset { get; set; }

    [XmlAttribute("EyeRotateVertical")]
    [DefaultValue(0f)]
    public float EyeRotateVertical { get; set; }

    [XmlAttribute("EyeRotateHorizontal")]
    [DefaultValue(0f)]
    public float EyeRotateHorizontal { get; set; }

    [XmlAttribute("EyeRotationMultiplier")]
    [DefaultValue(0f)]
    public float EyeRotationMultiplier { get; set; } = 1f;

    [XmlAttribute("IndirectColor")]
    public string? IndirectColorString {
        get => IndirectColor.ToXmlValue();
        set => IndirectColor = value.XmlToVector3();
    }

    [XmlAttribute("Metalness")]
    [DefaultValue(0f)]
    public float Metalness { get; set; }

    [XmlAttribute("WrapColor")]
    public string? WrapColorString {
        get => WrapColor.ToXmlValue();
        set => WrapColor = value.XmlToVector3();
    }

    [XmlAttribute("WrapDiffuse")]
    [DefaultValue(0f)]
    public float WrapDiffuse { get; set; }

    [XmlAttribute("bendDetailLeafAmplitude")]
    [DefaultValue(0f)]
    public float BendDetailLeafAmplitude { get; set; }

    [XmlAttribute("bendDetailFrequency")]
    [DefaultValue(0f)]
    public float BendDetailFrequency { get; set; }

    [XmlAttribute("bendDetailBranchAmplitude")]
    [DefaultValue(0f)]
    public float BendDetailBranchAmplitude { get; set; }

    [XmlAttribute("BlendFalloff")]
    [DefaultValue(0f)]
    public float BlendFalloff { get; set; }

    [XmlAttribute("BlendLayer2Tiling")]
    [DefaultValue(0f)]
    public float BlendLayer2Tiling { get; set; }

    [XmlAttribute("BlendFactor")]
    [DefaultValue(0f)]
    public float BlendFactor { get; set; }

    [XmlAttribute("blendWithTerrainAmount")]
    [DefaultValue(0f)]
    public float BlendWithTerrainAmount { get; set; }

    [XmlAttribute("BackDiffuse")]
    public string? BackDiffuseString {
        get => BackDiffuse.ToXmlValue();
        set => BackDiffuse = value.XmlToVector3();
    }

    [XmlAttribute("BackDiffuseMultiplier")]
    [DefaultValue(0f)]
    public float BackDiffuseMultiplier { get; set; }

    [XmlAttribute("BackShadowBias")]
    [DefaultValue(0f)]
    public float BackShadowBias { get; set; }

    [XmlAttribute("BackViewDep")]
    [DefaultValue(0f)]
    public float BackViewDep { get; set; }

    [XmlAttribute("CapOpacityFalloff")]
    [DefaultValue(0f)]
    public float CapOpacityFalloff { get; set; }

    [XmlAttribute("BackLightScale")]
    [DefaultValue(0f)]
    public float BackLightScale { get; set; }

    [XmlAttribute("BumpScale")]
    [DefaultValue(0f)]
    public float BumpScale { get; set; }

    [XmlAttribute("IndexOfRefraction")]
    [DefaultValue(0f)]
    public float IndexOfRefraction { get; set; }

    [XmlAttribute("TintCloudiness")]
    [DefaultValue(0f)]
    public float TintCloudiness { get; set; }

    [XmlAttribute("TintColor")]
    public string? TintColorString {
        get => TintColor.ToXmlValue();
        set => TintColor = value.XmlToVector3();
    }
}
