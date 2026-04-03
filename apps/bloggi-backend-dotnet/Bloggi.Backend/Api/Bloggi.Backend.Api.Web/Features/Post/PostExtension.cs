using Bloggi.Backend.Api.Web.Features.Post.Services;

namespace Bloggi.Backend.Api.Web.Features.Post;

public static class PostExtension
{
    public static void AddPostModule(
        this IServiceCollection services,
        IConfigurationManager configurationManager
    )
    {
        services.AddScoped<IPostModule, PostModule>();
        services.AddScoped<PostService>();
        services.AddScoped<PreviewService>();
        services.AddScoped<TagsService>();
        services.AddScoped<PostBlockService>();
        services.AddScoped<PostSeriesService>();
        services.AddScoped<RenderService>();
    }
}