using System.Text.RegularExpressions;
using ErrorOr;
using HtmlAgilityPack;
using ZiggyCreatures.Caching.Fusion;

namespace Bloggi.Backend.Api.Web.Infrastructure.Services;

public record LinkPreview(
    string Url,
    string? Title,
    string? Description,
    string? Image,
    string? Favicon
);

public class LinkUnfurlService(
    HttpClient httpClient,
    ILogger<LinkUnfurlService> logger,
    IFusionCache cache
    )
{
    private const string BotName = "BloggiBot";
    private const string CacheKey = "link-unfurl";
    
    
    public async Task<ErrorOr<LinkPreview>> UnfurlAsync(string url, CancellationToken ct = default)
    {
        try
        {
            var cacheKey = $"{CacheKey}:{url}";
            var previewCached = await cache.GetOrDefaultAsync<LinkPreview>(cacheKey, token: ct);
            if (previewCached is not null)
            {
                logger.LogInformation("Link preview retrieved from cache for {Url}", url);
                return previewCached;
            }
            var uri = new Uri(url);

            var allowed = await IsAllowedByRobotsAsync(uri, ct);
            if (!allowed)
            {
                logger.LogInformation("BloggiBot blocked by robots.txt for {Host}", uri.Host);
                return Errors.Infrastructure.Unfurl.BlockedByRobots;
            }
            
            var response = await httpClient.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
            {
                if(response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return Errors.Infrastructure.Unfurl.FailedToFetch;
                
                return Errors.Infrastructure.Unfurl.FailedToUnfurl;
            }

            var html = await response.Content.ReadAsStringAsync(ct);
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var head = doc.DocumentNode.SelectSingleNode("//head");
            if (head is null)
            {
                logger.LogWarning("Head element not found");
                return Errors.Infrastructure.Unfurl.NoHeadToUnfurl;
            }

            string? GetMeta(string property, string attribute = "property")
                => head.SelectSingleNode($"//meta[@{attribute}='{property}']")
                       ?.GetAttributeValue("content", null);

            var title       = GetMeta("og:title")
                           ?? GetMeta("twitter:title", "name")
                           ?? head.SelectSingleNode("//title")?.InnerText?.Trim();

            var description = GetMeta("og:description")
                           ?? GetMeta("twitter:description", "name")
                           ?? GetMeta("description", "name");

            var image       = GetMeta("og:image")
                           ?? GetMeta("twitter:image", "name");

            var favicon     = head.SelectSingleNode("//link[@rel='icon' or @rel='shortcut icon']")
                                  ?.GetAttributeValue("href", null)
                           ?? new Uri(url).GetLeftPart(UriPartial.Authority) + "/favicon.ico";

            if (image is not null && !image.StartsWith("http"))
            {
                var base64 = new Uri(url);
                image = new Uri(base64, image).ToString();
            }

            var linkPreview = new LinkPreview(uri.ToString(), title, description, image, favicon);
            await cache.SetAsync(cacheKey, linkPreview, TimeSpan.FromMinutes(60), token: ct);
            return linkPreview;
        }
        catch(Exception e)
        {
            logger.LogError(e, "Failed to unfurl link {Url}", url);
            return Errors.Infrastructure.Unfurl.FailedToUnfurl;
        }
    }
    
    private async Task<bool> IsAllowedByRobotsAsync(Uri uri, CancellationToken ct)
    {
        try
        {
            var robotsUrl = $"{uri.Scheme}://{uri.Host}/robots.txt";
            var response  = await httpClient.GetAsync(robotsUrl, ct);

            if (!response.IsSuccessStatusCode)
                return true;

            var content = await response.Content.ReadAsStringAsync(ct);
            return ParseRobotsTxt(content, uri.PathAndQuery);
        }
        catch
        {
            return true;
        }
    }

    private static bool ParseRobotsTxt(string content, string path)
    {
        
        var lines = content
            .Split('\n')
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrWhiteSpace(l) && !l.StartsWith('#'))
            .ToList();

        bool? specificResult = null;
        bool? wildcardResult = null;

        bool inOurSection      = false;
        bool inWildcardSection = false;

        foreach (var line in lines)
        {
            if (line.StartsWith("User-agent:", StringComparison.OrdinalIgnoreCase))
            {
                var agent = line["User-agent:".Length..].Trim();
                inOurSection      = agent.Equals(BotName, StringComparison.OrdinalIgnoreCase);
                inWildcardSection = agent == "*";
                continue;
            }

            if (line.StartsWith("Disallow:", StringComparison.OrdinalIgnoreCase))
            {
                var disallowedPath = line["Disallow:".Length..].Trim();

                if (string.IsNullOrEmpty(disallowedPath))
                    continue;

                if (PathMatches(path, disallowedPath))
                {
                    if (inOurSection)      specificResult = false;
                    if (inWildcardSection) wildcardResult = false;
                }
                continue;
            }

            if (line.StartsWith("Allow:", StringComparison.OrdinalIgnoreCase))
            {
                var allowedPath = line["Allow:".Length..].Trim();
                if (PathMatches(path, allowedPath))
                {
                    if (inOurSection)      specificResult = true;
                    if (inWildcardSection) wildcardResult = true;
                }
            }
        }

        return specificResult ?? wildcardResult ?? true;
    }

    private static bool PathMatches(string requestPath, string robotsPath)
    {
        if (robotsPath == "/") return true;

        var pattern = robotsPath
            .Replace("*", ".*")
            .TrimEnd('$');

        return Regex.IsMatch(requestPath, $"^{pattern}",
            RegexOptions.IgnoreCase);
    }
}