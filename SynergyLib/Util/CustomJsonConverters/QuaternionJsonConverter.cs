using System;
using System.Numerics;
using Newtonsoft.Json;

namespace SynergyLib.Util.CustomJsonConverters;

public class QuaternionJsonConverter : JsonConverter<Quaternion> {
    public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer serializer) {
        writer.WriteStartArray();
        writer.WriteValue(value.X);
        writer.WriteValue(value.Y);
        writer.WriteValue(value.Z);
        writer.WriteValue(value.W);
        writer.WriteEndArray();
    }

    public override Quaternion ReadJson(
        JsonReader reader,
        Type objectType,
        Quaternion existingValue,
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
