using System.Text.Json.Serialization;
using Bloggi.Backend.Api.Web.Extensions;
using FastEndpoints;
using FluentValidation;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.Editor.LinkMeta;

internal static partial class LinkMeta
{
    
    private record Response(
        string Link,
        int Success = 1,
        [property: JsonPropertyName("meta")] ResponseMetaData? Meta = null
    );

    private record ResponseMetaData(
        [property: JsonPropertyName("title")] string? Title,
        [property: JsonPropertyName("description")] string? Description,
        [property: JsonPropertyName("image")]ResponseMetaImage? Image
    );

    private record ResponseMetaImage(
        [property: JsonPropertyName("url")] string? Url,
        [property: JsonPropertyName("favicon")] string? Favicon
        );
}