using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using SynergyLib.FileFormat.CryEngine.CryXml.CharacterDefinitionElements;
using SynergyLib.FileFormat.CryEngine.CryXml.CharacterParametersElements;

namespace SynergyLib.FileFormat.CryEngine.CryXml;

[XmlRoot("Params")]
public class CharacterParameters {
    [XmlArray("AnimationList")]
    [XmlArrayItem("Animation", Type = typeof(Animation))]
    [XmlArrayItem("Comment", Type = typeof(Comment))]
    [XmlArrayItem("Parts", Type = typeof(Parts))]
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
    
    [XmlElement("Parts")]
    public Parts? Parts { get; set; }
}