using System.Text.Json;
using Bloggi.Backend.Api.Database.Posts;
using Bloggi.Backend.Api.Web.Database;
using Bloggi.Backend.EditorJS.Core;
using Bloggi.Backend.EditorJS.Core.Models;
using Bloggi.Backend.EditorJS.Core.Utils;
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
    /// Inserts, updates, or deletes post blocks in the database for a specific post.
    /// Compares the provided blocks with existing ones, ensuring the database state
    /// is consistent with the incoming data.
    /// </summary>
    /// <param name="postId">The unique identifier of the post to update blocks for.</param>
    /// 
    /// <param name="blocks">A read-only list of <see cref="EditorBlock"/> objects
    /// representing the blocks to be upserted.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <return>A <see cref="Task"/> that represents the asynchronous operation.</return>
    public async Task<Dictionary<string, Guid>> UpsertBlocksAsync(
        Guid postId,
        IReadOnlyList<EditorBlock> blocks,
        CancellationToken ct = default
    )
    {   
        var existing = await dbContext.PostBlocks
            .Where(pb => pb.PostId == postId)
            .Select(pb => new { pb.Id, pb.BlockId, pb.BlockHash, pb.Position })
            .ToDictionaryAsync(pb => pb.BlockId, cancellationToken: ct);

        var toInsert = new List<PostBlock>();
        var toUpdate = new List<PostBlock>();
        var incomingIds = new HashSet<string>();
        Dictionary<string, Guid> ids = new();

        for (var i = 0; i < blocks.Count; i++)
        {
            var block = blocks[i];
            var hash = BlockHash.Compute(block.Data);
            incomingIds.Add(block.Id);

            if (!existing.TryGetValue(block.Id, out var existingBlock))
            {
                var newBlockId = Guid.CreateVersion7();
                toInsert.Add(new PostBlock
                {
                    PostId      = postId,
                    Id          = newBlockId,
                    BlockId     = block.Id,
                    BlockType   = block.Type.ToString(),
                    Position    = i,
                    BlockData   = JsonDocument.Parse(block.Data.GetRawText()),
                    BlockHash    = hash,
                    BlockText = string.Empty,
                });
                ids.Add(block.Id, newBlockId);
            }
            else if (existingBlock.BlockHash != hash || existingBlock.Position != i)
            {
                toUpdate.Add(new PostBlock
                {
                    PostId      = postId,
                    BlockId     = block.Id,
                    BlockType   = block.Type.ToString(),
                    Position    = i,
                    BlockData   = JsonDocument.Parse(block.Data.GetRawText()),
                    BlockHash    = hash,
                    BlockText = string.Empty,
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
                .Where(pb => pb.PostId == postId && toDeleteIds.Contains(pb.BlockId))
                .ExecuteDeleteAsync(cancellationToken: ct);

        await dbContext.SaveChangesAsync(cancellationToken: ct);
        return ids;
    }
    
}