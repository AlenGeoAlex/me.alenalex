using System.Text.Json.Serialization;

namespace Bloggi.Backend.EditorJS.Core.Models;

public class OutputData
{
    [JsonPropertyName("time")] public long Time { get; set; }
    
    [JsonIgnore] public DateTimeOffset TimeParsed => DateTimeOffset.FromUnixTimeMilliseconds(Time);
    
    [JsonPropertyName("version")] public string Version { get; set; }

    [JsonPropertyName("blocks")] public List<EditorBlock> Blocks { get; set; }
}