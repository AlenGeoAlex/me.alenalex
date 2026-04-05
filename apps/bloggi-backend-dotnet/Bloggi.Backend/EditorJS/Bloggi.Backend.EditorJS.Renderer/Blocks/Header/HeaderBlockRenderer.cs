using Bloggi.Backend.EditorJS.Core;
using Bloggi.Backend.EditorJS.Core.Models;
using Microsoft.Extensions.Logging;

namespace Bloggi.Backend.EditorJS.Renderer.Blocks.Header;

public class HeaderBlockRenderer(ILogger<AbstractBlockRenderer> logger, InlineProcessor inlineProcessor, IHtmlDocumentWriter documentWriter) : AbstractBlockRenderer<HeaderData>(logger, inlineProcessor, documentWriter)
{
    private readonly List<InlineRule> _inlineRules =
    [
        InlineRule.Italic,
        InlineRule.Bold,
    ];
    public override BlockTypes BlockType => BlockTypes.Header;
    public override IReadOnlyList<InlineRule> InlineRules => _inlineRules;
    protected override async Task<string> RenderCoreAsync(EditorBlock<HeaderData> block, RenderOptions options, CancellationToken ct)
    {
        
        var text = await ProcessInlineAsync(block.TypedData.Text);
        switch (block.TypedData.Level)
        {
            case 1:
                return HtmlDocumentWriter.Create("h1", (ele) =>
                {
                    ele.SetAttribute("class", "post-h1 post-heading-1");
                    ele.InnerHtml = text;
                });
            case 2:
                return HtmlDocumentWriter.Create("h2", (ele) =>
                {
                    ele.SetAttribute("class", "post-h2 post-heading-2");
                    ele.InnerHtml = text;
                });
            case 3:
                return HtmlDocumentWriter.Create("h3", element =>
                {
                    element.SetAttribute("class", "post-h3 post-heading-3");
                    element.InnerHtml = text;
                });
            
            case 4:
                return HtmlDocumentWriter.Create("h4", element =>
                {
                    element.SetAttribute("class", "post-h4 post-heading-4");
                    element.InnerHtml = text;
                });
            case 5:
                return HtmlDocumentWriter.Create("h5", element =>
                {
                    element.SetAttribute("class", "post-h5 post-heading-5");
                    element.InnerHtml = text;
                });
            case 6:
                return HtmlDocumentWriter.Create("h6", element =>
                {
                    element.SetAttribute("class", "post-h6 post-heading-6");
                    element.InnerHtml = text;
                });
            default:
                return text;
        }
    }
    
    
}