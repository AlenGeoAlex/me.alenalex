using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bloggi.Backend.EditorJS.Core;

public enum BlockTypes
{
    Unknown,
    Paragraph,
    Header,
    Link,
    Embed,
    Quote,
    Code,
    Table,
    Warning,
    InlineCode,
    List,
    Image,
}

public static class BlockTypesExtensions
{
    public static string Identifier(this BlockTypes blockType) => blockType
        .ToString()
        .ToLower();
}

/// <summary>
/// A custom JSON converter for the <see cref="BlockTypes"/> enumeration,
/// providing serialization and deserialization functionality to ensure compatibility
/// with the specific data formats used in EditorJS.
/// </summary>
/// <remarks>
/// This converter ensures that during serialization, the <see cref="BlockTypes"/> values
/// are written as lowercase strings matching the format required by EditorJS. Similarly,
/// during deserialization, case-insensitive parsing is performed to map JSON string
/// values back to the corresponding <see cref="BlockTypes"/> enumeration values.
/// </remarks>
/// <seealso cref="BlockTypes"/>
public sealed class BlockTypeJsonConverter : JsonConverter<BlockTypes>
{
    public override BlockTypes Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var raw = reader.GetString();

        if (string.IsNullOrWhiteSpace(raw))
            return BlockTypes.Unknown;

        return Enum.TryParse<BlockTypes>(raw, ignoreCase: true, out var result) ? result : BlockTypes.Unknown;
    }

    public override void Write(
        Utf8JsonWriter writer,
        BlockTypes value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Identifier());
    }
}