using Microsoft.AspNetCore.Mvc;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.PostBlock.GetPostBlock;

internal static partial class GetPostBlock
{
    private record Request(
        [FromRoute] Guid PostId
    );
    
    private record Response(
        EditorJS.Core.Models.OutputData OutputData
    );
}