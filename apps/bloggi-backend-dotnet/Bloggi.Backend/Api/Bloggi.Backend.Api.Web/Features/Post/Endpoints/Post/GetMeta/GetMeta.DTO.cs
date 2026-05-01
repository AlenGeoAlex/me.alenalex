namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.Post.GetMeta;

internal static partial class GetMeta
{
    private record Request(
        Guid PostId
    );

    private record Response(
        Guid PostId,
        string? OpenGraphTitle,
        string? OpenGraphDescription,
        string? OpenGraphImageUrl,
        string? CanonicalUrl,
        string Robot,
        object? SchemaOrgJson,
        string EditorVersion
    );
    
    
}