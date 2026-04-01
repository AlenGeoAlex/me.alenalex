using System.Text.Json.Serialization;

namespace Bloggi.Backend.EditorJS.Core.Models;

public class OutputData
{
    [JsonPropertyName("time")] public long Time { get; private set; }
    
    [JsonIgnore] public DateTimeOffset TimeParsed => DateTimeOffset.FromUnixTimeMilliseconds(Time);
    
    [JsonPropertyName("version")] public string Version { get; private set; }

    [JsonPropertyName("blocks")] public IReadOnlySet<EditorBlock> Blocks { get; private set; }
}