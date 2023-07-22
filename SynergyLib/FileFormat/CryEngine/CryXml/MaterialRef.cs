using System.Xml.Serialization;

namespace SynergyLib.FileFormat.CryEngine.CryXml;

[XmlRoot("MaterialRef")]
public class MaterialRef : MaterialOrRef {
    public override object Clone() => MemberwiseClone();
}
