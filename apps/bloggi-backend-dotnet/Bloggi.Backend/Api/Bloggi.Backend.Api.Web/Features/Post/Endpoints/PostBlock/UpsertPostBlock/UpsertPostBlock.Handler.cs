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
            var upsertBlockResult = await postBlockService.UpsertBlocksAsync(new PostBlockService.UpsertBlockDataRequest(req.PostId, req.EditorJsData.Version, (IReadOnlyList<EditorBlock>)req.EditorJsData.Blocks), ct);
            if (upsertBlockResult.IsError)
                return upsertBlockResult.Errors;
            var upsertBlock = upsertBlockResult.Value;
            return new Response(upsertBlock.BlockId);
        }

        public override void Configure()
        {
            Post("/");
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