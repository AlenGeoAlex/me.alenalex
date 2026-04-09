using Bloggi.Backend.Api.Web.Features.Post.Services;
using ErrorOr;
using FastEndpoints;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.Post.UpdatePost;

internal static partial class UpdatePost
{
    class Handler(
        ILogger<Handler> logger,
        PostService postService,
        TagsService tagsService
        ) : Endpoint<UpdatePost.Request, ErrorOr<UpdatePost.Response>>
    {
        public override async Task<ErrorOr<Response>> ExecuteAsync(Request req, CancellationToken ct)
        {
            var updatePostResult = await postService.UpdatePostAsync(new PostService.UpdatePostRequest(req.PostId, req.Title, req.Excerpt), ct: ct);
            if(updatePostResult.IsError)
                return updatePostResult.Errors;

            var tagResponseResult = await tagsService.UpsertTagsAndGetIdAsync(new TagsService.UpsertTagsAndGetIdRequest(req.Tags), ct);
            if(tagResponseResult.IsError)
                return tagResponseResult.Errors;

            var tagIds = tagResponseResult.Value.Select(x => x.Id).ToArray();

            var tagsResult = await postService.LinkPostTagsAsync(new PostService.LinkPostTagRequest(req.PostId, tagIds), ct: ct);
            if(tagsResult.IsError)
                return tagsResult.Errors;
            
            return new Response(req.PostId);
        }

        public override void Configure()
        {
            Put("{postId:guid}");
            Group<PostGroup>();
            Description(x =>
            {
                x.WithSummary("Update a post");
                x.WithName("UpdatePost");
                x.WithDescription("Update a post");
                x.Produces<Response>(200);
            });
        }
    }
}