using System.Text.Json;
using Bloggi.Backend.Api.Database.Posts;
using Bloggi.Backend.Api.Web.Database;
using Bloggi.Backend.EditorJS.Core;
using Bloggi.Backend.EditorJS.Core.Models;
using Bloggi.Backend.EditorJS.Core.Utils;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Bloggi.Backend.Api.Web.Features.Post.Services;

public class PostBlockService(
    ILogger<PostBlockService> logger,
    IBloggiDbContext dbContext
    )
{
    /// <summary>
    /// Retrieves a list of blocks associated with a given post from the database.
    /// The blocks are returned in their corresponding order based on their position.
    /// </summary>
    /// <param name="postId">The unique identifier of the post to retrieve blocks for.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <return>A <see cref="Task"/> that represents the asynchronous operation, containing a list of <see cref="EditorBlock"/> objects.</return>
    public async Task<List<EditorBlock>> GetBlocksAsync(Guid postId, CancellationToken ct = default)
    {
        var postBlocks = await dbContext.PostBlocks
            .AsNoTracking()
            .Where(pb => pb.PostId == postId)
            .OrderBy(pb => pb.Position)
            .ToListAsync(ct);

        return postBlocks
            .Select(x => new EditorBlock(x.BlockId, Enum.Parse<BlockTypes>(x.BlockType),
                x.BlockData.RootElement))
            .ToList();
    }

    /// <summary>
    /// Inserts, updates, or deletes blocks for a specified post in the database.
    /// Existing blocks are compared by their hash and position to determine
    /// whether they need to be updated. Missing blocks are deleted, while new ones are inserted.
    /// </summary>
    /// <param name="request">
    /// A request containing the unique identifier of the post, the editor version,
    /// and the list of <see cref="EditorBlock"/> objects to be processed.
    /// </param>
    /// <param name="ct">
    /// A <see cref="CancellationToken"/> to observe while waiting for the task to complete.
    /// </param>
    /// <returns>
    /// An <see cref="ErrorOr{T}"/> object containing a response with a dictionary mapping
    /// block IDs to their corresponding database-generated GUIDs.
    /// </returns>
    public async Task<ErrorOr<UpsertBlockDataResponse>> UpsertBlocksAsync(
        UpsertBlockDataRequest request,
        CancellationToken ct = default
    )
    {   
        var existing = await dbContext.PostBlocks
            .Where(pb => pb.PostId == request.PostId)
            .Select(pb => new { pb.Id, pb.BlockId, pb.BlockHash, pb.Position })
            .ToDictionaryAsync(pb => pb.BlockId, cancellationToken: ct);

        var toInsert = new List<PostBlock>();
        var toUpdate = new List<PostBlock>();
        var incomingIds = new HashSet<string>();
        Dictionary<string, Guid> ids = new();

        for (var i = 0; i < request.Blocks.Count; i++)
        {
            var block = request.Blocks[i];
            var hash = BlockHash.Compute(block.Data);
            incomingIds.Add(block.Id);

            if (!existing.TryGetValue(block.Id, out var existingBlock))
            {
                var newBlockId = Guid.CreateVersion7();
                toInsert.Add(new PostBlock
                {
                    PostId      = request.PostId,
                    Id          = newBlockId,
                    BlockId     = block.Id,
                    BlockType   = block.Type.ToString(),
                    Position    = i,
                    BlockData   = JsonDocument.Parse(block.Data.GetRawText()),
                    BlockHash    = hash,
                    BlockText = string.Empty,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow,
                });
                ids.Add(block.Id, newBlockId);
            }
            else if (existingBlock.BlockHash != hash || existingBlock.Position != i)
            {
                toUpdate.Add(new PostBlock
                {
                    Id          = existingBlock.Id,
                    PostId      = request.PostId,
                    BlockId     = block.Id,
                    BlockType   = block.Type.ToString(),
                    Position    = i,
                    BlockData   = JsonDocument.Parse(block.Data.GetRawText()),
                    BlockHash    = hash,
                    BlockText = string.Empty,
                    UpdatedAt = DateTimeOffset.UtcNow,
                });
                ids.Add(block.Id, existingBlock.Id);
            }
            else
            {
                ids.Add(block.Id, existingBlock.Id);
            }
        }
        
        var toDeleteIds = existing.Keys
            .Where(id => !incomingIds.Contains(id))
            .ToList();

        if (toInsert.Count > 0)
            dbContext.PostBlocks.AddRange(toInsert);

        if (toUpdate.Count > 0)
            dbContext.PostBlocks.UpdateRange(toUpdate);

        if (toDeleteIds.Count > 0)
            await dbContext.PostBlocks
                .Where(pb => pb.PostId == request.PostId && toDeleteIds.Contains(pb.BlockId))
                .ExecuteDeleteAsync(cancellationToken: ct);
        
        await dbContext.PostMetas.Where(x => x.PostId == request.PostId)
            .ExecuteUpdateAsync(x => x.SetProperty(x => x.EditorVersion, request.Version), cancellationToken: ct);

        await dbContext.SaveChangesAsync(cancellationToken: ct);
        return new UpsertBlockDataResponse(ids);
    }

    
    public async Task<ErrorOr<GetBlocksForPostResponse>> GetBlocksForPostAsync(GetBlocksForPostRequest request, CancellationToken ct = default)
    {
        var posts = dbContext.Posts
            .AsNoTracking();

        if (request.IncludeVersion)
        {
            posts = posts.Include(x => x.Metadata);
        }

        posts = posts.Include(x => x.Blocks);
        var post = posts.FirstOrDefault(x => x.Id == request.PostId);
        
        if(post is null)
            return Errors.Post.PostNotFound;
        
        var blocks = post.Blocks
            .OrderBy(x => x.Position)
            .Select(x => new EditorBlock(x.BlockId, Enum.Parse<BlockTypes>(x.BlockType), x.BlockData.RootElement))
            .ToArray();

        var lastUpdateOn = post.Blocks.Max(x => x.UpdatedAt);

        return new GetBlocksForPostResponse(blocks, post.Metadata?.EditorVersion, lastUpdateOn);
    }

    #region Models
    
    public record GetBlocksForPostRequest(
        Guid PostId,
        bool IncludeVersion = true,
        bool IncludeTime = true
    );

    public record GetBlocksForPostResponse(
        EditorBlock[] Blocks,
        string? EditorVersion,
        DateTimeOffset? LastUpdateOn
    );

    public record UpsertBlockDataRequest(
        Guid PostId,
        string Version,
        IReadOnlyList<EditorBlock> Blocks
    );
    
    public record UpsertBlockDataResponse(
        Dictionary<string, Guid> BlockId
    );

    #endregion

}