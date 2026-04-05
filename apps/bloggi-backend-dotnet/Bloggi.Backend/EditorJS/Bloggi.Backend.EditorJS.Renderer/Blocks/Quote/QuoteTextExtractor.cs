using System.Text;
using Bloggi.Backend.EditorJS.Core;
using Bloggi.Backend.EditorJS.Core.Models;
using Microsoft.Extensions.Logging;

namespace Bloggi.Backend.EditorJS.Renderer.Blocks.Quote;

public class QuoteTextExtractor(ILogger<AbstractBlockTextExtractor> logger) : AbstractBlockTextExtractor<QuoteData>(logger)
{
    public override BlockTypes BlockType => BlockTypes.Quote;
    public override async Task<string> ExtractAsync(EditorBlock<QuoteData> block, CancellationToken cancellationToken = default)
    {
        var typedData = block.TypedData;
        StringBuilder builder = new StringBuilder();
        
        return StripHtml(builder.ToString());
    }
}