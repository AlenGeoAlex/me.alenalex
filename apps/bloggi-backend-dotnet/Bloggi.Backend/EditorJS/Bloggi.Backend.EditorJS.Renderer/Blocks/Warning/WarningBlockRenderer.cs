using Bloggi.Backend.EditorJS.Core;
using Bloggi.Backend.EditorJS.Core.Models;
using Microsoft.Extensions.Logging;

namespace Bloggi.Backend.EditorJS.Renderer.Blocks.Warning;

public class WarningBlockRenderer(ILogger<AbstractBlockRenderer> logger, InlineProcessor inlineProcessor, IHtmlDocumentWriter htmlDocumentWriter) : AbstractBlockRenderer<WarningData>(logger, inlineProcessor, htmlDocumentWriter)
{
    private readonly List<InlineRule> _rules =
    [
        InlineRule.Bold,
        InlineRule.Italic,
        InlineRule.Underline,
        InlineRule.Strikethrough,
        InlineRule.Anchor,
        InlineRule.Code,
        InlineRule.Mark
    ];
    
    public override BlockTypes BlockType => BlockTypes.Warning;
    public override IReadOnlyList<InlineRule> InlineRules => _rules;
    protected override async Task<string> RenderCoreAsync(EditorBlock<WarningData> block, RenderOptions options, CancellationToken ct)
    {
        var message = block.TypedData.Message;
        message = await InlineProcessor.ProcessAsync(message, InlineRules);
        var title = block.TypedData.Title;
        title = await InlineProcessor.ProcessAsync(title, []);
        
        var outerDiv = HtmlDocumentWriter.CreateElement("div", div =>
        {
            div.SetAttribute("class", "post-callout post-callout-warning");
        });

        var iconSpan = HtmlDocumentWriter.CreateElement("span", span =>
        {
            span.SetAttribute("class", "post-callout-icon");
            var icon = HtmlDocumentWriter.CreateElement("i", i =>
            {
                i.SetAttribute("class", "pi pi-exclamation-circle");
            });
            span.AppendChild(icon);
        });
        
        HtmlDocumentWriter.Append(outerDiv, iconSpan);
        
        var titleDiv = HtmlDocumentWriter.CreateElement("div", titleDiv =>
        {
            titleDiv.SetAttribute("class", "post-callout-title");
            titleDiv.InnerHtml = title;
        });
        
        var bodyDiv = HtmlDocumentWriter.CreateElement("div", bodyDiv =>
        {
            bodyDiv.SetAttribute("class", "post-callout-body");
            bodyDiv.AppendChild(titleDiv);
            bodyDiv.AppendChild(HtmlDocumentWriter.CreateElement("p", p =>
            {
                p.InnerHtml = message;
            }));
        });
        
        HtmlDocumentWriter.Append(outerDiv, bodyDiv);

        return outerDiv.OuterHtml;
    }
    
}