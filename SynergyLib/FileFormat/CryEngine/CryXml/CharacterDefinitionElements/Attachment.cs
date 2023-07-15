using System.Numerics;
using System.Xml.Serialization;

namespace SynergyLib.FileFormat.CryEngine.CryXml.CharacterDefinitionElements;

[XmlRoot(ElementName = "Attachment")]
public class Attachment {
    [XmlIgnore]
    public Quaternion Rotation = Quaternion.Identity;

    [XmlIgnore]
    public Vector3 Position = Vector3.Zero;

    [XmlAttribute(AttributeName = "AName")]
    public string? AName { get; set; }

    [XmlAttribute(AttributeName = "Type")]
    public string? Type { get; set; }

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
    public string? BoneName { get; set; }

    [XmlAttribute(AttributeName = "Binding")]
    public string? Binding { get; set; }

    [XmlAttribute(AttributeName = "Flags")]
    public int Flags { get; set; }

    [XmlAttribute(AttributeName = "Material")]
    public string? Material { get; set; }
}
