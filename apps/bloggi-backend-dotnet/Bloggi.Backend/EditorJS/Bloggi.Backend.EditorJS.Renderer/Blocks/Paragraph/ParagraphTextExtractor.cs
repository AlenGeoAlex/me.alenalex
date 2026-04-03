using Bloggi.Backend.EditorJS.Core;
using Bloggi.Backend.EditorJS.Core.Models;
using Microsoft.Extensions.Logging;

namespace Bloggi.Backend.EditorJS.Renderer.Blocks.Paragraph;

public class ParagraphTextExtractor(ILogger<AbstractBlockTextExtractor> logger) : AbstractBlockTextExtractor<ParagraphData>(logger)
{
    public override BlockTypes BlockType => BlockTypes.Paragraph;
    public override async Task<string> ExtractAsync(EditorBlock<ParagraphData> block, CancellationToken cancellationToken = default)
    {
        var rawText = StripHtml(block.TypedData.Text);
        return rawText;
    }
}