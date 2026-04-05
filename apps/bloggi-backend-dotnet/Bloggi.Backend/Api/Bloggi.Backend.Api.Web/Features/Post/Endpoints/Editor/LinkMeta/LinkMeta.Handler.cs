using Bloggi.Backend.Api.Web.Infrastructure.Services;
using ErrorOr;
using FastEndpoints;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.Editor.LinkMeta;

internal static partial class LinkMeta
{

    class Handler(
        LinkUnfurlService linkUnfurlService
        ) : Endpoint<Request, ErrorOr<Response>>
    {
        public override async Task<ErrorOr<Response>> ExecuteAsync(Request req, CancellationToken ct)
        {
            var unfurlResult = await linkUnfurlService.UnfurlAsync(req.Url, ct);
            if(unfurlResult.IsError)
                return unfurlResult.Errors;
            
            var unfurl = unfurlResult.Value;
            return new Response(unfurl.Title, unfurl.Description, unfurl.Image, unfurl.Favicon);
        }

        public override void Configure()
        {
            Post("/crawl-meta");
            Group<EditorGroup>();
            Description(x =>
            {
                x.WithDescription("Crawl meta data for a url");
                x.WithSummary("Crawl meta data for a url");
                x.WithName("CrawlMeta");
            });
        }
    }
    
}