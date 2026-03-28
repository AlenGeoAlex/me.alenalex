using Bloggi.Backend.Api.Web.Features.Glossary.Services;

namespace Bloggi.Backend.Api.Web.Features.Glossary;

public static class GlossaryExtension
{
    public static void AddGlossaryModule(
        this IServiceCollection services,
        IConfigurationManager configurationManager
        )
    {
        services.AddScoped<IGlossaryModule, GlossaryModule>();
        services.AddScoped<GlossaryService>();
    }
}