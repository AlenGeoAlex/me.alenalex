using Bloggi.Backend.EditorJS.Core;
using Bloggi.Backend.EditorJS.Core.Models;
using Microsoft.Extensions.Logging;

namespace Bloggi.Backend.EditorJS.Renderer.Blocks.Paragraph;

public class ParagraphRenderer(ILogger<AbstractBlockRenderer> logger, InlineProcessor inlineProcessor) : AbstractBlockRenderer<ParagraphData>(logger, inlineProcessor)
{
    private readonly List<InlineRule> _inlineRules = new List<InlineRule>(){
        new InlineRule("b", null, "strong", "post-bold", Array.Empty<string>()),
        new InlineRule("i", null, "em", "post-italic", Array.Empty<string>()),
    };
    
    public override BlockTypes BlockType => BlockTypes.Paragraph;

    public override IReadOnlyList<InlineRule> InlineRules => _inlineRules;
    
    protected override async Task<string> RenderCoreAsync(EditorBlock<ParagraphData> block, RenderOptions options, CancellationToken ct)
    {
        var text = await ProcessInlineAsync(block.TypedData.Text);
        return $"<p class=\"post-p\">{text}</p>";
    }
}