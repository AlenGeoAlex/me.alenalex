using System.Text.Json.Serialization;

namespace Bloggi.Backend.EditorJS.Renderer.Blocks.Warning;

public record WarningData(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("message")] string Message
    );