namespace Bloggi.Backend.EditorJS.Core.Models;

public record InlineRule(
    string MatchTag,
    string? MatchClass,
    string OutputTag,
    string? OutputClass,
    IReadOnlyList<string> AllowedAttributes
);