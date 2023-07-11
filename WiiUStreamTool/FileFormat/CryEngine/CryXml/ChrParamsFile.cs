using System;
using System.Xml.Serialization;
using WiiUStreamTool.FileFormat.CryEngine.CryXml.ChrParamsSubElements;

namespace WiiUStreamTool.FileFormat.CryEngine.CryXml;

[XmlRoot(ElementName = "Params")]
public class ChrParamsFile {
    [XmlArray(ElementName = "AnimationList")]
    [XmlArrayItem(ElementName = "Animation")]
    public Animation[] Animations { get; set; } = Array.Empty<Animation>();
}
