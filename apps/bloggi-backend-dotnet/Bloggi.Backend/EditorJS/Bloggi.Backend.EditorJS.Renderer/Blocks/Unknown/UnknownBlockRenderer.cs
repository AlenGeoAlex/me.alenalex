using System.Text.Json;
using Bloggi.Backend.EditorJS.Core;
using Bloggi.Backend.EditorJS.Core.Models;
using Microsoft.Extensions.Logging;

namespace Bloggi.Backend.EditorJS.Renderer.Blocks.Unknown;

public class UnknownBlockRenderer(ILogger<AbstractBlockRenderer> logger, InlineProcessor inlineProcessor) : AbstractBlockRenderer<object>(logger, inlineProcessor)
{
    public override BlockTypes BlockType => BlockTypes.Unknown;
    public override IReadOnlyList<InlineRule> InlineRules => [];
    protected override async Task<string> RenderCoreAsync(EditorBlock<object> block, RenderOptions options, CancellationToken ct)
    {
        Logger.LogWarning("Unknown block type encountered with id: " + block.Id + "");
        if(options.IsPreview)
            return $"""
                   <span style=\"color: red;\">
                   An unknown type of block was encountered. blockId: {block.Id}.
                   The block data: {JsonSerializer.Serialize(block.Data)}
                   </span> 
                   """;
        
        throw new InvalidOperationException("Unknown block type encountered with id: " + block.Id + "");
    }
}