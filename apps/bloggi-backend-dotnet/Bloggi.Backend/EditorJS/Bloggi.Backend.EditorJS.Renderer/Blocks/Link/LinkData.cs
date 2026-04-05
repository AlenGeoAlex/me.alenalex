using System.Text.Json.Serialization;

namespace Bloggi.Backend.EditorJS.Renderer.Blocks.Link;

public class LinkData
{
    [JsonPropertyName("link")] public string Link { get; set; }
    
    [JsonPropertyName("meta")] public LinkData.LinkMeta? Meta { get; set; }
    
    public record LinkMeta(
        [property: JsonPropertyName("title")] string? Title,
        [property: JsonPropertyName("description")]
        string? Description,
        [property: JsonPropertyName("image")] LinkData.LinkImage? Image
    );
    
    public record LinkImage(
        [property: JsonPropertyName("url")] string? Url,
        [property: JsonPropertyName("favicon")] string? Favicon
    );
}