using System.Reflection;
using Bloggi.Backend.Api.Web.Options;
using Microsoft.Extensions.Options;

namespace Bloggi.Backend.Api.Web.Infrastructure.Startup;

/// <summary>
/// Service responsible for handling initialization logic related to embedded files during application startup.
/// This includes extracting and writing embedded resources, such as templates, to the file system.
/// </summary>
public class EmbeddedFilesStartupService(
    IWebHostEnvironment environment,
    IOptions<AppOptions> appOptions,
    ILogger<EmbeddedFilesStartupService> logger
    ) : IHostedService
{
    private const string TemplateIdentifier = "Bloggi.Backend.Api.Web.Assets.index.html";

    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var assembly = Assembly.GetExecutingAssembly();

        var outputDir = Path.Combine(
            appOptions.Value.TemplatePath,
            "default");
        
        var outputPath = Path.Combine(outputDir, "template.html");

        if (Path.Exists(outputPath))
        {
            logger.LogWarning("Template already exists at {OutputPath}. Skipping", outputPath);
            return;
        }
        
        Directory.CreateDirectory(outputDir);
        
        await using var resourceStream = assembly.GetManifestResourceStream(TemplateIdentifier)
                                         ?? throw new FileNotFoundException(
                                             $"Embedded resource '{TemplateIdentifier}' was not found in assembly '{assembly.FullName}'.");

        logger.LogInformation("Writing template to {OutputPath}", outputPath);
        await using var fileStream = new FileStream(
            outputPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None);

        await resourceStream.CopyToAsync(fileStream, cancellationToken);
        logger.LogInformation("Template written to {OutputPath}", outputPath);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}