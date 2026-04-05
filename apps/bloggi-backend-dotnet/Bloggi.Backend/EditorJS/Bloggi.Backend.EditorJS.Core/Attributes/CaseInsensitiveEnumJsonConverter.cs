namespace Bloggi.Backend.EditorJS.Core.Attributes;

using System.Text.Json;
using System.Text.Json.Serialization;

public sealed class CaseInsensitiveEnumJsonConverter<TEnum> : JsonConverter<TEnum>
    where TEnum : struct, Enum
{
    public override TEnum Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            return default;

        var raw = reader.GetString();

        if (string.IsNullOrWhiteSpace(raw))
            return default;

        return Enum.TryParse<TEnum>(raw, ignoreCase: true, out var result)
            ? result
            : default;
    }

    public override void Write(
        Utf8JsonWriter writer,
        TEnum value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}