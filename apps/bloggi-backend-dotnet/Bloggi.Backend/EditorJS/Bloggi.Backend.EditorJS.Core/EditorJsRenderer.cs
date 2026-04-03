using System.Text;
using Bloggi.Backend.EditorJS.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bloggi.Backend.EditorJS.Core;

public class EditorJsRenderer(
    ILogger<EditorJsRenderer> logger,
    IServiceProvider serviceProvider
    )
{

    public async Task<string> RenderAllAsync(IEnumerable<EditorBlock> blocks,
        RenderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        options ??= new RenderOptions()
        {
            IsPreview = false
        };
        var blockProviderCache = new Dictionary<BlockTypes, IBlockRenderer>();
        
        StringBuilder result = new StringBuilder();
        
        foreach (var editorBlock in blocks)
        {
            if (!blockProviderCache.TryGetValue(editorBlock.Type, out var renderer))
            {
                renderer = serviceProvider.GetRequiredKeyedService<IBlockRenderer>(editorBlock.Type.ToString());
                blockProviderCache[editorBlock.Type] = renderer;
            }
            
            var renderText = await RenderBlockAsync(editorBlock, renderer, options, cancellationToken);
            result.AppendLine(renderText);
            result.AppendLine();
        }
        
        return result.ToString();
    }
    
    private async Task<string> RenderBlockAsync(
        EditorBlock block,
        IBlockRenderer renderer,
        RenderOptions options,
        CancellationToken cancellationToken = default)
    {
        var renderText = await renderer.RenderAsync(block, options, cancellationToken);
        return renderText;
    }
    
    
    
}