using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace SynergyLib.FileFormat.CryEngine.CryXml.MaterialElements;

/// <summary>The texture object</summary>
[XmlRoot(ElementName = "Texture")]
public class Texture {
    public enum TypeEnum {
        [XmlEnum("0")]
        Default = 0,

        [XmlEnum("3")]
        Environment = 3,

        [XmlEnum("5")]
        Interface = 5,

        [XmlEnum("7")]
        CubeMap = 7,

        [XmlEnum("Nearest Cube-Map probe for alpha blended")]
        NearestCubeMap = 8
    }

    public enum MapTypeEnum {
        Unknown,
        Diffuse,
        Normals,
        Specular,
        Env,
        Detail,
        Opacity,
        Decal,
        SubSurface,
        Custom,
        Custom2,
    }

    [XmlAttribute(AttributeName = "Map")]
    public string? MapString { get; set; }

    /// <summary>Diffuse, Specular, Bumpmap, Environment, HeightMamp or Custom</summary>
    [XmlIgnore]
    public MapTypeEnum Map {
        get => MapString switch {
            "Diffuse" => MapTypeEnum.Diffuse,
            "Specular" => MapTypeEnum.Specular,
            "Bumpmap" => MapTypeEnum.Normals,
            "Environment" => MapTypeEnum.Env,
            "Detail" => MapTypeEnum.Detail,
            "Opacity" => MapTypeEnum.Opacity,
            "Decal" => MapTypeEnum.Decal,
            "SubSurface" => MapTypeEnum.SubSurface,
            "Custom" => MapTypeEnum.Custom,
            "[1] Custom" => MapTypeEnum.Custom2,
            _ => 0,
        };
        set => MapString = value switch {
            MapTypeEnum.Diffuse => "Diffuse", 
            MapTypeEnum.Specular => "Specular", 
            MapTypeEnum.Normals => "Bumpmap", 
            MapTypeEnum.Env => "Environment", 
            MapTypeEnum.Detail => "Detail", 
            MapTypeEnum.Opacity => "Opacity", 
            MapTypeEnum.Decal => "Decal", 
            MapTypeEnum.SubSurface => "SubSurface", 
            MapTypeEnum.Custom => "Custom", 
            MapTypeEnum.Custom2 => "[1] Custom",
            _ => $"({value})",
        };
    }

    /// <summary>Location of the texture</summary>
    [XmlAttribute(AttributeName = "File")]
    public string? File { get; set; }

    /// <summary>The type of the texture</summary>
    [XmlAttribute(AttributeName = "TexType")]
    [DefaultValue(TypeEnum.Default)]
    public TypeEnum TexType;

    [XmlAttribute(AttributeName = "IsTileU")]
    [DefaultValue(1)]
    public int IsTileUInt { get; set; } = 1;

    [XmlIgnore]
    public bool IsTileU {
        get => IsTileUInt != 0;
        set => IsTileUInt = value ? 1 : 0;
    }

    [XmlAttribute(AttributeName = "IsTileV")]
    [DefaultValue(1)]
    public int IsTileVInt { get; set; } = 1;

    [XmlIgnore]
    public bool IsTileV {
        get => IsTileVInt != 0;
        set => IsTileVInt = value ? 1 : 0;
    }

    /// <summary>The modifier to apply to the texture</summary>
    [XmlElement(ElementName = "TexMod")]
    public TextureModifier? Modifier;
}
