using Bloggi.Backend.EditorJS.Core;
using Bloggi.Backend.EditorJS.Core.Models;
using Microsoft.Extensions.Logging;

namespace Bloggi.Backend.EditorJS.Renderer.Blocks.Math;

public class MathBlockRenderer(ILogger<AbstractBlockRenderer> logger, InlineProcessor inlineProcessor, IHtmlDocumentWriter htmlDocumentWriter) : AbstractBlockRenderer<MathData>(logger, inlineProcessor, htmlDocumentWriter)
{
    public override BlockTypes BlockType => BlockTypes.Math;
    public override IReadOnlyList<InlineRule> InlineRules => Array.Empty<InlineRule>();
    protected override async Task<string> RenderCoreAsync(EditorBlock<MathData> block, RenderOptions options, CancellationToken ct)
    {
        var mathData = block.TypedData;
        var mathDataString = $"$${mathData.Latex}$$";

        var pEle = HtmlDocumentWriter.CreateElement("p", element =>
        {
            element.InnerHtml = mathDataString;
        });
        
        return pEle.OuterHtml;  
    }
}