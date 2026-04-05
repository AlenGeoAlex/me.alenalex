using Bloggi.Backend.EditorJS.Core;
using Bloggi.Backend.EditorJS.Core.Models;
using Microsoft.Extensions.Logging;

namespace Bloggi.Backend.EditorJS.Renderer.Blocks.Header;

public class HeaderTextExtractor(ILogger<AbstractBlockTextExtractor> logger) : AbstractBlockTextExtractor<HeaderData>(logger)
{
    public override BlockTypes BlockType => BlockTypes.Header;
    public override async Task<string> ExtractAsync(EditorBlock<HeaderData> block, CancellationToken cancellationToken = default)
    {
        return StripHtml(block.TypedData.Text);
    }
}