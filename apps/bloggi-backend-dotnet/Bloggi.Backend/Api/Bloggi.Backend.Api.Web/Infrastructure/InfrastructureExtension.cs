using Bloggi.Backend.Api.Web.Infrastructure.Context;
using Bloggi.Backend.Api.Web.Infrastructure.Services;
using Bloggi.Backend.Api.Web.Infrastructure.Services.Contracts;

namespace Bloggi.Backend.Api.Web.Infrastructure;

public static class InfrastructureExtension
{
    public static IServiceCollection AddContext(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddHttpContextAccessor();
        serviceCollection.AddScoped<IContextFactory, ApiContextFactory>();
        serviceCollection.AddScoped<TokenService>();
        serviceCollection.AddSingleton<TemplateService>();
        return serviceCollection;
    }

    public static IServiceCollection AddPipelineServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IFileService, S3FileService>();
        serviceCollection.AddHttpClient();
        serviceCollection.AddHttpClient<LinkUnfurlService>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(
                "BloggiBot-blog.alenalex.me/1.0 (+https://blog.alenalex.me/bot)");
        });
        return serviceCollection;
    }
}