using System.Text.Json.Serialization;

namespace Bloggi.Backend.EditorJS.Renderer.Blocks.Paragraph;

/// <summary>
/// Represents the data structure for a paragraph block in the EditorJS renderer.
/// It contains the text content of the paragraph.
/// </summary>
public record ParagraphData(
    [property: JsonPropertyName("text")] string Text
    );
