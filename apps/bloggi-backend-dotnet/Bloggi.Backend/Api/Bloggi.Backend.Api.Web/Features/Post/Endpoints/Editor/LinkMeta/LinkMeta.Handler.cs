using Bloggi.Backend.Api.Web.Infrastructure.Services;
using ErrorOr;
using FastEndpoints;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.Editor.LinkMeta;

internal static partial class LinkMeta
{

    class Handler(
        ILogger<Handler> logger,
        LinkUnfurlService linkUnfurlService
        ) : EndpointWithoutRequest<ErrorOr<Response>>
    {
        
        
        
        public override async Task<ErrorOr<Response>> ExecuteAsync( CancellationToken ct)
        {
            if (!HttpContext.Request.Query.TryGetValue("url", out var url) || string.IsNullOrWhiteSpace(url))
            {
                return Error.Failure("Url.Missing", "Url is missing");
            }
            
            logger.LogInformation("Crawling meta data for url: {Url}", url!);
            var unfurlResult = await linkUnfurlService.UnfurlAsync(url!, ct);
            if (unfurlResult.IsError)
            {
                return new Response(
                    url!,
                    0,
                    null
                );
            }
            
            var unfurl = unfurlResult.Value;
            
            return new Response(
                Link: unfurl.Url,
                1,
                new ResponseMeta(
                    unfurl.Title,
                    unfurl.Description,
                    new ResponseMetaImage(
                        unfurl.Image,
                        unfurl.Favicon
                    )
                )
            );
        }

        public override void Configure()
        {
            Get("/crawl-meta");
            Group<EditorGroup>();
            Description(x =>
            {
                x.WithDescription("Crawl meta data for a url");
                x.WithSummary("Crawl meta data for a url");
                x.WithName("CrawlMeta");
                x.Produces<Response>(200);
                x.Produces<ProblemDetails>(404);
                x.Produces<ProblemDetails>(400);
                x.Produces<ProblemDetails>(500);
                x.Produces<ProblemDetails>(403);
            });
        }
    }
    
}