using System.Text.Json.Serialization;

namespace Bloggi.Backend.EditorJS.Renderer.Blocks.Button;

public class ButtonData
{
    [JsonPropertyName("text")] public string Text { get; set; }
    [JsonPropertyName("link")] public string Link { get; set; }
}