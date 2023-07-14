using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using SynergyLib.FileFormat.CryEngine.CryXml.ChrParamsSubElements;

namespace SynergyLib.FileFormat.CryEngine.CryXml;

[XmlRoot(ElementName = "Params")]
public class ChrParamsFile {
    [XmlArray(ElementName = "AnimationList")]
    [XmlArrayItem(ElementName = "Animation", Type = typeof(Animation))]
    [XmlArrayItem(ElementName = "Comment", Type = typeof(Comment))]
    public object[] Items { get; set; } = Array.Empty<object>();

    [XmlIgnore]
    public IEnumerable<Animation> Animations => Items.OfType<Animation>();

    [XmlIgnore]
    public string? BasePath => Animations.SingleOrDefault(x => x.Name == "#filepath")?.Path;

    [XmlIgnore]
    public string? AnimEventDatabasePath => Animations.SingleOrDefault(x => x.Name == "$AnimEventDatabase")?.Path;

    [XmlIgnore]
    public string? TracksDatabasePath => Animations.SingleOrDefault(x => x.Name == "$TracksDatabase")?.Path;
    
    [XmlIgnore]
    public string? FaceLibPath => Animations.SingleOrDefault(x => x.Name == "$facelib")?.Path;
}
