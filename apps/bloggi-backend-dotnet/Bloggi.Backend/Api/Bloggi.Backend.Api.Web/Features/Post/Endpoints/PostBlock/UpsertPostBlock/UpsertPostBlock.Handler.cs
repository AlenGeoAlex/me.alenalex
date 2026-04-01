using Bloggi.Backend.Api.Web.Features.Post.Services;
using Bloggi.Backend.EditorJS.Core.Models;
using ErrorOr;
using FastEndpoints;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.PostBlock.UpsertPostBlock;

internal static partial class UpsertPostBlock
{

    class Handler(
        ILogger<Handler> logger,
        PostBlockService postBlockService,
        PostService postService
        ) : Endpoint<Request, ErrorOr<Response>>
    {
        public override async Task<ErrorOr<Response>> ExecuteAsync(Request req, CancellationToken ct)
        {
            var blockIds = await postBlockService.UpsertBlocksAsync(req.PostId, (IReadOnlyList<EditorBlock>)req.EditorJsData.Blocks, ct);
            return new Response(blockIds);
        }

        public override void Configure()
        {
            Post("/post-block/");
            Group<PostBlockGroup>();
            
            Version(1);
            Description(x =>
            {
                x.WithDescription("Create or update a post block");
                x.WithSummary("Endpoint for creating or updating a post block");
                x.WithName("UpsertPostBlock");
                x.Produces<Response>(200);
            });
        }
    }
    
}