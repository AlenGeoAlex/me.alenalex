using Bloggi.Backend.Api.Web.Features.Post.Services;
using Bloggi.Backend.EditorJS.Core.Models;
using ErrorOr;
using FastEndpoints;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.PostBlock.GetPostBlock;

internal static partial class GetPostBlock
{
    class Handler(
        ILogger<Handler> logger,
        PostBlockService postBlockService
        ) : Endpoint<Request, ErrorOr<Response>>
    {
        public override async Task<ErrorOr<Response>> ExecuteAsync(Request req, CancellationToken ct)
        {
            var postBlocksResponse = await postBlockService.GetBlocksForPostAsync(new PostBlockService.GetBlocksForPostRequest(req.PostId), ct);
            
            if(postBlocksResponse.IsError)
                return postBlocksResponse.Errors;
            
            var postBlocks = postBlocksResponse.Value;
            var data = new OutputData()
            {
                Blocks = [..postBlocks.Blocks],
                Version = postBlocks.EditorVersion!,
                Time = postBlocks.LastUpdateOn.GetValueOrDefault(DateTimeOffset.Now).ToUnixTimeMilliseconds()
            };
            return new Response(data);
        }

        public override void Configure()
        {
            Get("");
            Description(x =>
            {
                x.WithDescription("Get all the blocks of a post");
                x.Produces<Response>(200);
                x.WithName("GetPostBlocksByPostId");
            });
            Group<PostBlockGroup>();
        }
    }
}