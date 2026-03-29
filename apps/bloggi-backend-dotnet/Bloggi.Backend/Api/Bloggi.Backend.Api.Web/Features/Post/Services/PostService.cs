using Bloggi.Backend.Api.Web.Database;
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
                new NpgsqlParameter("tagIds", request.TagIds),
                ct);

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
                    new NpgsqlParameter("tagIds",  request.TagIds),
                    ct);
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

    #region Models

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