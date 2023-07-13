using System.Numerics;
using System.Xml.Serialization;

namespace SynergyLib.FileFormat.CryEngine.CryXml.CdfSubElements;

[XmlRoot(ElementName = "Attachment")]
public class Attachment {
    [XmlIgnore]
    public Quaternion Rotation = Quaternion.Identity;

    [XmlIgnore]
    public Vector3 Position = Vector3.Zero;

    [XmlAttribute(AttributeName = "AName")]
    public string AName { get; set; } = string.Empty;

    [XmlAttribute(AttributeName = "Type")]
    public string Type { get; set; } = string.Empty;

    [XmlAttribute(AttributeName = "Rotation")]
    public string? RotationString {
        get => Rotation.ToXmlValue();
        set => Rotation = value.XmlToQuaternion() ?? Quaternion.Identity;
    }

    [XmlAttribute(AttributeName = "Position")]
    public string? PositionString {
        get => Position.ToXmlValue();
        set => Position = value.XmlToVector3() ?? Vector3.Zero;
    }

    [XmlAttribute(AttributeName = "BoneName")]
    public string BoneName { get; set; } = string.Empty;

    [XmlAttribute(AttributeName = "Binding")]
    public string Binding { get; set; } = string.Empty;

    [XmlAttribute(AttributeName = "Flags")]
    public int Flags { get; set; }
}
