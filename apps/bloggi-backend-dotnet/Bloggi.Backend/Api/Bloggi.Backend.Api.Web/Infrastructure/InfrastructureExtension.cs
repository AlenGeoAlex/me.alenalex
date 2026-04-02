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
        return serviceCollection;
    }

    public static IServiceCollection AddPipelineServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IFileService, S3FileService>();
        serviceCollection.AddHttpClient();
        return serviceCollection;
    }
}