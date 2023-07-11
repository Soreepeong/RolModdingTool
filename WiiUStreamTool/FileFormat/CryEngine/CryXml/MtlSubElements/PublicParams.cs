using System.Numerics;
using System.Xml.Serialization;

namespace WiiUStreamTool.FileFormat.CryEngine.CryXml.MtlSubElements;

[XmlRoot(ElementName = "PublicParams")]
public class PublicParams {
    public Vector3? SilhouetteColor;
    public Vector3? IrisColor;
    public Vector3? IndirectColor;

    [XmlAttribute(AttributeName = "IrisAspectRatio")]
    public float IrisAspectRatio { get; set; } = 1f;

    [XmlAttribute(AttributeName = "IrisColor")]
    public string? IrisColorString {
        get => IrisColor.ToXmlValue();
        set => IrisColor = value.XmlToVector3();
    }

    [XmlAttribute(AttributeName = "IrisRotate")]
    public float IrisRotate { get; set; }

    [XmlAttribute(AttributeName = "IrisSize")]
    public float IrisSize { get; set; } = 1f;

    [XmlAttribute(AttributeName = "EyeSadFactor")]
    public float EyeSadFactor { get; set; }

    [XmlAttribute(AttributeName = "PupilScale")]
    public float PupilScale { get; set; } = 1f;

    [XmlAttribute(AttributeName = "SilhouetteColor")]
    public string? SilhouetteColorString {
        get => SilhouetteColor.ToXmlValue();
        set => SilhouetteColor = value.XmlToVector3();
    }

    [XmlAttribute(AttributeName = "SilhouetteIntensity")]
    public float SilhouetteIntensity { get; set; } = 1f;

    [XmlAttribute(AttributeName = "FresnelPower")]
    public float FresnelPower { get; set; }

    [XmlAttribute(AttributeName = "FresnelScale")]
    public float FresnelScale { get; set; } = 1f;

    [XmlAttribute(AttributeName = "FresnelBias")]
    public float FresnelBias { get; set; } = 1f;

    [XmlAttribute(AttributeName = "IrisParallaxOffset")]
    public float IrisParallaxOffset { get; set; }

    [XmlAttribute(AttributeName = "EyeRotateVertical")]
    public float EyeRotateVertical { get; set; }

    [XmlAttribute(AttributeName = "EyeRotateHorizontal")]
    public float EyeRotateHorizontal { get; set; }

    [XmlAttribute(AttributeName = "EyeRotationMultiplier")]
    public float EyeRotationMultiplier { get; set; } = 1f;

    [XmlAttribute(AttributeName = "IndirectColor")]
    public string? IndirectColorString {
        get => IndirectColor.ToXmlValue();
        set => IndirectColor = value.XmlToVector3();
    }

    [XmlAttribute(AttributeName = "Metalness")]
    public float Metalness { get; set; }
}
