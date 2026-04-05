using System.Text;
using Bloggi.Backend.EditorJS.Core;
using Bloggi.Backend.EditorJS.Core.Models;
using Microsoft.Extensions.Logging;

namespace Bloggi.Backend.EditorJS.Renderer.Blocks.Link;

public class LinkTextExtractor(ILogger<AbstractBlockTextExtractor> logger) : AbstractBlockTextExtractor<LinkData>(logger)
{
    public override BlockTypes BlockType => BlockTypes.Link;
    public override async Task<string> ExtractAsync(EditorBlock<LinkData> block, CancellationToken cancellationToken = default)
    {
        var linkData = block.TypedData;
        StringBuilder builder = new StringBuilder();
        if(string.IsNullOrWhiteSpace(linkData.Meta?.Title))
            builder.AppendLine(linkData.Link);
        
        if(string.IsNullOrWhiteSpace(linkData.Meta?.Description))
            builder.AppendLine(linkData.Meta?.Title);
        
        return StripHtml(builder.ToString());
    }
}