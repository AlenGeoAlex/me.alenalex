using System.Reflection;
using Bloggi.Backend.Api.Web.Options;
using Microsoft.Extensions.Options;

namespace Bloggi.Backend.Api.Web.Infrastructure.Services;

/// <summary>
/// Provides services related to handling templates within the Bloggi backend application.
/// This service is responsible for operations that involve using application configuration options
/// and managing template-related functionalities.
/// </summary>
public class TemplateService(
    ILogger<TemplateService> logger,
    IOptionsSnapshot<AppOptions> appOptions
    )
{

    
    private async Task EnsureTemplateExistsAsync()
    {
        logger.LogInformation("Ensuring template exists");
        var templateFolder = appOptions.Value.TemplatePath;
        if(!Path.Exists(templateFolder))
            await CopyTemplatesAsync();
        
        
    }

    private async Task CopyTemplatesAsync()
    {
        logger.LogInformation("Copying templates to {templateFolder}", appOptions.Value.TemplatePath);
        
    }
    
}