using Bloggi.Backend.EditorJS.Core;
using Bloggi.Backend.EditorJS.Renderer.Blocks.Paragraph;
using Bloggi.Backend.EditorJS.Renderer.Blocks.Unknown;
using Microsoft.Extensions.DependencyInjection;

namespace Bloggi.Backend.EditorJS.Renderer;

public static class EditorJsRendererBootstrapExtensions
{
    public static IServiceCollection AddEditorJs(this IServiceCollection services)
    {
        services.AddSingleton<InlineProcessor>();
        services.AddSingleton<EditorJsRenderer>();
        
        // Unknown block is the default block type.
        services.AddKeyedSingleton<ITextExtractor, UnknownBlockTextExtractor>(nameof(BlockTypes.Unknown));
        services.AddKeyedSingleton<IBlockRenderer, UnknownBlockRenderer>(nameof(BlockTypes.Unknown));
        
        // Paragraph block
        services.AddKeyedSingleton<ITextExtractor, ParagraphTextExtractor>(nameof(BlockTypes.Paragraph));
        services.AddKeyedSingleton<IBlockRenderer, ParagraphRenderer>(nameof(BlockTypes.Paragraph));
        return services;
    }
}