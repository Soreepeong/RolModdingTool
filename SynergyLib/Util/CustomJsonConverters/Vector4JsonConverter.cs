using System;
using System.Numerics;
using Newtonsoft.Json;

namespace SynergyLib.Util.CustomJsonConverters;

public class Vector4JsonConverter : JsonConverter<Vector4> {
    public override void WriteJson(JsonWriter writer, Vector4 value, JsonSerializer serializer) {
        writer.WriteStartArray();
        writer.WriteValue(value.X);
        writer.WriteValue(value.Y);
        writer.WriteValue(value.Z);
        writer.WriteValue(value.W);
        writer.WriteEndArray();
    }

    public override Vector4 ReadJson(
        JsonReader reader,
        Type objectType,
        Vector4 existingValue,
        bool hasExistingValue,
        JsonSerializer serializer) {
        if (reader.TokenType != JsonToken.StartArray)
            throw new JsonSerializationException();

        var x = reader.ReadAsDouble() ?? throw new JsonSerializationException();
        var y = reader.ReadAsDouble() ?? throw new JsonSerializationException();
        var z = reader.ReadAsDouble() ?? throw new JsonSerializationException();
        var w = reader.ReadAsDouble() ?? throw new JsonSerializationException();
        if (!reader.Read() || reader.TokenType != JsonToken.EndArray)
            throw new JsonSerializationException();

        return new((float) x, (float) y, (float) z, (float) w);
    }
}