using System.Numerics;
using System.Xml.Serialization;

namespace SynergyLib.FileFormat.CryEngine.CryXml.CharacterDefinitionElements;

[XmlRoot("Attachment")]
public class Attachment {
    [XmlIgnore]
    public Quaternion Rotation = Quaternion.Identity;

    [XmlIgnore]
    public Vector3 Position = Vector3.Zero;

    [XmlAttribute("AName")]
    public string? AName { get; set; }

    [XmlAttribute("Type")]
    public string? Type { get; set; }

    [XmlAttribute("Rotation")]
    public string? RotationString {
        get => Rotation.ToXmlValue();
        set => Rotation = value.XmlToQuaternion() ?? Quaternion.Identity;
    }

    [XmlAttribute("Position")]
    public string? PositionString {
        get => Position.ToXmlValue();
        set => Position = value?.XmlToVector3() ?? Vector3.Zero;
    }

    [XmlAttribute("BoneName")]
    public string? BoneName { get; set; }

    [XmlAttribute("Binding")]
    public string? Binding { get; set; }

    [XmlAttribute("Flags")]
    public int Flags { get; set; }

    [XmlAttribute("Material")]
    public string? Material { get; set; }
}
