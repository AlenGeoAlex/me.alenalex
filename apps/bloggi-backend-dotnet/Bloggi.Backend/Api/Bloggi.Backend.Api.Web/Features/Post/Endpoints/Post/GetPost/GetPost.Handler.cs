using Bloggi.Backend.Api.Web.Attributes;
using Bloggi.Backend.Api.Web.Extensions;
using Bloggi.Backend.Api.Web.Features.Post.Services;
using Bloggi.Backend.Api.Web.Features.User;
using ErrorOr;
using FastEndpoints;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.Post.GetPost;

internal static partial class GetPost
{
    class Handler(
        ILogger<Handler> logger,
        PostService postService,
        RenderService renderService,
        IUserModule userModule
        ) : Endpoint<Request, ErrorOr<Response>>
    {
        public override async Task<ErrorOr<Response>> ExecuteAsync(Request req, CancellationToken ct)
        {
            // var res = await renderService.RenderAsync(new RenderRequest(req.Id, true), ct: ct);
            // if (res.IsError)
            //     return res.Errors;
            //
            // var html = res.Value;
            
            logger.LogInformation("Getting post {Id}", req.Id);
            var include = HttpContext.Request.Query.ParseEnumArray<GetPostIncludeProperty>(nameof(req.Include));
            var postSummary = await postService.GetPostSummaryAsync(req.Id, includeTags: include.Contains(GetPostIncludeProperty.Tags), ct: ct);
            if (postSummary.IsError)
                return postSummary.Errors;

            var post = postSummary.Value;
            var response = new Response(
                post.Id,
                post.Title,
                post.Excerpt,
                post.Slug,
                post.Tags.Select(t => new TagDto(t.TagId, t.Slug, t.DisplayName)).ToArray(),
                Enum.Parse<StatusDto>(post.Status.ToString()),
                post.RenderedKey,
                post.CreatedAt,
                post.UpdatedAt,
                post.PublishedAt,
                null
            );

            if (include.Contains(GetPostIncludeProperty.Author))
            {
                var userId = await userModule.GetUserAsync(post.AuthorId, ct);
                if (userId.IsError)
                {
                    logger.LogError("Failed to get user {Id} due to {Error}", post.AuthorId, userId.Errors.First().Description);
                    return userId.Errors;
                }

                var userBase = userId.Value;
                response = response with
                {
                    Author = new AuthorDto(userBase.Id, userBase.Name, userBase.AvatarUrl, userBase.Email)
                };
            }
            
            return response;
        }

        public override void Configure()
        {
            Get("{id:guid}");
            Group<PostGroup>();
            Version(1);
            Description(x =>
            {
                x.WithSummary("Get a post by ID");
                x.WithName("GetPost");
                x.Produces<Response>(200);
                x.ProducesProblemDetails();
            });
        }
    }
}