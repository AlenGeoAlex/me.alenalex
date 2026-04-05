using Bloggi.Backend.EditorJS.Core;
using Bloggi.Backend.EditorJS.Core.Models;
using Microsoft.Extensions.Logging;

namespace Bloggi.Backend.EditorJS.Renderer.Blocks.Image;

public class ImageTextExtractor(ILogger<AbstractBlockTextExtractor> logger) : AbstractBlockTextExtractor<ImageData>(logger)
{
    public override BlockTypes BlockType => BlockTypes.Image;
    public override async Task<string> ExtractAsync(EditorBlock<ImageData> block, CancellationToken cancellationToken = default)
    {
        var cap = block.TypedData.Caption;
        if(string.IsNullOrWhiteSpace(cap))
            return string.Empty;

        cap = StripHtml(cap);
        return cap;
    }
}