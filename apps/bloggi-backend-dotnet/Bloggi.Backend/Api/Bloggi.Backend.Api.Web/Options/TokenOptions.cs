namespace Bloggi.Backend.Api.Web.Options;

public class TokenOptions
{
    public string Secret { get; set; } = string.Empty;
    public string? EncryptionKey { get; set; }
    public string? Issuer { get; set; }
    public int ExpiryHours { get; set; } = 12;
}