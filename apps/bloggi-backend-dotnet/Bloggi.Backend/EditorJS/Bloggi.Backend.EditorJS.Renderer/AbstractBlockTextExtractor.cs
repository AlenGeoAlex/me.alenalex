using System.Text.RegularExpressions;
using Bloggi.Backend.EditorJS.Core;
using Bloggi.Backend.EditorJS.Core.Models;
using Microsoft.Extensions.Logging;

namespace Bloggi.Backend.EditorJS.Renderer;

public abstract class AbstractBlockTextExtractor(
    ILogger<AbstractBlockTextExtractor> logger
) : ITextExtractor
{
    protected readonly ILogger<AbstractBlockTextExtractor> Logger = logger;

    public abstract BlockTypes BlockType { get; }

    public abstract Task<string> ExtractAsync(EditorBlock block, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes HTML tags and decodes certain HTML entities from the input string.
    /// Converts the input into a plain text representation.
    /// </summary>
    /// <param name="raw">The input string containing HTML.</param>
    /// <returns>A plain text string with HTML tags removed and entities decoded.</returns>
    protected static string StripHtml(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
        return Regex.Replace(raw, "<[^>]*>", " ")
            .Replace("&nbsp;", " ")
            .Replace("&amp;", "&")
            .Replace("&lt;", "<")
            .Replace("&gt;", ">")
            .Replace("&quot;", "\"")
            .Replace("&#039;", "'")
            .Trim();
    }

    // Joins a collection of strings with a space, stripping HTML from each
    protected static string JoinItems(IEnumerable<string?> items)
        => string.Join(' ', items
            .Select(StripHtml)
            .Where(s => !string.IsNullOrWhiteSpace(s)));
}

public abstract class AbstractBlockTextExtractor<TData>(
    ILogger<AbstractBlockTextExtractor> logger
) : AbstractBlockTextExtractor(logger), ITextExtractor<TData>
    where TData : class
{
    public override Task<string> ExtractAsync(EditorBlock block, CancellationToken cancellationToken = default)
        => ExtractAsync(new EditorBlock<TData>(block), cancellationToken);

    public abstract Task<string> ExtractAsync(EditorBlock<TData> block, CancellationToken cancellationToken = default);
}