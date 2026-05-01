using Bloggi.Backend.Api.Web.Features.Post.Services;
using ErrorOr;
using FastEndpoints;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.Post.GetMeta;

internal static partial class GetMeta
{
    class Handler(
        ILogger<Handler> logger,
        PostService postService
        ) : Endpoint<Request, ErrorOr<Response>>
    {
        public override async Task<ErrorOr<Response>> ExecuteAsync(Request req, CancellationToken ct)
        {
            var postInfo = await postService.GetPostAsync(new PostService.GetPostByIdRequest(
                req.PostId,
                false,
                false,
                true,
                false,
                false
            ), ct);
            
            if(postInfo.IsError)
                return postInfo.Errors;

            var postByIdResponse = postInfo.Value;
            var getPostMetaDto = postByIdResponse.Meta;
            if(getPostMetaDto is null)
                return Error.Unexpected(nameof(GetMeta)+":"+nameof(ExecuteAsync), "Failed to get meta data for post");

            return new Response(
                req.PostId,
                getPostMetaDto.OpenGraphTitle,
                getPostMetaDto.OpenGraphDescription,
                getPostMetaDto.OpenGraphImageUrl,
                getPostMetaDto.CanonicalUrl,
                getPostMetaDto.Robot,
                getPostMetaDto.JsonLd,
                string.Empty
            );
        }

        public override void Configure()
        {
            Get("{postId:guid}/meta");
            Group<PostGroup>();
            Description(x =>
            {
                x.WithName("GetPostMeta");
                x.WithSummary("Get meta data for a post");
                x.Produces<Response>();
            });
        }
    }
}