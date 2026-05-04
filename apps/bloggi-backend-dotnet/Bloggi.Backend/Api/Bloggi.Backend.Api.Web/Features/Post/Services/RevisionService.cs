using System.Text.Json;
using System.Text.Json.Nodes;
using Bloggi.Backend.Api.Database.Posts;
using Bloggi.Backend.Api.Web.Database;
using Bloggi.Backend.Api.Web.Options;
using Bloggi.Backend.EditorJS.Core.Models;
using EntityFrameworkCore.Locking;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ZiggyCreatures.Caching.Fusion;

namespace Bloggi.Backend.Api.Web.Features.Post.Services;

public class RevisionService(
    ILogger<PostService> logger,
    IBloggiDbContext dbContext,
    PostBlockService blockService,
    IOptionsSnapshot<AppOptions> appOptions,
    IFusionCache cache,
    TimeProvider timeProvider
    )
{
    
    private const string DistributedLockKey = "post-revisions-";

    /// <summary>
    /// Asynchronously retrieves the revisions associated with a specified post, ordered by their creation date
    /// in descending order. Each revision includes metadata such as the revision number, creation time, update time,
    /// optional key, and publication details.
    /// </summary>
    /// <param name="request">
    /// An object containing the details of the request, including the ID of the post for which
    /// the revisions are being retrieved.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the operation if required.
    /// </param>
    /// <returns>
    /// An <see cref="ErrorOr{T}"/> object containing a <see cref="GetRevisionsOfPostResponse"/> on success,
    /// or a collection of errors if the operation fails.
    /// </returns>
    public async Task<ErrorOr<GetRevisionsOfPostResponse>> GetRevisionsOfPostAsync(GetRevisionsOfPost request,
        CancellationToken cancellationToken = default)
    {
        var posts = await dbContext
            .Posts
            .Include(x => x.Revisions)
            .AsNoTracking()
            .Where(x => x.Id == request.PostId)
            .FirstOrDefaultAsync(cancellationToken);
        
        if(posts is null)
            return Errors.Post.PostNotFound;

        var revisionsList = posts.Revisions
            .Select(x => new RevisionSummary(x.Id, x.Revision, x.CreatedAt, x.UpdatedAt, x.Key, x.PublishedAt))
            .OrderByDescending(x => x.CreatedAt)
            .ToArray();
        
        return new GetRevisionsOfPostResponse(revisionsList, posts.RenderedKey);
    }

    /// <summary>
    /// Asynchronously retrieves the collection of editor blocks associated with a specified revision of a post.
    /// The blocks represent the content structure stored for the revision and are deserialized into an array of
    /// <see cref="EditorBlock"/> instances.
    /// </summary>
    /// <param name="request">
    /// An object containing the identifiers for the post and the specific revision whose blocks are being retrieved.
    /// The request should include both the <see cref="GetBlocksOfRevisionRequest.PostId"/> and
    /// <see cref="GetBlocksOfRevisionRequest.RevisionId"/> values.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the operation if needed.
    /// </param>
    /// <returns>
    /// An <see cref="ErrorOr{T}"/> object containing an array of <see cref="EditorBlock"/> objects representing the
    /// editor blocks of the specified revision when successful, or an error if the operation fails, such as when
    /// no matching revision is found.
    /// </returns>
    public async Task<ErrorOr<EditorBlock[]>> GetBlocksForRevisionAsync(GetBlocksOfRevisionRequest request,
        CancellationToken cancellationToken = default)
    {
        var postBlock = await dbContext.PostRevisions
            .AsNoTracking()
            .Where(x => x.Id == request.RevisionId && x.PostId == request.PostId)
            .Select(x => x.Blocks)
            .FirstOrDefaultAsync(cancellationToken);

        if (postBlock is null)
            return Errors.Revision.NoRevisionFoundYet;
        
        return postBlock.Deserialize<EditorBlock[]>() ?? [];
    }

    /// <summary>
    /// Asynchronously creates a new revision for a specified post. The revision includes
    /// a set of editor blocks and metadata, and optionally saves the changes instantly.
    /// </summary>
    /// <param name="request">
    /// An object containing the details of the request, including the ID of the post for which
    /// the revision is being created.
    /// </param>
    /// <param name="executeInstantly">
    /// A boolean flag indicating whether the changes should be saved to the database immediately.
    /// Defaults to true.
    /// </param>
    /// <param name="cancellationToken">
    /// A cancellation token that can be used to cancel the operation if required.
    /// </param>
    /// <seealso cref="Errors.Revision.LastRevisionYetNotPublished"/>
    /// <seealso cref="Errors.Revision.ExistingRevisionCreationPending"/>
    /// <seealso cref="Errors.Revision.FailedToCreateRevision"/>
    /// <returns>
    /// An <see cref="ErrorOr{T}"/> object containing a <see cref="CreateRevisionResponse"/> on success,
    /// or a collection of errors if the operation fails.
    /// </returns>
    public async Task<ErrorOr<CreateRevisionResponse>> CreateRevisionAsync(CreateRevisionRequest request,
        bool executeInstantly = true,
        CancellationToken cancellationToken = default)
    {
        if (await IsLastRevisionPendingAsync(request.PostId, cancellationToken))
            return Errors.Revision.LastRevisionYetNotPublished;
        
        await using var handle = await dbContext.Database.TryAcquireDistributedLockAsync($"{DistributedLockKey}{request.PostId}" , ct: cancellationToken);
        if (handle is null)
        {
            logger.LogWarning("Failed to acquire lock for post {PostId}", request.PostId);
            return Errors.Revision.ExistingRevisionCreationPending;
        }
        
        try
        {
            var id = Guid.CreateVersion7();
            var currentMaxRevision = await dbContext.PostRevisions
                .Where(x => x.PostId == request.PostId)
                .MaxAsync(x => (int?) x.Revision, cancellationToken) ?? 0;
            
            logger.LogInformation("Current max revision for post {PostId} is {Revision}", request.PostId, currentMaxRevision);
            
            var editorBlocks = await blockService.GetBlocksAsync(request.PostId, cancellationToken);
            var jsonArray = new JsonArray();
            foreach (var block in editorBlocks)
            {
                jsonArray.Add(block);
            }

            logger.LogInformation("Loaded block array with {Count} elements", jsonArray.Count);

            var doc = JsonDocument.Parse(jsonArray.ToString());
            dbContext.PostRevisions.Add(new PostRevision()
            {
                Id = id,
                PostId = request.PostId,
                Revision = currentMaxRevision + 1,
                Blocks = doc,
                CreatedAt = timeProvider.GetUtcNow(),
                PublishedAt = null,
                UpdatedAt = timeProvider.GetUtcNow(),
            });

            if (executeInstantly)
                await dbContext.SaveChangesAsync(cancellationToken);

            return new CreateRevisionResponse(id);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to create revision for post {PostId}", request.PostId);
            return Errors.Revision.FailedToCreateRevision;
        }
        finally
        {
            logger.LogInformation("Released lock for post {PostId}", request.PostId);
            await handle.ReleaseAsync(cancellationToken);
        }
    }

    public async Task<ErrorOr<RevisionSummary>> GetRevisionAsync(GetRevisionRequest request,
        CancellationToken cancellationToken = default)
    {
        var revision = await dbContext.PostRevisions.FirstOrDefaultAsync(x => x.Id == request.RevisionId && x.PostId == request.PostId, cancellationToken: cancellationToken);
        if(revision is null)
            return Errors.Revision.NoRevisionFoundYet;
        
        return new RevisionSummary(revision.Id, revision.Revision, revision.CreatedAt, revision.UpdatedAt, revision.Key, revision.PublishedAt);   
    }

    private async Task<bool> IsLastRevisionPendingAsync(Guid postId, CancellationToken cancellationToken = default)
    {
        var lastRevision = await dbContext.PostRevisions
            .Where(x => x.PostId == postId)
            .OrderByDescending(x => x.Revision)
            .FirstOrDefaultAsync(cancellationToken);
        
        if(lastRevision is null)
            return false;
        
        return lastRevision.PublishedAt is null || lastRevision.Key is null;
    }

    public async Task<ErrorOr<DeleteRevisionResponse>> DeleteRevisionAsync(DeleteRevisionRequest request,
        CancellationToken cancellationToken = default)
    {
        var revisionResult = await GetRevisionAsync(new GetRevisionRequest(request.RevisionId, request.PostId), cancellationToken);
        if(revisionResult.IsError)
            return revisionResult.Errors;
        
        var revision = revisionResult.Value;
        if(revision.IsPublished)
            return Errors.Revision.CannotDeletePublishedRevision;

        var postRevision = await dbContext.PostRevisions.FindAsync([request.RevisionId], cancellationToken: cancellationToken)!;
        dbContext.PostRevisions.Remove(postRevision!);
        await dbContext.SaveChangesAsync(cancellationToken);
        
        return new DeleteRevisionResponse();
    }
    
    #region Models

    public record CreateRevisionRequest(Guid PostId);
    
    public record CreateRevisionResponse(Guid RevisionId);
    
    public record GetRevisionsOfPost(Guid PostId);
    
    public record GetRevisionsOfPostResponse(
        RevisionSummary[] Revisions,
        string? CurrentPublishedKey = null
        );
    
    public record GetRevisionRequest(Guid RevisionId, Guid PostId);

    public record DeleteRevisionRequest(Guid PostId, Guid RevisionId);
    
    public record DeleteRevisionResponse();

    public record RevisionSummary(
        Guid Id,
        int Revision,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt,
        string? Key,
        DateTimeOffset? PublishedAt = null)
    {
        public bool IsPublished = PublishedAt is not null && Key is not null;
    }

    public record GetBlocksOfRevisionRequest(Guid PostId, Guid RevisionId);
    #endregion
}