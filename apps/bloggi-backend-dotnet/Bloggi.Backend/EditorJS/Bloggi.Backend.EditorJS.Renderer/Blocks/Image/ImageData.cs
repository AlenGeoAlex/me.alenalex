using System.Text.Json.Serialization;

namespace Bloggi.Backend.EditorJS.Renderer.Blocks.Image;

public class ImageData
{

    [JsonPropertyName("file")] public ImageData.FileData File { get; set; } = null!;
    [JsonPropertyName("caption")] public string? Caption { get; set; }
    [JsonPropertyName("stretched")] public bool Stretched { get; set; }
    [JsonPropertyName("withBorder")] public bool WithBorder { get; set; }
    [JsonPropertyName("withBackground")] public bool WithBackground { get; set; }

    public class FileData
    {
        [JsonPropertyName("url")] public string Url { get; set; }
    }
}
