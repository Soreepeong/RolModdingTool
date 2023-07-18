using System.ComponentModel;
using System.Xml.Serialization;

namespace SynergyLib.FileFormat.CryEngine.CryXml.MaterialElements;

/// <summary>The texture modifier</summary>
[XmlRoot("TexMod")]
public class TextureModifier {
    [XmlAttribute("TileU")]
    [DefaultValue(0)]
    public float TileU { get; set; }

    [XmlAttribute("TileV")]
    [DefaultValue(0)]
    public float TileV { get; set; }

    [XmlAttribute("OffsetU")]
    [DefaultValue(0)]
    public float OffsetU { get; set; }

    [XmlAttribute("OffsetV")]
    [DefaultValue(0)]
    public float OffsetV { get; set; }

    [XmlAttribute("TexMod_bTexGenProjected")]
    [DefaultValue(1)]
    public int __Projected {
        get => Projected ? 1 : 0;
        set => Projected = value == 1;
    }

    [XmlIgnore]
    public bool Projected { get; set; }

    [XmlAttribute("TexMod_UOscillatorType")]
    [DefaultValue(0)]
    public int __UOscillatorType;

    public ETexModMoveType UOscillatorType {
        get => (ETexModMoveType) __UOscillatorType;
        set => __UOscillatorType = (int) value;
    }

    [XmlAttribute("TexMod_VOscillatorType")]
    [DefaultValue(0)]
    public int __VOscillatorType;

    public ETexModMoveType VOscillatorType {
        get => (ETexModMoveType) __VOscillatorType;
        set => __VOscillatorType = (int) value;
    }

    [XmlAttribute("TexMod_RotateType")]
    [DefaultValue(ETexModRotateType.NoChange)]
    public int __RotateType { get; set; }

    public ETexModRotateType RotateType {
        get => (ETexModRotateType) __RotateType;
        set => __RotateType = (int) value;
    }

    [XmlAttribute("TexMod_TexGenType")]
    [DefaultValue(ETexGenType.Stream)]
    public int __GenType { get; set; }

    public ETexGenType GenType {
        get => (ETexGenType) __GenType;
        set => __GenType = (int) value;
    }

    [XmlAttribute("RotateU")]
    [DefaultValue(0)]
    public float RotateU { get; set; }

    [XmlAttribute("RotateV")]
    [DefaultValue(0)]
    public float RotateV { get; set; }

    [XmlAttribute("RotateW")]
    [DefaultValue(0)]
    public float RotateW { get; set; }

    [XmlAttribute("TexMod_URotateRate")]
    public float URotateRate { get; set; }

    [XmlAttribute("TexMod_VRotateRate")]
    public float VRotateRate { get; set; }

    [XmlAttribute("TexMod_WRotateRate")]
    public float WRotateRate { get; set; }

    [XmlAttribute("TexMod_URotatePhase")]
    public float URotatePhase { get; set; }

    [XmlAttribute("TexMod_VRotatePhase")]
    public float VRotatePhase { get; set; }

    [XmlAttribute("TexMod_WRotatePhase")]
    public float WRotatePhase { get; set; }

    [XmlAttribute("TexMod_URotateAmplitude")]
    public float URotateAmplitude { get; set; }

    [XmlAttribute("TexMod_VRotateAmplitude")]
    public float VRotateAmplitude { get; set; }

    [XmlAttribute("TexMod_WRotateAmplitude")]
    public float WRotateAmplitude { get; set; }

    [XmlAttribute("TexMod_URotateCenter")]
    public float URotateCenter { get; set; }

    [XmlAttribute("TexMod_VRotateCenter")]
    public float VRotateCenter { get; set; }

    [XmlAttribute("TexMod_WRotateCenter")]
    public float WRotateCenter { get; set; }

    [XmlAttribute("TexMod_UOscillatorRate")]
    public float UOscillatorRate { get; set; }

    [XmlAttribute("TexMod_VOscillatorRate")]
    public float VOscillatorRate { get; set; }

    [XmlAttribute("TexMod_UOscillatorPhase")]
    public float UOscillatorPhase { get; set; }

    [XmlAttribute("TexMod_VOscillatorPhase")]
    public float VOscillatorPhase { get; set; }

    [XmlAttribute("TexMod_UOscillatorAmplitude")]
    public float UOscillatorAmplitude { get; set; }

    [XmlAttribute("TexMod_VOscillatorAmplitude")]
    public float VOscillatorAmplitude { get; set; }
}
