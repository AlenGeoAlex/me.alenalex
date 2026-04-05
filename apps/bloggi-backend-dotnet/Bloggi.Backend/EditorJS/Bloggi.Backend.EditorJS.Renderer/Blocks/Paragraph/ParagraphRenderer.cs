using Bloggi.Backend.EditorJS.Core;
using Bloggi.Backend.EditorJS.Core.Models;
using Microsoft.Extensions.Logging;

namespace Bloggi.Backend.EditorJS.Renderer.Blocks.Paragraph;

public class ParagraphRenderer(ILogger<AbstractBlockRenderer> logger, InlineProcessor inlineProcessor, IHtmlDocumentWriter documentWriter) : AbstractBlockRenderer<ParagraphData>(logger, inlineProcessor, documentWriter)
{
    private readonly List<InlineRule> _inlineRules =
    [
        InlineRule.Bold,
        InlineRule.Italic,
        InlineRule.Underline,
        InlineRule.Strikethrough,
        InlineRule.Anchor,
        InlineRule.Code,
        InlineRule.Mark
    ];
    
    public override BlockTypes BlockType => BlockTypes.Paragraph;

    public override IReadOnlyList<InlineRule> InlineRules => _inlineRules;
    
    protected override async Task<string> RenderCoreAsync(EditorBlock<ParagraphData> block, RenderOptions options, CancellationToken ct)
    {
        var text = await ProcessInlineAsync(block.TypedData.Text);
        return HtmlDocumentWriter.Create("p", ele =>
        {
            ele.InnerHtml = text;
            ele.SetAttribute("class", "post-p");
        }); 
    }
}