using AngleSharp;
using Bloggi.Backend.EditorJS.Core;
using Bloggi.Backend.EditorJS.Core.Models;
using Microsoft.Extensions.Logging;

namespace Bloggi.Backend.EditorJS.Renderer.Blocks.Quote;

public class QuoteBlockRenderer(ILogger<AbstractBlockRenderer> logger, InlineProcessor inlineProcessor, IHtmlDocumentWriter htmlDocumentWriter) : AbstractBlockRenderer<QuoteData>(logger, inlineProcessor, htmlDocumentWriter)
{
    public override BlockTypes BlockType => BlockTypes.Quote;

    public override IReadOnlyList<InlineRule> InlineRules => new List<InlineRule>([
        InlineRule.Italic,
        InlineRule.Bold,
        InlineRule.Anchor,
    ]);
    
    protected override async Task<string> RenderCoreAsync(EditorBlock<QuoteData> block, RenderOptions options, CancellationToken ct)
    {
        var data = block.TypedData;

        var blockQuoteElement = HtmlDocumentWriter.CreateElement("blockquote", bq =>
        {
            bq.SetAttribute("class", "post-quote");
        });
        
        var text = await ProcessInlineAsync(data.Text);
        var pElement = HtmlDocumentWriter.CreateElement("p", p =>
        {
            p.InnerHtml = text;
        });
        HtmlDocumentWriter.Append(blockQuoteElement, pElement);

        if (data.Caption != null)
        {
            var caption = await ProcessInlineAsync(data.Caption);
            var captionElement = HtmlDocumentWriter.CreateElement("cite", cite =>
            {
                cite.InnerHtml = caption;
            });
            HtmlDocumentWriter.Append(blockQuoteElement, captionElement);
        }
        
        return blockQuoteElement.OuterHtml;
    }
}