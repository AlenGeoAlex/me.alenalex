using Microsoft.AspNetCore.Mvc;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.File.GetFile;

internal static partial class GetFile
{
    private record Request(
        [FromRoute] Guid PostId,
        [FromRoute] Guid FileId
    );

    private record Response(
        string Url
    );
}