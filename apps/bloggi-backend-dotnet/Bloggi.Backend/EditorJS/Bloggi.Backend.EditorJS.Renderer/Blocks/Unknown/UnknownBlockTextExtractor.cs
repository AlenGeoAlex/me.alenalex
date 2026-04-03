using Bloggi.Backend.EditorJS.Core;
using Bloggi.Backend.EditorJS.Core.Models;
using Microsoft.Extensions.Logging;

namespace Bloggi.Backend.EditorJS.Renderer.Blocks.Unknown;

public class UnknownBlockTextExtractor(ILogger<AbstractBlockTextExtractor> logger) : AbstractBlockTextExtractor<object>(logger)
{
    public override BlockTypes BlockType => BlockTypes.Unknown;
    public override async Task<string> ExtractAsync(EditorBlock<object> block, CancellationToken cancellationToken = default)
    {
        Logger.LogWarning("Unknown block type encountered with id: " + block.Id + "");
        return "";
    }
}