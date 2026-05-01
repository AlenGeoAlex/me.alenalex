using System.Text.Json;
using Bloggi.Backend.Api.Web.Features.Post.Services;
using ErrorOr;
using FastEndpoints;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.Post.UpdatePostMeta;

internal static partial class UpdatePostMeta
{
    class Handler(
        ILogger<Handler> logger,
        PostService postService
        ) : Endpoint<Request, ErrorOr<object?>>
    {
        public override async Task<ErrorOr<object?>> ExecuteAsync(Request req, CancellationToken ct)
        {
            var postResult = await postService.UpdatePostMetaAsync(new PostService.UpdatePostMeta(
                req.PostId,
                req.OpenGraphTitle,
                req.OpenGraphDescription,
                req.OpenGraphImageUrl,
                req.CanonicalUrl,
                req.Robot,
                string.IsNullOrWhiteSpace(req.SchemaOrgJson) ? JsonElement.Parse("{}")
                    : JsonElement.Parse(req.SchemaOrgJson!)
            ), true, ct);

            if (postResult.IsError)
            {
                return postResult.Errors;
            }

            return new();
        }
        
        public override void Configure()
        {
            Put("/{postId}/meta");
            Group<PostGroup>();
            Description(x =>
            {
                x.WithName("UpdatePostMeta");
                x.WithDescription("Updates metadata for a specific post by its ID");
                x.Produces(200);
                x.Produces(400);
            });
        }
    }
}