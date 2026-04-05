using System.Text.Json.Serialization;
using Bloggi.Backend.EditorJS.Core.Attributes;

namespace Bloggi.Backend.EditorJS.Renderer.Blocks.Quote;

public class QuoteData
{
    [JsonPropertyName("text")] public string Text { get; set; }
    [JsonPropertyName("caption")] public string? Caption { get; set; }
    [JsonPropertyName("alignment")]
    [JsonConverter(typeof(CaseInsensitiveEnumJsonConverter<QuoteAlignment>))]
    public QuoteAlignment Alignment { get; set; } = QuoteAlignment.Left;
    
    public enum QuoteAlignment
    {
        Center,
        Right,
        Left
    } 
}