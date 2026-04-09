using Microsoft.AspNetCore.Mvc;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.Post.GetFiles;

internal static partial class GetFiles
{
    private record Request(
        [property: FromRoute] Guid PostId
    );

    private record Response(
        Guid PostId,
        File[] Files
    );
    
    private record File(
        Guid FileId,
        string Name,
        string Type,
        long Size,
        string Hash,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt
    );
}