using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace SynergyLib.FileFormat.CryEngine.CryXml;

[JsonConverter(typeof(ParsedGenMaskJsonConverter))]
public class ParsedGenMask : IEnumerable<string> {
    private readonly HashSet<string> _items;
    public bool UseBumpMap;
    public bool UseSpecularMap;
    public bool UseScatterInNormalMap;
    public bool UseHeightInNormalMap;
    public bool UseSpecAlphaInDiffuseMap;
    public bool UseGlossInSpecularMap;
    public bool UseNormalMapInDetailMap;
    public bool UseInvertedBlendMap;
    public bool UseGlowDecalMap;
    public bool UseBlendSpecularInSubSurfaceMap;
    public bool UseBlendDiffuseInCustomMap;
    public bool UseDirtLayerInCustomMap2;
    public bool UseAddNormalInCustomMap2;
    public bool UseBlurRefractionInCustomMap2;
    public bool UseVertexColors;

    public ParsedGenMask() => _items = new();

    public ParsedGenMask(string? stringGenMask) : this(
        (stringGenMask ?? "").Split("%", StringSplitOptions.RemoveEmptyEntries)) { }

    public ParsedGenMask(IEnumerable<string> genMaskSet) {
        _items = genMaskSet.ToHashSet();
        UseBumpMap = _items.Remove("BUMP_MAP");
        UseSpecularMap = _items.Remove("GLOSS_MAP");
        UseScatterInNormalMap = _items.Remove("TEMP_SKIN");
        UseHeightInNormalMap = _items.Remove("BLENDHEIGHT_DISPL");
        UseSpecAlphaInDiffuseMap = _items.Remove("GLOSS_DIFFUSEALPHA");
        UseGlossInSpecularMap = _items.Remove("SPECULARPOW_GLOSSALPHA");
        UseNormalMapInDetailMap = _items.Remove("DETAIL_TEXTURE_IS_NORMALMAP");
        UseInvertedBlendMap = _items.Remove("BLENDHEIGHT_INVERT");
        UseGlowDecalMap = _items.Remove("DECAL_ALPHAGLOW");
        UseBlendSpecularInSubSurfaceMap = _items.Remove("BLENDSPECULAR");
        UseBlendDiffuseInCustomMap = _items.Remove("BLENDLAYER");
        UseDirtLayerInCustomMap2 = _items.Remove("DIRTLAYER");
        UseAddNormalInCustomMap2 = _items.Remove("BLENDNORMAL_ADD");
        UseBlurRefractionInCustomMap2 = _items.Remove("BLUR_REFRACTION");
        UseVertexColors = _items.Remove("VERTCOLORS");
    }

    public IEnumerator<string> GetEnumerator() {
        if (UseBumpMap) yield return "BUMP_MAP";
        if (UseSpecularMap) yield return "GLOSS_MAP";
        if (UseScatterInNormalMap) yield return "TEMP_SKIN";
        if (UseHeightInNormalMap) yield return "BLENDHEIGHT_DISPL";
        if (UseSpecAlphaInDiffuseMap) yield return "GLOSS_DIFFUSEALPHA";
        if (UseGlossInSpecularMap) yield return "SPECULARPOW_GLOSSALPHA";
        if (UseNormalMapInDetailMap) yield return "DETAIL_TEXTURE_IS_NORMALMAP";
        if (UseInvertedBlendMap) yield return "BLENDHEIGHT_INVERT";
        if (UseGlowDecalMap) yield return "DECAL_ALPHAGLOW";
        if (UseBlendSpecularInSubSurfaceMap) yield return "BLENDSPECULAR";
        if (UseBlendDiffuseInCustomMap) yield return "BLENDLAYER";
        if (UseDirtLayerInCustomMap2) yield return "DIRTLAYER";
        if (UseAddNormalInCustomMap2) yield return "BLENDNORMAL_ADD";
        if (UseBlurRefractionInCustomMap2) yield return "BLUR_REFRACTION";
        if (UseVertexColors) yield return "VERTCOLORS";
        foreach (var item in _items)
            yield return item;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public override string ToString() {
        var r = string.Join('%', this);
        return r == "" ? "" : "%" + r;
    }

    public class ParsedGenMaskJsonConverter : JsonConverter {
        public override void WriteJson(JsonWriter writer, object? untypedValue, JsonSerializer serializer) {
            if (untypedValue is not ParsedGenMask value)
                throw new JsonSerializationException();
            
            writer.WriteStartArray();
            foreach (var v in value)
                writer.WriteValue(v);
            writer.WriteEndArray();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) {
            if (reader.TokenType != JsonToken.StartArray)
                throw new JsonSerializationException();
            var values = new List<string>();
            while (reader.Read()) {
                if (reader.TokenType == JsonToken.EndArray)
                    break;
                if (reader.TokenType != JsonToken.String)
                    throw new JsonSerializationException();
                values.Add((string) reader.Value!);
            }

            return new ParsedGenMask(values);
        }

        public override bool CanConvert(Type objectType) => typeof(ParsedGenMask) == objectType;
    }
}
