using Microsoft.AspNetCore.Mvc;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.File.DeleteFile;

internal static partial class DeleteFile
{
    private record Request(
        [FromRoute] Guid PostId,
        [FromRoute] Guid FileId
        );
}