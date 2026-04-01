using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bloggi.Backend.EditorJS.Core.Models;
public class EditorBlock(string id, BlockTypes type, JsonElement data)
{
    [JsonPropertyName("id")]
    public string Id { get; protected set; } = id;

    [JsonPropertyName("type")]
    [JsonConverter(typeof(BlockTypeJsonConverter))]
    public BlockTypes Type { get; protected set; } = type;

    [JsonPropertyName("data")]
    public JsonElement Data { get; protected set; } = data;

    public TData As<TData>(JsonSerializerOptions? options = null) =>
        Data.Deserialize<TData>(options ?? JsonSerializerOptions.Default)
        ?? throw new InvalidOperationException(
            $"Block {Id} ({Type}) could not be deserialize as {typeof(TData).Name}.");
}

public class EditorBlock<TData> : EditorBlock where TData : class
{
    public TData TypedData { get; private set; } = null!;

    public EditorBlock(EditorBlock raw) : base(raw.Id, raw.Type, raw.Data)
    {
        Id   = raw.Id;
        Type = raw.Type;
        Data = raw.Data;
        TypedData = raw.As<TData>() 
                    ?? throw new InvalidOperationException(
                        $"Block {raw.Id} data could not be deserialize as {typeof(TData).Name}.");
    }
}