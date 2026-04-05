using System.Text.Json.Serialization;

namespace Bloggi.Backend.EditorJS.Renderer.Blocks.Header;

public class HeaderData
{
    [JsonPropertyName("level")] public int Level { get; set; }
    
    [JsonPropertyName("text")] public string Text { get; set; }
}