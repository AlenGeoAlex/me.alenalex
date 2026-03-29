using Bloggi.Backend.Api.Web.Features.Post.Services;
using Bloggi.Backend.Api.Web.Infrastructure.Context;
using ErrorOr;
using FastEndpoints;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.Post.CreatePost;

internal static partial class CreatePost
{

    class Handler(
        ILogger<Handler> logger,
        PostService postService,
        TagsService tagService,
        IContextFactory contextFactory
        ) : Endpoint<Request, ErrorOr<Response>>
    {
        public override async Task<ErrorOr<Response>> ExecuteAsync(Request req, CancellationToken ct)
        {
            var context = contextFactory.Context;
            var userId = context.UserId;
            var postId = await postService.CreatePostAsync(new PostService.CreatePostRequest(req.Title, req.Excerpt, userId), ct: ct);
            if (postId.IsError)
                return postId.Errors;

            // This can be not a critical problem, so we don't care about it'
            var tagCollectionResult = await tagService.UpsertTagsAndGetIdAsync(new TagsService.UpsertTagsAndGetIdRequest(req.Tags), ct);
            if (tagCollectionResult.IsError)
            {
                logger.LogWarning("Failed to upsert tags for post {PostId} due to {Error}", postId.Value.PostId, tagCollectionResult.Errors.First().Description);
                return new Response(postId.Value.PostId);
            }

            var tagLinkingResult = await postService.LinkPostTagsAsync(new PostService.LinkPostTagRequest(
                postId.Value.PostId,
                tagCollectionResult.Value.Select(x => x.Id).ToArray()
            ), ct: ct);

            if (tagLinkingResult.IsError)
            {
                logger.LogWarning("Failed to link tags for post {PostId} due to {Error}", postId.Value.PostId, tagLinkingResult.Errors.First().Description);
                return new Response(postId.Value.PostId);
            }
            
            return new Response(postId.Value.PostId);
        }

        public override void Configure()
        {
            Post("/post");
            Version(1);
            Description(x =>
            {
                x.WithDescription("Create a new post");
                x.WithName("CreatePost");
                x.Produces<Response>(200);
            });
        }
    }
    
    
}