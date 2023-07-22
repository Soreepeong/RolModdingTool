using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SynergyLib.FileFormat.CryEngine.CryXml.MaterialElements;

[JsonConverter(typeof(StringEnumConverter))]
public enum SurfaceType {
    [XmlEnum("")]
    Empty,

    [XmlEnum("None")]
    None,

    [XmlEnum("mat_brb_climbable")]
    BrbClimbable,

    [XmlEnum("mat_brb_fence_metal")]
    BrbFenceMetal,

    [XmlEnum("mat_brb_knucklesburrow")]
    BrbKnucklesburrow,

    [XmlEnum("mat_brb_nocamera")]
    BrbNocamera,

    [XmlEnum("mat_brb_sonicramp")]
    BrbSonicramp,

    [XmlEnum("mat_brb_warthog")]
    BrbWarthog,

    [XmlEnum("mat_canopy")]
    Canopy,

    [XmlEnum("mat_default")]
    Default,

    [XmlEnum("mat_dirt")]
    Dirt,

    [XmlEnum("mat_energy_barrier")]
    EnergyBarrier,

    [XmlEnum("mat_grass")]
    Grass,

    [XmlEnum("mat_ice")]
    Ice,

    [XmlEnum("mat_invulnerable")]
    Invulnerable,

    [XmlEnum("mat_metal")]
    Metal,

    [XmlEnum("mat_metal_grate")]
    MetalGrate,

    [XmlEnum("mat_metal_solid")]
    MetalSolid,

    [XmlEnum("mat_nodraw")]
    Nodraw,

    [XmlEnum("mat_player_collider")]
    PlayerCollider,

    [XmlEnum("mat_rock")]
    Rock,

    [XmlEnum("mat_sand")]
    Sand,

    [XmlEnum("mat_stone_breakable")]
    StoneBreakable,

    [XmlEnum("mat_stone_loose")]
    StoneLoose,

    [XmlEnum("mat_stone_solid")]
    StoneSolid,

    [XmlEnum("mat_vegetation")]
    Vegetation,

    [XmlEnum("mat_water")]
    Water,

    [XmlEnum("mat_water_shallow")]
    WaterShallow,

    [XmlEnum("mat_waterfall")]
    Waterfall,

    [XmlEnum("mat_wood")]
    Wood,

    [XmlEnum("mat_wood_breakable")]
    WoodBreakable,

    [XmlEnum("mat_wood_plank")]
    WoodPlank,

    [XmlEnum("mat_wood_solid")]
    WoodSolid,
}
