using System.Text.Json;
using Bloggi.Backend.Api.Database.Posts;
using Bloggi.Backend.Api.Web.Database;
using Bloggi.Backend.Api.Web.Events;
using Bloggi.Backend.Api.Web.Extensions;
using Bloggi.Backend.Api.Web.Features.Post.Events;
using Bloggi.Backend.Api.Web.Options;
using Bloggi.Backend.EditorJS.Core;
using Bloggi.Backend.EditorJS.Core.Models;
using ErrorOr;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using ZiggyCreatures.Caching.Fusion;

namespace Bloggi.Backend.Api.Web.Features.Post.Services;

public class PostService(
    ILogger<PostService> logger,
    IBloggiDbContext dbContext,
    IOptionsSnapshot<AppOptions> appOptions,
    IFusionCache cache,
    TimeProvider timeProvider
    )
{
    /// <summary>
    /// Asynchronously creates a new post based on the provided request data and optionally saves it to the database.
    /// </summary>
    /// <param name="request">
    /// The request containing details of the post to be created, including title, excerpt, author ID, and status.
    /// </param>
    /// <param name="executeInstantly">
    /// A boolean indicating whether the changes should be saved to the database immediately.
    /// Defaults to true.
    /// </param>
    /// <param name="ct">
    /// A cancellation token that can be used to observe and propagate cancellation requests.
    /// </param>
    /// <seealso cref="Error.Unexpected"/>
    /// <returns>
    /// Returns a result containing either the ID of the newly created post wrapped in a <see cref="CreatePostResponse"/>
    /// object, or an error if the operation fails.
    /// </returns>
    public async Task<ErrorOr<CreatePostResponse>> CreatePostAsync(
        CreatePostRequest request,
        bool executeInstantly = true,
        CancellationToken ct = default
    )
    {
        var postId = Guid.CreateVersion7();
        var timeNow = timeProvider.GetUtcNow();
        try
        {
            var post = new Api.Database.Posts.Post()
            {
                Id = postId,
                UserId = request.AuthorId,
                PublishedAt = null,
                CreatedAt = timeNow,
                UpdatedAt = timeNow,
                Title = request.Title,
                Slug = request.Title.Slugify(),
                Excerpt = request.Excerpt,
                Status = Enum.Parse<Api.Database.Posts.Post.PostStatus>(request.Status),
            };
            dbContext.Posts.Add(post);

            var appBaseUrl = new Uri($"{appOptions.Value.BaseUrl}/blog/{post.Slug}");
            var postMeta = new PostMeta()
            {
                Id = Guid.CreateVersion7(),
                PostId = postId,
                OpenGraphTitle = null,
                OpenGraphDescription = null,
                OpenGraphImageUrl = null,
                CanonicalUrl = appBaseUrl.ToString(),
                Robot = "index, follow",
                EditorVersion = string.Empty,
            };
            dbContext.PostMetas.Add(postMeta);
            
            if (!executeInstantly) return new CreatePostResponse(postId);
            
            await dbContext.SaveChangesAsync(ct);
            return new CreatePostResponse(postId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create post");
            return Error.Unexpected($"{nameof(PostService)}.{nameof(CreatePostAsync)}", ex.Message);
        }
    }

    /// <summary>
    /// Asynchronously links specified tags to a post by updating the tags associated with a given post ID in the database.
    /// </summary>
    /// <param name="request">
    /// The request containing the post ID for which the tags need to be linked, and an array of tag IDs to associate with the post.
    /// </param>
    /// <param name="ct">
    /// A cancellation token that can be used to observe and propagate cancellation requests.
    /// </param>
    /// <returns>
    /// Returns a result indicating success or failure of the tag linking operation. On success, returns true. On failure, returns an error object describing the issue.
    /// </returns>
    public async Task<ErrorOr<bool>> LinkPostTagsAsync(
        LinkPostTagRequest request,
        CancellationToken ct = default)
    {
        logger.LogInformation("Linking tags {TagIds} to post {PostId}",
            string.Join(", ", request.TagIds), request.PostId);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(ct);

        try
        {
            var deleteSql = """
                            DELETE FROM post.post_tags
                            WHERE post_id = @postId
                              AND tag_id != ALL(@tagIds);
                            """;

            await dbContext.Database.ExecuteSqlRawAsync(deleteSql,
                new NpgsqlParameter("postId", request.PostId),
                new NpgsqlParameter("tagIds", request.TagIds));

            if (request.TagIds.Length > 0)
            {
                var postIds = Enumerable.Repeat(request.PostId, request.TagIds.Length).ToArray();

                var insertSql = """
                                INSERT INTO post.post_tags (post_id, tag_id)
                                SELECT unnest(@postIds), unnest(@tagIds)
                                ON CONFLICT (post_id, tag_id) DO NOTHING;
                                """;

                await dbContext.Database.ExecuteSqlRawAsync(insertSql,
                    new NpgsqlParameter("postIds", postIds),
                    new NpgsqlParameter("tagIds",  request.TagIds));
            }

            await new ClearCacheEventHandler.Event(
                [
                    $"{PostCacheKeys.PostMasterKey}:{request.PostId}",
                ],
                [request.PostId.ToString(), $"{PostCacheKeys.RenderCacheKey}:{request.PostId}", $"{PostCacheKeys.PostMasterKey}:{request.PostId}"]
            ).PublishAsync(Mode.WaitForNone, cancellation: ct);
            await transaction.CommitAsync(ct);
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(ct);
            logger.LogError(ex, "Failed to link tags to post {PostId}", request.PostId);
            return Error.Failure("PostTags.LinkFailed", "Failed to update post tags");
        }
    }
    
    /// <summary>
    /// Asynchronously retrieves a summary of the specified post, including details such as title, slug, excerpt,
    /// and optionally associated tags.
    /// </summary>
    /// <param name="postId">
    /// The unique identifier of the post to retrieve.
    /// </param>
    /// <param name="includeTags">
    /// A boolean indicating whether to include associated tags in the summary. Defaults to true.
    /// </param>
    /// <param name="ct">
    /// A cancellation token that can be used to observe and propagate cancellation requests.
    /// </param>
    /// <returns>
    /// Returns a result containing either the post summary encapsulated in a <see cref="PostSummary"/> object,
    /// or an error if the post could not be found or if another failure occurs.
    /// </returns>
    public async Task<ErrorOr<PostSummary>> GetPostSummaryAsync(
        Guid postId,
        bool includeTags = true,
        CancellationToken ct = default)
    {
        var cacheKey = $"{PostCacheKeys.PostMasterKey}:{postId}:{PostCacheKeys.PostSummaryCacheKey}:{includeTags}";
        var cachedResponse =  await cache.GetOrDefaultAsync<PostSummary>(cacheKey, token: ct);
        if (cachedResponse is not null)
        {
            logger.LogInformation("Post summary retrieved from cache for {CacheKey}", cacheKey);
            return cachedResponse;
        }
        
        var queryable = dbContext.Posts
            .AsNoTracking();

        if (includeTags)
        {
            logger.LogInformation("Including tags in post summary query");
            queryable.Include(x => x.PostTags)
                .ThenInclude(x => x.Tag);
        }
            
        queryable = queryable.Where(x => x.Id == postId);
        var post = await queryable.FirstOrDefaultAsync(ct);
        
        if(post is null)
            return Errors.Post.PostNotFound;

        // Populate tags first
        PostTagSummary[] tags = new PostTagSummary[post.PostTags?.Count ?? 0];
        if (post.PostTags is not null)
        {
            for (var i = 0; i < post.PostTags.Count; i++)
            {
                var tag = post.PostTags[i];
                tags[i] = new PostTagSummary(tag.TagId, tag.Tag.Slug, tag.Tag.DisplayName);
            }
        }

        var postSummary = new PostSummary(
            post.Id,
            post.Title,
            post.Slug,
            post.Excerpt,
            post.RenderedKey,
            post.UserId,
            post.CreatedAt,
            post.UpdatedAt,
            post.PublishedAt,
            post.Status,
            tags
        );

        await cache.SetAsync(cacheKey, postSummary, options =>
        {
            options.Duration = TimeSpan.FromMinutes(10);
        }, [postId.ToString(), "tags", "post", $"{PostCacheKeys.PostMasterKey}:{post.Id}"], ct);
        
        return postSummary;
    }

    /// <summary>
    /// Asynchronously updates the details of an existing post based on the provided request data
    /// and optionally saves the changes to the database.
    /// </summary>
    /// <param name="request">
    /// The request containing the updated details of the post, including post ID, title, and excerpt.
    /// </param>
    /// <param name="executeInstantly">
    /// A boolean indicating whether the changes should be saved to the database immediately.
    /// Defaults to true.
    /// </param>
    /// <param name="ct">
    /// A cancellation token that can be used to observe and propagate cancellation requests.
    /// </param>
    /// <returns>
    /// Returns a result containing either the ID of the updated post wrapped in an <see cref="UpdatePostResponse"/> object,
    /// or an error if the operation fails (e.g., if the post is not found).
    /// </returns>
    public async Task<ErrorOr<UpdatePostResponse>> UpdatePostAsync(
        UpdatePostRequest request,
        bool executeInstantly = true,
        CancellationToken ct = default
    )
    {
        var post = await dbContext.Posts.Where(x => x.Id == request.PostId).FirstOrDefaultAsync(ct);
        if (post is null)
            return Errors.Post.PostNotFound;
        
        post.Title = request.Title;
        post.Excerpt = request.Excerpt;
        
        if(executeInstantly)
            await dbContext.SaveChangesAsync(ct);

        await new ClearCacheEventHandler.Event(
            [$"{PostCacheKeys.PostMasterKey}:{request.PostId}"],
            [request.PostId.ToString()]
        ).PublishAsync(cancellation: ct, waitMode: Mode.WaitForNone);
        
        return new UpdatePostResponse(request.PostId);
    }

    /// <summary>
    /// Asynchronously retrieves all files associated with a specific post.
    /// </summary>
    /// <param name="postId">
    /// The unique identifier of the post whose files are to be retrieved.
    /// </param>
    /// <param name="ct">
    /// A cancellation token that can be used to observe and propagate cancellation requests.
    /// </param>
    /// <returns>
    /// Returns a result containing a <see cref="GetPostFileResponse"/> object with details of the files associated with the
    /// specified post, or an error if the operation fails.
    /// </returns>
    public async Task<ErrorOr<GetPostFileResponse>> GetFilesOfPostAsync(
        Guid postId,
        CancellationToken ct = default
    )
    {
        var postListAsync = await dbContext.PostFiles
            .Where(x => x.PostId == postId)
            .ToListAsync(cancellationToken: ct);

        return postListAsync.Count == 0 ?
            new GetPostFileResponse(postId, []) : new GetPostFileResponse(postId, postListAsync.Select(x => new PostFile(x.Id, x.Name, x.Size, x.ContentType, x.Hash, x.CreatedAt, x.UpdatedAt)).ToArray());
    }

    /// <summary>
    /// Asynchronously creates a new file for an existing post based on the provided request data.
    /// </summary>
    /// <param name="request">
    /// The request containing details of the file to be created, including the post ID, file name,
    /// type, size, and hash.
    /// </param>
    /// <param name="ct">
    /// A cancellation token that can be used to observe and propagate cancellation requests.
    /// </param>
    /// <seealso cref="Errors.PostFile.PostAssociationNotFound"/>
    /// <seealso cref="Errors.Post.PostNotFound"/>   
    /// <returns>
    /// Returns a result containing either the ID of the newly created file wrapped in a <see cref="CreatePostFileResponse"/>
    /// object, or an error if the operation fails.
    /// </returns>
    public async Task<ErrorOr<CreatePostFileResponse>> CreateFileForPostAsync(CreatePostFileRequest request,
        CancellationToken ct = default)
    {
        var id = Guid.CreateVersion7();
        var query = """
                    INSERT INTO post.post_files (id, post_id, name, content_type, size, hash, created_at, updated_at)
                    VALUES (@id, @postId, @fileName, @type, @size, @hash, @createdAt, @updatedAt)
                    ON CONFLICT (post_id, hash)
                    DO UPDATE SET name = @fileName, updated_at = @updatedAt
                    RETURNING *
                    """;

        var now = timeProvider.GetUtcNow();
        var response = await dbContext.PostFiles.FromSqlRaw(query,
            new NpgsqlParameter("id", id),
            new NpgsqlParameter("postId", request.PostId),
            new NpgsqlParameter("fileName", request.FileName),
            new NpgsqlParameter("type", request.Type),
            new NpgsqlParameter("size", request.Size),
            new NpgsqlParameter("hash", request.Hash),
            new NpgsqlParameter("createdAt", now),
            new NpgsqlParameter("updatedAt", now)
        ).AsAsyncEnumerable().FirstOrDefaultAsync(ct);
        
        if(response is null)
            return Errors.PostFile.PostAssociationNotFound;
        
        if(!response.PostId.HasValue)
            return Errors.Post.PostNotFound;
        
        return new CreatePostFileResponse(response.Id, response.PostId.Value, response.Id != id);
    }

    public async Task<ErrorOr<GetPostByIdResponse>> GetPostAsync(GetPostByIdRequest request, CancellationToken ct = default)
    {
        var cacheKey = $"{PostCacheKeys.PostMasterKey}:{request.PostId}:{PostCacheKeys.PostByIdCacheKey}:{request.IncludeTags}:{request.IncludeAuthor}:{request.IncludeMeta}:{request.IncludeHead}:{request.IncludeBlocks}";
        var cachedResponse = await cache.GetOrDefaultAsync<GetPostByIdResponse>(cacheKey, token: ct);
        if (cachedResponse is not null)
        {
            logger.LogInformation("Post retrieved from cache for {CacheKey}", cacheKey);
            return cachedResponse;       
        }
        
        var posts = dbContext.Posts.AsNoTracking();
        if (request.IncludeTags)
        {
            posts = posts.Include(x => x.PostTags)
                .ThenInclude(x => x.Tag);
        }
        

        if (request.IncludeAuthor)
            posts = posts.Include(x => x.User);
        
        if(request.IncludeMeta)
            posts = posts.Include(x => x.Metadata);
        
        if(request.IncludeHead)
            posts = posts.Include(x => x.Heads);
        
        if(request.IncludeBlocks)
            posts = posts.Include(x => x.Blocks);       
        
        var post = await posts.FirstOrDefaultAsync(x => x.Id == request.PostId, ct);
        if (post is null)
            return Errors.Post.PostNotFound;

        var response = new GetPostByIdResponse
        {
            Id = post.Id,
            UserId = post.UserId,
            Slug = post.Slug,
            Title = post.Title,
            Excerpt = post.Excerpt,
            Status = post.Status.ToString(),
            RenderedKey = post.RenderedKey,
            Template = post.Template,
            PublishedAt = post.PublishedAt,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt,
            Author = request.IncludeAuthor ? new GetPostByIdResponse.GetPostAuthorDto
            {
                Id = post.User.Id,
                DisplayName = post.User.DisplayName
            } : null,
            Meta = request.IncludeMeta ? new GetPostByIdResponse.GetPostMetaDto
            {
                OpenGraphTitle = post.Metadata.OpenGraphTitle,
                OpenGraphDescription = post.Metadata.OpenGraphDescription,
                OpenGraphImageUrl = post.Metadata.OpenGraphImageUrl,
                CanonicalUrl = post.Metadata.CanonicalUrl,
                Robot = post.Metadata.Robot,
                EditorVersion = post.Metadata.EditorVersion,
                JsonLd = JsonElement.Parse(post.Metadata.SchemaOrgJson?.RootElement.GetRawText() ?? "{}")
            } : null,
            Heads = request.IncludeHead
                ? post.Heads.Select(x => new GetPostByIdResponse.GetPostHeadDto
                {
                    Kind = x.Kind.ToString(),
                    Content = x.Content,
                    Position = x.Position
                }).ToList()
                : null,
            Tags = request.IncludeTags
                ? post.PostTags.Select(x => new GetPostByIdResponse.GetPostTagDto
                {
                    Id = x.Tag.Id,
                    Slug = x.Tag.Slug,
                    DisplayName = x.Tag.DisplayName
                }).ToList()
                : null,
            Blocks = request.IncludeBlocks
                ? post.Blocks.OrderBy(x => x.Position).Select(x => new EditorBlock(x.BlockId, Enum.TryParse<BlockTypes>(x.BlockType, out var tpe) ? tpe : BlockTypes.Unknown, x.BlockData.RootElement)).ToList()
                : []
        };

        List<string> cacheTags = [
            $"{PostCacheKeys.PostMasterKey}:{post.Id}"
        ];
        if (request.IncludeAuthor)
        {
            cacheTags.Add(PostCacheKeys.PostCacheTags.User);
            cacheTags.Add(post.User.Id.ToString());
        }

        if (request.IncludeMeta)
            cacheTags.Add(PostCacheKeys.PostCacheTags.Meta);
        
        if (request.IncludeHead)
            cacheTags.Add(PostCacheKeys.PostCacheTags.Head);
        
        if (request.IncludeTags)
            cacheTags.Add(PostCacheKeys.PostCacheTags.Tag);
        
        if (request.IncludeBlocks)
            cacheTags.Add(PostCacheKeys.PostCacheTags.Block);       
        
        await cache.SetAsync(cacheKey, response, options =>
        {
            options.Duration = TimeSpan.FromMinutes(20);
        }, cacheTags, ct);
        
        return response;
    }

    /// <summary>
    /// Asynchronously updates metadata for a specific post in the database based on the provided request,
    /// and optionally saves the changes immediately.
    /// </summary>
    /// <param name="request">
    /// The request containing the metadata details to be updated, including OpenGraph information,
    /// canonical URL, Schema.org data, and robot directives.
    /// </param>
    /// <param name="executeInstantly">
    /// A boolean indicating whether the updates should be saved to the database immediately.
    /// Defaults to true.
    /// </param>
    /// <param name="ct">
    /// A cancellation token that can be used to observe and propagate cancellation requests.
    /// </param>
    /// <returns>
    /// Returns a result containing either a boolean indicating the success of the operation or an error if the update fails.
    /// </returns>
    public async Task<ErrorOr<bool>> UpdatePostMetaAsync(UpdatePostMeta request,
        bool executeInstantly = true,
        CancellationToken ct = default)
    {
        var postId = request.PostId;

        try
        {
            await dbContext.PostMetas
                .Where(p => p.PostId == postId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Robot, request.Robot)
                    .SetProperty(p => p.SchemaOrgJson, JsonDocument.Parse(
                        request.SchemaOrgJson.HasValue
                            ? request.SchemaOrgJson.Value.GetRawText()
                            : "{}"))
                    .SetProperty(p => p.OpenGraphDescription, request.OpenGraphDescription)
                    .SetProperty(p => p.OpenGraphImageUrl, request.OpenGraphImageUrl)
                    .SetProperty(p => p.OpenGraphTitle, request.OpenGraphTitle)
                    .SetProperty(p => p.CanonicalUrl, request.CanonicalUrl), cancellationToken: ct);

            if (executeInstantly)
                await dbContext.SaveChangesAsync(ct);

            await new ClearCacheEventHandler.Event([], [$"{PostCacheKeys.PostMasterKey}:{request.PostId}"])
                .PublishAsync(cancellation: ct, waitMode: Mode.WaitForNone);
            
            return true;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to update post meta for post {PostId}", postId);
            return Errors.PostMeta.FailedToUpdatePostMeta;
        }
    }

    #region Models

    public record UpdatePostMeta(
        Guid PostId,
        string? OpenGraphTitle,
        string? OpenGraphDescription,
        string? OpenGraphImageUrl,
        string? CanonicalUrl,
        string Robot,
        JsonElement? SchemaOrgJson
    );

    public class GetPostByIdResponse
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public string Slug { get; init; } = null!;
        public string Title { get; init; } = null!;
        public string Excerpt { get; init; } = "";
        public string Status { get; init; } = null!;
        public string? RenderedKey { get; init; }
        public string Template { get; init; } = null!;
        public DateTimeOffset? PublishedAt { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset UpdatedAt { get; init; }

        public GetPostAuthorDto? Author { get; init; }
        public GetPostMetaDto? Meta { get; init; }
        public List<GetPostHeadDto>? Heads { get; init; }
        public List<GetPostTagDto>? Tags { get; init; }
        
        public List<EditorBlock> Blocks { get; init; } = new();
        public string ReadTimeInMins { get; set; } = "1";

        public sealed class GetPostAuthorDto
        {
            public Guid Id { get; init; }
            public string? DisplayName { get; init; }
        }

        public sealed class GetPostMetaDto
        {
            public string? OpenGraphTitle { get; init; }
            public string? OpenGraphDescription { get; init; }
            public string? OpenGraphImageUrl { get; init; }
            public string? CanonicalUrl { get; init; }
            public string Robot { get; init; } = null!;
            public string EditorVersion { get; init; } = null!;
            
            public JsonElement? JsonLd { get; init; }
        }

        public sealed class GetPostHeadDto
        {
            public string Kind { get; init; } = null!;
            public string Content { get; init; } = null!;
            public int Position { get; init; }
        }

        public sealed class GetPostTagDto
        {
            public Guid Id { get; init; }
            public string Slug { get; init; } = null!;
            public string DisplayName { get; init; } = null!;
        }
    }
    
    public record GetPostByIdRequest(
        Guid PostId,
        bool IncludeTags = true,
        bool IncludeAuthor = true,
        bool IncludeMeta = true,
        bool IncludeHead = true,
        bool IncludeBlocks = true
        );

    public record CreatePostFileRequest(
        Guid PostId,
        string FileName,
        string Type,
        long Size,
        string Hash
    );

    public record CreatePostFileResponse(
        Guid FileId,
        Guid PostId,
        bool Exists
    );

    public record PostSummary(
        Guid Id,
        string Title,
        string Slug,
        string? Excerpt,
        string? RenderedKey,
        Guid AuthorId,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt,
        DateTimeOffset? PublishedAt,
        Api.Database.Posts.Post.PostStatus Status,
        PostTagSummary[] Tags
    );

    public record PostTagSummary(
        Guid TagId,
        string Slug,
        string DisplayName
    );
    
    public record PostTags(
        Guid PostId,
        Guid[] TagIds
    );

    /// <summary>
    /// Represents the request model for linking tags to a specific post.
    /// </summary>
    public record LinkPostTagRequest(Guid PostId, Guid[] TagIds);

    /// <summary>
    /// Represents the request model for creating a new post.
    /// </summary>
    public record CreatePostRequest(
        string Title,
        string? Excerpt,
        Guid AuthorId,
        string Status = nameof(Api.Database.Posts.Post.PostStatus.Draft)
    );

    /// <summary>
    /// Represents the response returned after attempting to create a new post.
    /// </summary>
    public record CreatePostResponse(
        Guid PostId
    );

    public record UpdatePostRequest(
        Guid PostId,
        string Title,
        string Excerpt
    );

    public record UpdatePostResponse(
        Guid PostId
    );

    /// <summary>
    /// Represents the response model for retrieving files associated with a specific post.
    /// </summary>
    /// <param name="PostId">The unique identifier of the post.</param>
    /// <param name="Files">A collection of files related to the specified post.</param>
    public record GetPostFileResponse(
        Guid PostId,
        PostFile[] Files
    );

    /// <summary>
    /// Represents a file associated with a specific post, including its metadata and creation details.
    /// </summary>
    public record PostFile(
        Guid FileId,
        string Name,
        long Size,
        string Type,
        string Hash,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt
    );

    #endregion
}