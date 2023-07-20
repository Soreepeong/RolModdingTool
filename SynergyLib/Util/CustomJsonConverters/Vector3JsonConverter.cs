using System;
using System.Numerics;
using Newtonsoft.Json;

namespace SynergyLib.Util.CustomJsonConverters;

public class Vector3JsonConverter : JsonConverter<Vector3> {
    public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer) {
        writer.WriteStartArray();
        writer.WriteValue(value.X);
        writer.WriteValue(value.Y);
        writer.WriteValue(value.Z);
        writer.WriteEndArray();
    }

    public override Vector3 ReadJson(
        JsonReader reader,
        Type objectType,
        Vector3 existingValue,
        bool hasExistingValue,
        JsonSerializer serializer) {
        if (reader.TokenType != JsonToken.StartArray)
            throw new JsonSerializationException();

        var x = reader.ReadAsDouble() ?? throw new JsonSerializationException();
        var y = reader.ReadAsDouble() ?? throw new JsonSerializationException();
        var z = reader.ReadAsDouble() ?? throw new JsonSerializationException();
        if (!reader.Read() || reader.TokenType != JsonToken.EndArray)
            throw new JsonSerializationException();

        return new((float) x, (float) y, (float) z);
    }
}