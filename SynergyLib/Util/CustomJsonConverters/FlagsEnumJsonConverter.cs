using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace SynergyLib.Util.CustomJsonConverters;

public class FlagsEnumJsonConverter<T> : JsonConverter<T> where T : unmanaged, Enum {
    [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
    private static readonly (string First, ulong Second)[] NameAndValues;

    [SuppressMessage("ReSharper", "StaticMemberInGenericType")]
    private static readonly (string First, ulong Second)[] NameAndValuesDistinct;

    static FlagsEnumJsonConverter() {
        NameAndValues = Enum.GetNames<T>().Zip(Enum.GetValues<T>().Select(x => Convert.ToUInt64((object?) x))).ToArray();
        NameAndValuesDistinct = NameAndValues.DistinctBy(x => x.Second).ToArray();
    }

    public override void WriteJson(JsonWriter writer, T flags, JsonSerializer serializer) {
        var flagInt = Convert.ToUInt64(flags);
        writer.WriteStartArray();
        foreach (var (name, flag) in NameAndValuesDistinct) {
            if ((flagInt & flag) != 0)
                writer.WriteValue(name);
            flagInt &= ~flag;
        }

        writer.WriteEndArray();
    }

    public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer) {
        if (reader.TokenType != JsonToken.StartArray)
            throw new JsonSerializationException();

        var value = 0ul;
        while (reader.Read()) {
            if (reader.TokenType == JsonToken.EndArray)
                break;
            if (reader.TokenType != JsonToken.String)
                throw new JsonSerializationException();
            var name = (string) reader.Value!;
            value |= NameAndValues.Single(x => string.Equals(x.First, name, StringComparison.OrdinalIgnoreCase)).Second;
        }

        unsafe {
            return *(T*) &value;
        }
    }
}
