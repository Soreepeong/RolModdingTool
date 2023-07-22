using System;
using System.Globalization;
using System.Numerics;
using JetBrains.Annotations;
using Newtonsoft.Json;
using SynergyLib.FileFormat.CryEngine;

namespace SynergyLib.Util.CustomJsonConverters;

public class Vector3JsonConverter : JsonConverter<Vector3> {
    private const NumberStyles AllowedStyles = NumberStyles.HexNumber | NumberStyles.Float;

    public Notation ValueNotation;

    public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer) {
        switch (ValueNotation) {
            case Notation.NumberList:
                writer.WriteStartArray();
                writer.WriteValue(value.X);
                writer.WriteValue(value.Y);
                writer.WriteValue(value.Z);
                writer.WriteEndArray();
                break;
            case Notation.NumberString:
                writer.WriteValue(value.ToXmlValue());
                break;
            case Notation.RgbByteString:
            case Notation.HexByteString: {
                var r = (byte) float.Clamp(value.X * 256, 0f, 255f);
                var g = (byte) float.Clamp(value.Y * 256, 0f, 255f);
                var b = (byte) float.Clamp(value.Z * 256, 0f, 255f);
                writer.WriteValue(
                    ValueNotation == Notation.RgbByteString
                        ? $"#{r:X02}{g:X02}{b:X02}"
                        : $"rgb({r}, {g}, {b})");
                break;
            }
            default:
                throw new InvalidOperationException();
        }
    }

    public override Vector3 ReadJson(
        JsonReader reader,
        Type objectType,
        Vector3 existingValue,
        bool hasExistingValue,
        JsonSerializer serializer) {
        switch (reader.TokenType) {
            case JsonToken.StartArray: {
                var x = reader.ReadAsDouble() ?? throw new JsonSerializationException();
                var y = reader.ReadAsDouble() ?? throw new JsonSerializationException();
                var z = reader.ReadAsDouble() ?? throw new JsonSerializationException();
                if (!reader.Read() || reader.TokenType != JsonToken.EndArray)
                    throw new JsonSerializationException();

                return new((float) x, (float) y, (float) z);
            }
            case JsonToken.String:
                var input = (string) reader.Value!;
                var s = input.Trim();
                if (s.StartsWith("#")) {
                    switch (s.Length) {
                        case 4:
                            return new(
                                float.Clamp(int.Parse(s.AsSpan(1, 1), NumberStyles.HexNumber) / 15f, 0f, 1f),
                                float.Clamp(int.Parse(s.AsSpan(2, 1), NumberStyles.HexNumber) / 15f, 0f, 1f),
                                float.Clamp(int.Parse(s.AsSpan(3, 1), NumberStyles.HexNumber) / 15f, 0f, 1f));
                        case 7:
                            return new(
                                float.Clamp(int.Parse(s.AsSpan(1, 2), NumberStyles.HexNumber) / 255f, 0f, 1f),
                                float.Clamp(int.Parse(s.AsSpan(3, 2), NumberStyles.HexNumber) / 255f, 0f, 1f),
                                float.Clamp(int.Parse(s.AsSpan(5, 2), NumberStyles.HexNumber) / 255f, 0f, 1f));
                        default:
                            throw new JsonSerializationException($"Could not be parsed as #RRGGBB or #RGB: {input}");
                    }
                }

                if (s.StartsWith("rgb", StringComparison.OrdinalIgnoreCase)) {
                    var ss = s[3..].Trim();
                    if (ss.StartsWith("(") && ss.EndsWith(")")) {
                        var components = ss[1..^1].Split(",", 4, StringSplitOptions.TrimEntries);
                        Span<float> floats = stackalloc float[3];
                        if (components.Length != 3)
                            throw new JsonSerializationException($"Could not be parsed as rgb(n, n, n): {input}");
                        for (var i = 0; i < 3; i++) {
                            var value = components[i];
                            if (value.Length > 2 && value[0] == '0' && value[1] is 'x' or 'X'
                                && int.TryParse(value, NumberStyles.HexNumber, null, out var valueInt))
                                floats[i] = float.Clamp(valueInt / 255f, 0f, 1f);
                            else if (float.TryParse(value, out var valueFloat))
                                floats[i] = float.Clamp(valueFloat / 255f, 0f, 1f);
                            else if (int.TryParse(value, out valueInt))
                                floats[i] = float.Clamp(valueInt / 255f, 0f, 1f);
                            else
                                throw new JsonSerializationException($"Could not be parsed as a number: {value}");
                        }

                        return new(floats);
                    }
                }

                throw new JsonSerializationException($"Could not be parsed as color: {input}");
            case JsonToken.Boolean:
            case JsonToken.Integer:
            case JsonToken.Float:
                throw new JsonSerializationException(
                    $"Unsupported value for Vector3: {reader.Value} of type {reader.TokenType}");
            default:
                throw new JsonSerializationException();
        }
    }

    public enum Notation {
        NumberList,
        NumberString,
        RgbByteString,
        HexByteString,

        // alias for command line invocation
        [UsedImplicitly]
        Numbers = NumberList,

        [UsedImplicitly]
        String = NumberString,

        [UsedImplicitly]
        Colors = RgbByteString,

        [UsedImplicitly]
        Rgb = RgbByteString,

        [UsedImplicitly]
        Hex = HexByteString,
    }
}
