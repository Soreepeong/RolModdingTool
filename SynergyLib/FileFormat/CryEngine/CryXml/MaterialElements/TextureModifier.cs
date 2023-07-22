using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace SynergyLib.FileFormat.CryEngine.CryXml.MaterialElements;

/// <summary>The texture modifier</summary>
[XmlRoot("TexMod")]
public class TextureModifier : ICloneable {
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [DefaultValue(ETexModMoveType.NoChange)]
    public ETexModMoveType UOscillatorType = ETexModMoveType.NoChange;

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [DefaultValue(ETexModMoveType.NoChange)]
    public ETexModMoveType VOscillatorType = ETexModMoveType.NoChange;

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [DefaultValue(ETexModRotateType.NoChange)]
    public ETexModRotateType RotateType = ETexModRotateType.NoChange;

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [DefaultValue(ETexGenType.Stream)]
    public ETexGenType GenType = ETexGenType.Stream;

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("TileU")]
    [DefaultValue(0)]
    public float TileU { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("TileV")]
    [DefaultValue(0)]
    public float TileV { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("OffsetU")]
    [DefaultValue(0)]
    public float OffsetU { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("OffsetV")]
    [DefaultValue(0)]
    public float OffsetV { get; set; }

    [JsonIgnore]
    [XmlAttribute("TexMod_UOscillatorType")]
    [DefaultValue(0)]
    public int UOscillatorTypeInt {
        get => (int)UOscillatorType;
        set => UOscillatorType = (ETexModMoveType)value;
    }

    [JsonIgnore]
    [XmlAttribute("TexMod_VOscillatorType")]
    [DefaultValue(0)]
    public int VOscillatorTypeInt {
        get => (int)VOscillatorType;
        set => VOscillatorType = (ETexModMoveType)value;
    }
    
    [JsonIgnore]
    [XmlAttribute("TexMod_RotateType")]
    [DefaultValue(0)]
    public int RotateTypeInt {
        get => (int)RotateType;
        set => RotateType = (ETexModRotateType)value;
    }

    [JsonIgnore]
    [XmlAttribute("TexMod_TexGenType")]
    [DefaultValue(0)]
    public int GenTypeInt {
        get => (int)GenType;
        set => GenType = (ETexGenType)value;
    }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("RotateU")]
    [DefaultValue(0)]
    public float RotateU { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("RotateV")]
    [DefaultValue(0)]
    public float RotateV { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("RotateW")]
    [DefaultValue(0)]
    public float RotateW { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("TexMod_URotateRate")]
    [DefaultValue(0)]
    public float URotateRate { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("TexMod_VRotateRate")]
    [DefaultValue(0)]
    public float VRotateRate { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("TexMod_WRotateRate")]
    [DefaultValue(0)]
    public float WRotateRate { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("TexMod_URotatePhase")]
    [DefaultValue(0)]
    public float URotatePhase { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("TexMod_VRotatePhase")]
    [DefaultValue(0)]
    public float VRotatePhase { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("TexMod_WRotatePhase")]
    [DefaultValue(0)]
    public float WRotatePhase { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("TexMod_URotateAmplitude")]
    [DefaultValue(0)]
    public float URotateAmplitude { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("TexMod_VRotateAmplitude")]
    [DefaultValue(0)]
    public float VRotateAmplitude { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("TexMod_WRotateAmplitude")]
    [DefaultValue(0)]
    public float WRotateAmplitude { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("TexMod_URotateCenter")]
    [DefaultValue(0)]
    public float URotateCenter { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("TexMod_VRotateCenter")]
    [DefaultValue(0)]
    public float VRotateCenter { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("TexMod_WRotateCenter")]
    [DefaultValue(0)]
    public float WRotateCenter { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("TexMod_UOscillatorRate")]
    [DefaultValue(0)]
    public float UOscillatorRate { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("TexMod_VOscillatorRate")]
    [DefaultValue(0)]
    public float VOscillatorRate { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("TexMod_UOscillatorPhase")]
    [DefaultValue(0)]
    public float UOscillatorPhase { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("TexMod_VOscillatorPhase")]
    [DefaultValue(0)]
    public float VOscillatorPhase { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("TexMod_UOscillatorAmplitude")]
    [DefaultValue(0)]
    public float UOscillatorAmplitude { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    [XmlAttribute("TexMod_VOscillatorAmplitude")]
    [DefaultValue(0)]
    public float VOscillatorAmplitude { get; set; }

    public object Clone() => MemberwiseClone();
}
