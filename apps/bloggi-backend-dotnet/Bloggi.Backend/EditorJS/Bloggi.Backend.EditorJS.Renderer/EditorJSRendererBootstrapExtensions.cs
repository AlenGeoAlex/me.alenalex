using AngleSharp;
using AngleSharp.Dom;
using Bloggi.Backend.EditorJS.Core;
using Bloggi.Backend.EditorJS.Renderer.Blocks.Code;
using Bloggi.Backend.EditorJS.Renderer.Blocks.Header;
using Bloggi.Backend.EditorJS.Renderer.Blocks.Image;
using Bloggi.Backend.EditorJS.Renderer.Blocks.Link;
using Bloggi.Backend.EditorJS.Renderer.Blocks.Paragraph;
using Bloggi.Backend.EditorJS.Renderer.Blocks.Quote;
using Bloggi.Backend.EditorJS.Renderer.Blocks.Unknown;
using Bloggi.Backend.EditorJS.Renderer.Blocks.Warning;
using Microsoft.Extensions.DependencyInjection;

namespace Bloggi.Backend.EditorJS.Renderer;

public static class EditorJsRendererBootstrapExtensions
{
    public static IServiceCollection AddEditorJs(this IServiceCollection services)
    {
        services.AddSingleton<InlineProcessor>();
        services.AddSingleton<EditorJsRenderer>();
        services.AddSingleton<IHtmlDocumentWriter, HtmlDocumentWriter>((_ => HtmlDocumentWriter.Create()));
        
        // Unknown block is the default block type.
        services.AddKeyedSingleton<ITextExtractor, UnknownBlockTextExtractor>(nameof(BlockTypes.Unknown));
        services.AddKeyedSingleton<IBlockRenderer, UnknownBlockRenderer>(nameof(BlockTypes.Unknown));
        
        // Paragraph block
        services.AddKeyedSingleton<ITextExtractor, ParagraphTextExtractor>(nameof(BlockTypes.Paragraph));
        services.AddKeyedSingleton<IBlockRenderer, ParagraphRenderer>(nameof(BlockTypes.Paragraph));
        
        // Header block
        services.AddKeyedSingleton<ITextExtractor, HeaderTextExtractor>(nameof(BlockTypes.Header));
        services.AddKeyedSingleton<IBlockRenderer, HeaderBlockRenderer>(nameof(BlockTypes.Header));
        
        // Code block
        services.AddKeyedSingleton<ITextExtractor, UnknownBlockTextExtractor>(nameof(BlockTypes.Code));
        services.AddKeyedSingleton<IBlockRenderer, CodeBlockRenderer>(nameof(BlockTypes.Code));
        
        // Warning block
        services.AddKeyedSingleton<ITextExtractor, WarningTextExtractor>(nameof(BlockTypes.Warning));
        services.AddKeyedSingleton<IBlockRenderer, WarningBlockRenderer>(nameof(BlockTypes.Warning));
        
        // Image block
        services.AddKeyedSingleton<ITextExtractor, ImageTextExtractor>(nameof(BlockTypes.Image));
        services.AddKeyedSingleton<IBlockRenderer, ImageBlockRenderer>(nameof(BlockTypes.Image));
        
        // Link block
        services.AddKeyedSingleton<ITextExtractor, LinkTextExtractor>(nameof(BlockTypes.Link));
        services.AddKeyedSingleton<IBlockRenderer, LinkBlockRenderer>(nameof(BlockTypes.Link));
        
        // Quote block
        services.AddKeyedSingleton<ITextExtractor, QuoteTextExtractor>(nameof(BlockTypes.Quote));
        services.AddKeyedSingleton<IBlockRenderer, QuoteBlockRenderer>(nameof(BlockTypes.Quote));
        return services;
    }
}