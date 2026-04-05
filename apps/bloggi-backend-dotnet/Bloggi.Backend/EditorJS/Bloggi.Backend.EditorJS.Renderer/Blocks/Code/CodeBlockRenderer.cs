using Bloggi.Backend.EditorJS.Core;
using Bloggi.Backend.EditorJS.Core.Models;
using Microsoft.Extensions.Logging;

namespace Bloggi.Backend.EditorJS.Renderer.Blocks.Code;

public class CodeBlockRenderer(ILogger<AbstractBlockRenderer> logger, InlineProcessor inlineProcessor, IHtmlDocumentWriter htmlDocumentWriter) : AbstractBlockRenderer<CodeData>(logger, inlineProcessor, htmlDocumentWriter)
{
    public override BlockTypes BlockType => BlockTypes.Code;
    public override IReadOnlyList<InlineRule> InlineRules => Array.Empty<InlineRule>();
    protected override async Task<string> RenderCoreAsync(EditorBlock<CodeData> block, RenderOptions options, CancellationToken ct)
    {
        var code = block.TypedData.Code;
        if(string.IsNullOrWhiteSpace(code))
            return string.Empty;

        

        var preCodeElement = HtmlDocumentWriter.CreateElement("pre", pre =>
        {
            pre.SetAttribute("class", "post-pre");
        });

        var codeElement = HtmlDocumentWriter.CreateElement("code", codeElement =>
        {
            codeElement.SetAttribute("class", "post-code");
            codeElement.InnerHtml = code;
        });

        HtmlDocumentWriter.Append(preCodeElement, codeElement);
        
        var divPostElement = HtmlDocumentWriter.CreateElement("div", div =>
        {
            div.SetAttribute("class", "post-code-wrap");
        });

        var button = HtmlDocumentWriter.CreateElement("button", button =>
        {
            button.SetAttribute("class", "post-code-copy");
            button.SetAttribute("onclick", "copyCode(this)");
            button.SetAttribute("aria-label", "Copy code");
            button.TextContent = "Copy";
        });
        
        HtmlDocumentWriter.Append(divPostElement, button)
            .Append(divPostElement, preCodeElement);
        
        return divPostElement.OuterHtml;
    }
}