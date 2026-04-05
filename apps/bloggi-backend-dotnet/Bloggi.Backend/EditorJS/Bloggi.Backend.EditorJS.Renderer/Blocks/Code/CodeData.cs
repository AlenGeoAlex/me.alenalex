using System.Text.Json.Serialization;

namespace Bloggi.Backend.EditorJS.Renderer.Blocks.Code;

public class CodeData
{
    [JsonPropertyName("code")]
    public string Code { get; set; }
}