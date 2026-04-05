using Bloggi.Backend.EditorJS.Core;
using Bloggi.Backend.EditorJS.Core.Models;
using Microsoft.Extensions.Logging;

namespace Bloggi.Backend.EditorJS.Renderer.Blocks.Warning;

public class WarningTextExtractor(ILogger<AbstractBlockTextExtractor> logger) : AbstractBlockTextExtractor<WarningData>(logger)
{
    public override BlockTypes BlockType => BlockTypes.Warning;
    public override Task<string> ExtractAsync(EditorBlock<WarningData> block, CancellationToken cancellationToken = default)
    {
        var message = block.TypedData.Message;
        message = StripHtml(message);
        return Task.FromResult(message);
    }
}