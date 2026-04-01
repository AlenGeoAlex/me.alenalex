using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Bloggi.Backend.EditorJS.Core.Utils;

public static class BlockHash
{
    public static string Compute(JsonElement data)
    {
        var canonical = JsonSerializer.Serialize(data, CachedOptions);
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static readonly JsonSerializerOptions CachedOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}