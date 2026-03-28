using System.ComponentModel.DataAnnotations;

namespace Bloggi.Backend.Api.Web.Options;

public class GoogleOAuthOptions
{
    public const string SectionName = "GoogleOAuth";
    
    [Required] public required string ClientId { get; set; }
    [Required] public required string RedirectUri { get; set; }
    public bool AllowRegistration { get; set; }
}