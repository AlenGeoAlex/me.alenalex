using Bloggi.Backend.EditorJS.Core;
using Bloggi.Backend.EditorJS.Core.Models;
using Microsoft.Extensions.Logging;

namespace Bloggi.Backend.EditorJS.Renderer.Blocks.Button;

public class ButtonBlockRenderer(ILogger<AbstractBlockRenderer> logger, InlineProcessor inlineProcessor, IHtmlDocumentWriter htmlDocumentWriter) : AbstractBlockRenderer<ButtonData>(logger, inlineProcessor, htmlDocumentWriter)
{
    public override BlockTypes BlockType => BlockTypes.Button;
    public override IReadOnlyList<InlineRule> InlineRules => Array.Empty<InlineRule>();
    protected override Task<string> RenderCoreAsync(EditorBlock<ButtonData> block, RenderOptions options, CancellationToken ct)
    {
        var link = block.TypedData.Link;
        var text = block.TypedData.Text;

        if (string.IsNullOrWhiteSpace(link) || string.IsNullOrWhiteSpace(text))
            return Task.FromResult(string.Empty);

        var wrap = HtmlDocumentWriter.CreateElement("div", d =>
        {
            d.SetAttribute("class", "post-button-wrap");
        });

        var anchor = HtmlDocumentWriter.CreateElement("a", a =>
        {
            a.SetAttribute("class", "post-button");
            a.SetAttribute("href", EncodePlain(link));
            a.SetAttribute("target", "_blank");
            a.SetAttribute("rel", "noopener noreferrer");
        });

        var textSpan = HtmlDocumentWriter.CreateElement("span", s =>
        {
            s.InnerHtml = EncodePlain(text);
        });

        var icon = HtmlDocumentWriter.CreateElement("i", i =>
        {
            i.SetAttribute("class", "pi pi-arrow-right");
        });

        HtmlDocumentWriter.Append(anchor, textSpan);
        HtmlDocumentWriter.Append(anchor, icon);
        HtmlDocumentWriter.Append(wrap, anchor);

        return Task.FromResult(wrap.OuterHtml);
    }
}