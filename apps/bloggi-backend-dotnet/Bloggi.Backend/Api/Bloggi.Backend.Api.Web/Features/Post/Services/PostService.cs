using Bloggi.Backend.Api.Web.Database;
using Bloggi.Backend.Api.Web.Extensions;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Bloggi.Backend.Api.Web.Features.Post.Services;

public class PostService(
    ILogger<PostService> logger,
    IBloggiDbContext dbContext,
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
            dbContext.Posts.Add(new Api.Database.Posts.Post()
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
            });
            
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
            post.UserId,
            post.CreatedAt,
            post.UpdatedAt,
            post.PublishedAt,
            post.Status,
            tags
        );
        
        return postSummary;
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
                    INSERT INTO post.post_files (id, post_id, file_name, type, size, hash, created_at, updated_at)
                    VALUES (@id, @postId, @fileName, @type, @size, @hash, @createdAt, @updatedAt)
                    ON CONFLICT (post_id, file_name)
                    DO UPDATE SET hash = @hash
                    RETURNING id, post_id;
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
        ).FirstOrDefaultAsync(ct);
        
        if(response is null)
            return Errors.PostFile.PostAssociationNotFound;
        
        if(!response.PostId.HasValue)
            return Errors.Post.PostNotFound;
        
        return new CreatePostFileResponse(response.Id, response.PostId.Value, response.Id != id);
    }

    #region Models

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

    #endregion
    

}