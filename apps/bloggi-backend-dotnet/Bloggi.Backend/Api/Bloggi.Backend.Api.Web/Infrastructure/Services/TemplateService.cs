using System.Reflection;
using System.Text.Json.Serialization;
using Bloggi.Backend.Api.Web.Options;
using ErrorOr;
using Microsoft.Extensions.Options;

namespace Bloggi.Backend.Api.Web.Infrastructure.Services;

/// <summary>
/// Provides services related to handling templates within the Bloggi backend application.
/// This service is responsible for operations that involve using application configuration options
/// and managing template-related functionalities.
/// </summary>
public class TemplateService(
    ILogger<TemplateService> logger,
    IOptions<AppOptions> appOptions,
    IHttpClientFactory httpClientFactory
    )
{
    private List<Template> templates = new List<Template>();
    private DateTimeOffset lastUpdated = DateTimeOffset.MinValue;

    public async Task<ErrorOr<string>> GetTemplateMainAsync(string templateName, CancellationToken ct = default)
    {
        var templatesResult = await GetTemplatesAsync(ct);
        if(templatesResult.IsError)
            return templatesResult.Errors;
        
        var template = templatesResult.Value.FirstOrDefault(x => x.Name == templateName);
        if(template == null)
            return Error.Failure("TemplateService.GetTemplateMain", $"Template {templateName} not found");
        
        using var httpClient = httpClientFactory.CreateClient("TemplateService");
        var response = await httpClient.GetAsync(template.Main, ct);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Failed to get template main from {Path}", template.Main);
            await response.Content.ReadAsStringAsync(ct);
            logger.LogError("Response: {Response}", response);
            return Error.Failure("TemplateService.GetTemplateMain", "Failed to get template main");
        }
        
        return await response.Content.ReadAsStringAsync(ct);
    }

    public async Task<ErrorOr<IReadOnlyList<Template>>> GetTemplatesAsync(CancellationToken ct)
    {
        if (DateTimeOffset.UtcNow - lastUpdated < TimeSpan.FromMinutes(3))
            return templates;

        var templateOptionPath = appOptions.Value.TemplateJson;
        using var httpClient = httpClientFactory.CreateClient("TemplateService");
        var response = await httpClient.GetAsync(templateOptionPath, ct);
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("Failed to get templates from {Path}", templateOptionPath);
            await response.Content.ReadAsStringAsync(ct);
            logger.LogError("Response: {Response}", response);
            return Error.Failure("TemplateService.GetTemplates", "Failed to get templates");
        }
        
        var template = await response.Content.ReadFromJsonAsync<List<Template>>(ct);
        if (template == null)
            return Error.Failure("TemplateService.GetTemplates", "Failed to parse templates");
        
        if(template.Count == 0)
            return Error.Failure("TemplateService.GetTemplates", "No templates found");
        
        templates = template;
        lastUpdated = DateTimeOffset.UtcNow;
        return templates;
    }
}

public record Template(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("main")] string Main,
    [property: JsonPropertyName("assets")] string[] Assets
    );