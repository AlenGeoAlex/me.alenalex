using System.ComponentModel.DataAnnotations;

namespace Bloggi.Backend.Api.Web.Options;

public sealed class S3FileServiceOptions
{
    public const string SectionName = "S3FileService";

    /// <summary>Client ID used for authenticating and identifying the S3 client.</summary>
    [Required]
    public string ClientId { get; set; } = null!;

    /// <summary>AWS / IAM Secret Access Key.</summary>
    [Required]
    public string ClientSecret { get; set; } = null!;
 
    /// <summary>S3 bucket name. Defaults to <c>bloggi</c>.</summary>
    public string Bucket { get; set; } = "bloggi";

    /// <summary>
    /// Base URL used to build public (CDN / direct) links to stored objects,
    /// e.g. <c>https://cdn.example.com</c> or <c>https://&lt;bucket&gt;.s3.amazonaws.com</c>.
    /// </summary>
    [Required]
    public string PublicUrl { get; set; } = null!;

    /// <summary>
    /// AWS region where the S3 bucket is located.
    /// </summary>
    [Required]
    public string Region { get; set; } = "auto";

    /// <summary>
    /// Custom endpoint URL for the S3-compatible storage service. If not set, the default endpoint is used.
    /// </summary>
    public string? Endpoint { get; set; }
 
    /// <summary>
    /// Default presigned-URL TTL when the caller does not specify one.
    /// Defaults to 15 minutes.
    /// </summary>
    public TimeSpan DefaultPresignedUrlExpiry { get; init; } = TimeSpan.FromMinutes(15);
}