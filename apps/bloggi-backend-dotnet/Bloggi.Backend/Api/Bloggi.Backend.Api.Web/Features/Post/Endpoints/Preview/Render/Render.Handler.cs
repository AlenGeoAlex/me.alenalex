using System.Text;
using Bloggi.Backend.Api.Web.Features.Post.Endpoints.Post;
using Bloggi.Backend.Api.Web.Features.Post.Services;
using ErrorOr;
using FastEndpoints;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.Preview.Render;

internal static partial class Render
{

    class Handler(
        ILogger<Handler> logger,
        RenderService renderService
        ) : Endpoint<Request, ErrorOr<Response>>
    {
        public override async Task<ErrorOr<Response>> ExecuteAsync(Request req, CancellationToken ct)
        {
            var renderResult = await renderService.RenderAsync(new RenderRequest(req.PostId, true), ct);
            if(renderResult.IsError)
                return renderResult.Errors;

            var renderValue = renderResult.Value;
            return new Response(
                Encoding.UTF8.GetBytes(renderValue.Html),
                renderValue.Errors.Where(x => x.HasValue)
                    .Select(x => new ResponseError(x!.Value.Code, x.Value.Description, x.Value.Type.ToString())).ToArray()
                );
        }

        public override void Configure()
        {
            Get("/{postId:guid}/preview");
            Description(x =>
            {
                x.WithSummary("Render a post preview");
                x.WithName("Preview");
                x.WithDescription("Render a post preview");
                x.Produces<Response>();
            });
            Group<PostGroup>();
        }
    }
    
}