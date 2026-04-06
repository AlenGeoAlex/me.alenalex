using System.Text.Json.Serialization;

namespace Bloggi.Backend.EditorJS.Renderer.Blocks.Math;

public record MathData(
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("latex")] string Latex
    );