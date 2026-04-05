using System.Net;
using AngleSharp.Dom;
using Bloggi.Backend.EditorJS.Core;
using Bloggi.Backend.EditorJS.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bloggi.Backend.EditorJS.Renderer;

public abstract class AbstractBlockRenderer(
    ILogger<AbstractBlockRenderer> logger,
    InlineProcessor inlineProcessor,
    IHtmlDocumentWriter htmlDocumentWriter
    ) : IBlockRenderer
{
    protected readonly ILogger<AbstractBlockRenderer> Logger = logger;
    protected readonly InlineProcessor InlineProcessor = inlineProcessor;
    protected readonly IHtmlDocumentWriter HtmlDocumentWriter = htmlDocumentWriter;
    
    public abstract BlockTypes BlockType { get; }
    public abstract IReadOnlyList<InlineRule> InlineRules { get; }
    public abstract Task<string> RenderAsync(EditorBlock block, RenderOptions options, CancellationToken cancellationToken = default);
}

public abstract class AbstractBlockRenderer<TData>(ILogger<AbstractBlockRenderer> logger, InlineProcessor inlineProcessor, IHtmlDocumentWriter htmlDocumentWriter) : AbstractBlockRenderer(logger, inlineProcessor, htmlDocumentWriter), IBlockRenderer<TData> where TData : class
{
    public override Task<string> RenderAsync(EditorBlock block, RenderOptions options, CancellationToken ct = default)
        => RenderAsync(new EditorBlock<TData>(block), options, ct);

    public Task<string> RenderAsync(EditorBlock<TData> block, RenderOptions options, CancellationToken ct = default)
        => RenderCoreAsync(block, options, ct);
    
    protected abstract Task<string> RenderCoreAsync(EditorBlock<TData> block, RenderOptions options, CancellationToken ct);
    
    protected Task<string> ProcessInlineAsync(string? raw)
        => InlineProcessor.ProcessAsync(raw ?? string.Empty, InlineRules);

    protected static string EncodePlain(string? raw)
        => WebUtility.HtmlEncode(raw ?? string.Empty);
}