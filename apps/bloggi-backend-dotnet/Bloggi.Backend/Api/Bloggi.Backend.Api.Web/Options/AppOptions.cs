using System.ComponentModel.DataAnnotations;

namespace Bloggi.Backend.Api.Web.Options;

public class AppOptions
{
    [Required] public string BaseUrl { get; set; } = "https://blog.alenalex.me";
    
    public string TemplatePath { get; set; } = "/var/bloggi/templates/";
}