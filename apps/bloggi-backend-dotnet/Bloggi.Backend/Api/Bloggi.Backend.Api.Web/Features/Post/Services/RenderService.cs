using System.Text.Json;
using AngleSharp;
using AngleSharp.Dom;
using Bloggi.Backend.Api.Web.Extensions;
using Bloggi.Backend.Api.Web.Infrastructure.Services;
using Bloggi.Backend.EditorJS.Core;
using Bloggi.Backend.EditorJS.Core.Models;
using ErrorOr;
using ZiggyCreatures.Caching.Fusion;

namespace Bloggi.Backend.Api.Web.Features.Post.Services;

public class RenderService(
    ILogger<RenderService> logger,
    PostService postService,
    PostBlockService blockService,
    TemplateService templateService,
    EditorJsRenderer renderer,
    IFusionCache cache
    )
{
    private const string ArticlePostContentId = "post-content";
    private const string BreadCrumbPostTitle = "breadcrumb-post-title";
    private const string TagsContainer = "post-tags-container";
    private const string PostTitle = "post-title";
    private const string PostExcerpt = "post-excerpt";
    private const string AuthorInitials = "author-initials";
    private const string AuthorName = "author-name";
    private const string PublishedAt = "published-at";
    private const string PublishedAtFooter = "published-at-fotter";
    private const string ReadTime = "read-time";
    private const string PostCover = "post-cover";
    
    /// <summary>
    /// Renders content based on the provided render request.
    /// </summary>
    /// <param name="request">The render request containing the post ID and preview flag.</param>
    /// <param name="ct">The cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the rendered content as a string.</returns>
    public async Task<ErrorOr<RenderResponse>> RenderAsync(
        RenderRequest request,
        CancellationToken ct = default
    )
    {
        var cacheKey = $"{PostCacheKeys.RenderCacheKey}:{request.PostId}";
        string? rawTemplate = null;
        
        // If its not a preview, don't even try to get the template from the cache
        if(request.IsPreview)
            rawTemplate = await cache.GetOrDefaultAsync<string>(cacheKey, token: ct);
        
        string html = "";
        List<Error?> errors = [];
        var configuration = Configuration.Default;
        using var context = BrowsingContext.New(configuration);
        var renderOptions = new RenderOptions()
        {
            IsPreview = request.IsPreview
        };
        if (rawTemplate is not null)
        {
            using var document = await context.OpenAsync(req => req.Content(rawTemplate), cancel: ct);
            var blocksResult = await blockService.GetBlocksForPostAsync(new PostBlockService.GetBlocksForPostRequest(request.PostId, false, false), ct);
            if(blocksResult.IsError)
                return blocksResult.Errors;
            
            var blocks = blocksResult.Value;
            errors.AddIfNotNull(await RenderBlockAsync(document, blocks.Blocks, renderOptions, ct));
            html = document.ToHtml();
        }
        else
        {
            var postResult = await postService.GetPostAsync(new PostService.GetPostByIdRequest(request.PostId), ct);
            if(postResult.IsError)
                return postResult.Errors;

            var post = postResult.Value;
            var templateStringResult = await templateService.GetTemplateMainAsync(post.Template, ct);
            if(templateStringResult.IsError)
                return templateStringResult.Errors;
        
            var template = templateStringResult.Value;
            
            using var document = await context.OpenAsync(req => req.Content(template), cancel: ct);
            errors.AddRange(await RenderMetadataAsync(document, post, renderOptions, ct));
            errors.AddIfNotNull(await RenderBreadCrumbsAsync(document, post, renderOptions, ct));
            errors.AddIfNotNull(await RenderFooterPublishedAtAsync(document, post, renderOptions, ct));
            errors.AddIfNotNull(await RenderTagsAsync(document, post, renderOptions, ct));
            errors.AddRange(await RenderPostHeaderAsync(document, post, renderOptions, ct));
            errors.AddRange(await RenderAuthorAsync(document, post, renderOptions, ct));
            await RenderGlobalJsConstants(document, post, renderOptions, ct);
            await cache.SetAsync(cacheKey, document.ToHtml(), new FusionCacheEntryOptions()
            {
              Duration  = TimeSpan.FromMinutes(10),
            }, [
            $"{PostCacheKeys.RenderCacheKey}:{post.Id}"
            ], ct);
                
            errors.AddIfNotNull(await RenderBlockAsync(document, post.Blocks, renderOptions, ct));
            html = document.ToHtml();
        }
        
        return new RenderResponse(html, errors);
    }

    /// <summary>
    /// Renders metadata for the specified document using the provided response and render options.
    /// </summary>
    /// <param name="document">The HTML document for which metadata will be rendered.</param>
    /// <param name="response">The response containing details about the post, such as title, excerpt, tags, and author information.</param>
    /// <param name="options">Options that customize the rendering process.</param>
    /// <param name="ct">The cancellation token to observe while waiting for the operation to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of errors, if any, that occurred during metadata rendering.</returns>
    private async Task<List<Error?>> RenderMetadataAsync(
        IDocument document,
        PostService.GetPostByIdResponse response,
        RenderOptions options,
        CancellationToken ct = default
    )
    {
        List<Error?> errors = [];
        var head = document.Head;
        if (head is null)
        {
            logger.LogWarning("Head element not found");
            errors.Add(Error.Failure($"{nameof(RenderMetadataAsync)}", "Head element not found"));
            return errors;
        }

        var titles = head.GetElementsByClassName("title");
        titles.ToList().ForEach(x => x.Remove());
        var title = document.CreateElement("title");
        title.InnerHtml = $"{response.Title}";
        head.AppendChild(title);

        var elements = head.GetElementsByTagName("meta").ToList();
        logger.LogInformation("Removing meta elements. Found {Count} elements", elements.Count);
        elements.ForEach(x => x.Remove());
        
        CreateMetaElementIfExists(document,"name", "description", response.Excerpt);
        CreateMetaElementIfExists(document, "name","keywords", string.Join(", ", response.Tags?.Select(x => x.DisplayName) ?? []));
        CreateMetaElementIfExists(document, "name","author", response.Author?.DisplayName);
        CreateMetaElementIfExists(document, "name", "robots", response.Meta?.Robot ?? "index, follow");
        CreateMetaElementIfExists(document, "property","og:type", "article");
        if (response.Meta is not null)
        {
            CreateMetaElementIfExists(document, "property","og:title", response.Meta.OpenGraphTitle ?? response.Title);
            CreateMetaElementIfExists(document, "property","og:description", response.Meta.OpenGraphDescription ?? response.Excerpt);
            CreateMetaElementIfExists(document, "property", "og:url", response.Meta.CanonicalUrl);
            CreateMetaElementIfExists(document, "property","og:image", response.Meta.OpenGraphImageUrl);
            CreateMetaElementIfExists(document, "property", "og:site_name", "Alen's Blog");
            
            CreateMetaElementIfExists(document, "name","twitter:card", "summary_large_image");
            CreateMetaElementIfExists(document, "name","twitter:title", response.Meta.OpenGraphTitle ?? response.Title);
            CreateMetaElementIfExists(document, "name","twitter:description", response.Meta?.OpenGraphDescription ?? response.Excerpt);
            CreateMetaElementIfExists(document, "name", "twitter:image", response.Meta.OpenGraphImageUrl);

            if (response.Meta.JsonLd is not null && response.Meta.JsonLd.Value.ValueKind == JsonValueKind.Object)
            {
                var jsonLd = document.CreateElement("script");
                jsonLd.SetAttribute("type", "application/ld+json");
                jsonLd.SetAttribute("id", "json-ld");
                jsonLd.InnerHtml = JsonSerializer.Serialize(response.Meta.JsonLd);
                document.Head?.AppendChild(jsonLd);
            }
                
        }
        return errors;
    }

    private void CreateMetaElementIfExists(IDocument document, string kind, string name, string? content)
    {
        if(string.IsNullOrWhiteSpace(content))
            return;
        
        var element = document.CreateElement("meta");
        element.SetAttribute("name", name);
        element.SetAttribute("content", content);
        document.Head?.AppendChild(element);
    }

    /// <summary>
    /// Renders global JavaScript constants and appends them to the document's head section.
    /// </summary>
    /// <param name="document">The DOM document where the script element is created and appended.</param>
    /// <param name="response">The response containing post details such as ID, slug, and title to generate the JavaScript constants.</param>
    /// <param name="options">The rendering options to customize the render process.</param>
    /// <param name="ct">The cancellation token to observe while performing the operation.</param>
    private async Task RenderGlobalJsConstants(
        IDocument document,
        PostService.GetPostByIdResponse response,
        RenderOptions options,
        CancellationToken ct = default
    )
    {
        var element = document.CreateElement("script");
        var jsScript = $"  const postId    = {JsonSerializer.Serialize(response.Id.ToString())};\n" +
                      $"  const postSlug  = {JsonSerializer.Serialize(response.Slug)};\n" +
                      $"  const postTitle = {JsonSerializer.Serialize(response.Title)};\n";
        element.InnerHtml = jsScript;
        document.Head?.AppendElement(element);
        logger.LogInformation("Global JS constants set");
    }

    /// <summary>
    /// Renders the footer section with the published date of the post.
    /// </summary>
    /// <param name="document">The document representing the post template.</param>
    /// <param name="response">The response containing the post details.</param>
    /// <param name="options">The rendering options, including preview-related settings.</param>
    /// <param name="ct">The cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>An error object if rendering fails; otherwise, null if successful.</returns>
    /// <exception cref="Exception">Thrown when the required footer element is missing and the operation is not in preview mode.</exception>
    private async Task<Error?> RenderFooterPublishedAtAsync(
        IDocument document,
        PostService.GetPostByIdResponse response,
        RenderOptions options,
        CancellationToken ct = default
    )
    {
        var publishedAt = document.GetElementById(PublishedAtFooter);
        if (publishedAt is null)
        {
            logger.LogWarning("Element with id 'published-at-footer' not found");
            if (options.IsPreview)
                return Error.Failure($"{nameof(RenderFooterPublishedAtAsync)}", "Element with id 'published-at-footer' not found");
            else
                throw new Exception("Element with id 'published-at-footer' not found");
        }
        else
        {
            if (response.PublishedAt.HasValue)
            {
                publishedAt.InnerHtml = response.PublishedAt.Value.ToString("MMMM dd, yyyy");
                publishedAt.SetAttribute("datetime", response.PublishedAt.Value.ToString());
                logger.LogInformation("Published at (Footer) set to {PublishedAt}", response.PublishedAt.Value.ToString("MMMM dd, yyyy"));  
            }
            else
            {
                publishedAt.TextContent = "Draft";
                logger.LogInformation("Published at (Footer) set to Draft"); 
            }
        }
        
        return null;
    }

    /// <summary>
    /// Renders the author details, including initials, name, publication date, and read time, into the specified document.
    /// </summary>
    /// <param name="document">The document to be updated with author details.</param>
    /// <param name="response">The response containing post details such as author information and publication metadata.</param>
    /// <param name="options">The rendering options, including flags for preview mode.</param>
    /// <param name="ct">The cancellation token to observe while performing the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a list of errors encountered during rendering, if any.</returns>
    /// <exception cref="Exception">Thrown when required elements for rendering author details are missing and the operation is not in preview mode.</exception>
    private async Task<List<Error?>> RenderAuthorAsync(
        IDocument document,
        PostService.GetPostByIdResponse response,
        RenderOptions options,
        CancellationToken ct = default
    )
    {
        List<Error?> errors = [];
        var authorInitials = document.GetElementById(AuthorInitials);
        if (authorInitials is null)
        {
            logger.LogWarning("Element with id 'author-initials' not found");
            if (options.IsPreview)
                errors.Add(Error.Failure($"{nameof(RenderAuthorAsync)}", "Element with id 'author-initials' not found"));
            else
                throw new Exception("Element with id 'author-initials' not found");
        }
        else
        {
            var c = response.Author?.DisplayName?[0] ?? 'A';
            authorInitials.TextContent = c.ToString();
            logger.LogInformation("Author initials set to {Initials}", c);
        }
        
        var authorName = document.GetElementById(AuthorName);
        if (authorName is null)
        {
            logger.LogWarning("Element with id 'author-name' not found");
            if (options.IsPreview)
                errors.Add(Error.Failure($"{nameof(RenderAuthorAsync)}", "Element with id 'author-name' not found"));
            else
                throw new Exception("Element with id 'author-name' not found");
        }
        else
        {
            authorName.TextContent = response.Author?.DisplayName ?? "Anonymous";
            logger.LogInformation("Author name set to {Name}", response.Author?.DisplayName ?? "Anonymous");  
        }
        
        
        var publishedAt = document.GetElementById(PublishedAt);
        if (publishedAt is null)
        {
            logger.LogWarning("Element with id 'published-at' not found");
            if (options.IsPreview)
                errors.Add(Error.Failure($"{nameof(RenderAuthorAsync)}", "Element with id 'published-at' not found"));
            else
                throw new Exception("Element with id 'published-at' not found");
        }
        else
        {
            if (response.PublishedAt.HasValue)
            {
                publishedAt.TextContent = response.PublishedAt.Value.ToString("MMMM dd, yyyy");
                publishedAt.SetAttribute("datetime", response.PublishedAt.Value.ToString());
                logger.LogInformation("Published at set to {PublishedAt}", response.PublishedAt.Value.ToString("MMMM dd, yyyy"));  
            }
            else
            {
                publishedAt.TextContent = "Draft";
                logger.LogInformation("Published at set to Draft"); 
            }
        }
        
        var readTime = document.GetElementById(ReadTime);
        if (readTime is not null)
        {
            readTime.TextContent = $"{response.ReadTimeInMins} min read";
            logger.LogInformation("Read time set to {ReadTime}", response.ReadTimeInMins.ToString()); 
        }
        
        return errors;
    }

    /// <summary>
    /// Renders the header section of a post document, including the title and excerpt and post cover, based on the provided response and options.
    /// </summary>
    /// <param name="document">The DOM document representing the post to be rendered.</param>
    /// <param name="response">The response containing post data, including the title and excerpt.</param>
    /// <param name="options">The rendering options such as preview settings.</param>
    /// <param name="ct">The cancellation token to observe while executing the operation.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a list of errors encountered during the rendering process.</returns>
    /// <exception cref="Exception">Thrown if required elements are not found in non-preview mode.</exception>
    private async Task<List<Error?>> RenderPostHeaderAsync(
        IDocument document,
        PostService.GetPostByIdResponse response,
        RenderOptions options,
        CancellationToken ct = default
    )
    {
        var postTitle = document.GetElementById(PostTitle);
        List<Error?> errors = [];
        if (postTitle is null)
        {
            logger.LogWarning("Element with id 'post-title' not found");
            if (options.IsPreview)
                errors.Add(Error.Failure($"{nameof(RenderPostHeaderAsync)}", "Element with id 'post-title' not found"));
            else
                throw new Exception("Element with id 'post-title' not found");
        }
        else
        {
            postTitle.TextContent = response.Title;
            logger.LogInformation("Post title set to {Title}", response.Title);   
        }

        if (string.IsNullOrWhiteSpace(response.Excerpt))
        {
            logger.LogWarning("Post excerpt is empty, Removing element with id 'post-excerpt'");
            document.GetElementById(PostExcerpt)?.Remove();
            return errors;
        }
        
        
        var postExcerpt = document.GetElementById(PostExcerpt);
        if (postExcerpt is null)
        {
            logger.LogWarning("Element with id 'post-excerpt' not found");
            if (options.IsPreview)
                errors.Add(Error.Failure($"{nameof(RenderPostHeaderAsync)}", "Element with id 'post-excerpt' not found"));
            else
                throw new Exception("Element with id 'post-excerpt' not found");
        }
        else
        {
            postExcerpt.TextContent = response.Excerpt;
            logger.LogInformation("Post excerpt set to {Excerpt}", response.Excerpt);   
        }
        
        var postCover = document.GetElementById(PostCover);
        if (postCover is not null)
        {
            if (string.IsNullOrWhiteSpace(response.Meta?.OpenGraphImageUrl))
            {
                logger.LogWarning("Post cover image is empty, Removing element with id 'post-cover'");
                postCover.Remove();
            }
            else
            {
                if (postCover.TagName == "div")
                {
                    var element = document.CreateElement("img");
                    element.SetAttribute("src", response.Meta?.OpenGraphImageUrl);
                    postCover.AppendChild(element);
                    logger.LogInformation("Post cover image set to {Image}", response.Meta?.OpenGraphImageUrl);
                }
                else if (postCover.TagName == "img")
                {
                    postCover.SetAttribute("src", response.Meta?.OpenGraphImageUrl);
                    logger.LogInformation("Post cover image set to {Image}", response.Meta?.OpenGraphImageUrl);
                }
                else
                {
                    logger.LogError("Post cover image element is not a div or img, Removing element with id 'post-cover'");
                    if(options.IsPreview)
                        errors.Add(Error.Unexpected($"{nameof(RenderPostHeaderAsync)}", "Post cover image element is not a div or img"));
                    postCover.Remove();
                }
            }
        }
        
        return errors;
    }

    private async Task<Error?> RenderTagsAsync(
        IDocument document,
        PostService.GetPostByIdResponse response,
        RenderOptions options,
        CancellationToken ct = default)
    {
        if(response.Tags is null || response.Tags.Count == 0)
            return null;
        
        var tagsDiv = document.GetElementById(TagsContainer);
        if (tagsDiv is null)
        {
            logger.LogWarning("Element with id 'post-tags-container' not found");
            if(options.IsPreview)
                return Error.Failure($"{nameof(RenderTagsAsync)}", "Element with id 'post-tags-container' not found");
            
            throw new Exception("Element with id 'post-tags-container' not found");
        }
        
        foreach (var tag in response.Tags)
        {
            var span = document.CreateElement("span");
            span.TextContent = tag.DisplayName;
            span.SetAttribute("class", "post-tag");
            span.Id = $"tag-{tag.Id}";
            tagsDiv.AppendChild(span);
            logger.LogInformation("Tag {Tag} added to post {PostId}", tag.DisplayName, response.Id);
        }
        
        return null;
    }

    /// <summary>
    /// Renders the breadcrumb title within the given document based on the post response.
    /// </summary>
    /// <param name="document">The DOM document object where the breadcrumb title will be rendered.</param>
    /// <param name="response">The post response containing the title to be rendered as a breadcrumb.</param>
    /// <param name="options">The render options specifying rendering behavior, such as preview mode.</param>
    /// <param name="ct">The cancellation token to observe while performing the rendering operation.</param>
    /// <returns>An error instance if rendering fails; otherwise, null if the operation succeeds.</returns>
    /// <exception cref="Exception">Thrown if the breadcrumb element is not found and preview mode is disabled.</exception>
    private async Task<Error?> RenderBreadCrumbsAsync(
        IDocument document,
        PostService.GetPostByIdResponse response,
        RenderOptions options,
        CancellationToken ct = default)
    {
        var breadcrumbPostTitle = document.GetElementById(BreadCrumbPostTitle);
        if (breadcrumbPostTitle is null)
        {
            logger.LogWarning("Element with id 'breadcrumb-post-title' not found");
            if(!options.IsPreview)
                throw new Exception("Element with id 'breadcrumb-post-title' not found");
            return Error.Failure($"{nameof(RenderBreadCrumbsAsync)}", "Element with id 'breadcrumb-post-title' not found");
        }

        breadcrumbPostTitle.TextContent = response.Title;
        logger.LogInformation("Breadcrumb post title set to {Title}", response.Title);
        return null;
    }

    /// <summary>
    /// Renders the specified blocks by injecting their rendered content into the appropriate HTML element within the document.
    /// </summary>
    /// <param name="document">The HTML document where the rendered content will be inserted.</param>
    /// <param name="blocks">The list of blocks to be rendered.</param>
    /// <param name="options">Options governing the rendering process, such as preview mode.</param>
    /// <param name="ct">The cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>An <c>Error?</c> indicating any issues encountered during rendering, or <c>null</c> if successful.</returns>
    /// <exception cref="Exception">Thrown when the expected HTML element for content insertion is not found and the operation is not in preview mode.</exception>
    private async Task<Error?> RenderBlockAsync(
        IDocument document,
        IReadOnlyList<EditorBlock> blocks,
        RenderOptions options,
        CancellationToken ct = default)
    {
        try
        {
            var text = await renderer.RenderAllAsync(blocks, options, ct);

            var elementById = document.GetElementById(ArticlePostContentId);
            if (elementById is null)
            {
                logger.LogWarning("Element with id 'post-content' not found");
                if(options.IsPreview)
                    return Error.Failure($"{nameof(RenderBlockAsync)}", "Element with id 'post-content' not found");
                
                throw new Exception("Element with id 'post-content' not found");
            }
                
            
            elementById.InnerHtml = text;
            return null;
        }
        catch (Exception e)
        {
            return Error.Unexpected($"{nameof(RenderBlockAsync)}", e.Message);
        }
    }
}

public record RenderRequest(
    Guid PostId,
    bool IsPreview
    );
    
public record RenderResponse(string Html, List<Error?> Errors);