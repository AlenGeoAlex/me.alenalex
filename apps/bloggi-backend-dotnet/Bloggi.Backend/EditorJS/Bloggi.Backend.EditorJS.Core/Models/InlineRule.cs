namespace Bloggi.Backend.EditorJS.Core.Models;

public record InlineRule(
    string MatchTag,
    string? MatchClass,
    string OutputTag,
    string? OutputClass,
    IReadOnlyList<string> AllowedAttributes
)
{
    /// <summary>
    /// Represents an inline rule for processing text with a bold formatting tag.
    /// </summary>
    /// <remarks>
    /// This rule is designed to match the "b" HTML tag during input processing, and converts it
    /// to the "strong" tag with an optional output class of "post-bold".
    /// No additional attributes are allowed for this rule.
    /// </remarks>
    public static InlineRule Bold => new("b", null, "strong", "post-bold", Array.Empty<string>());

    /// <summary>
    /// Represents an inline rule for processing text with an italic formatting tag.
    /// </summary>
    /// <remarks>
    /// This rule is designed to match the "i" HTML tag during input processing, and converts it
    /// to the "em" tag with an optional output class of "post-italic".
    /// No additional attributes are allowed for this rule.
    /// </remarks>
    public static InlineRule Italic => new("i", null, "em", "post-italic", Array.Empty<string>());

    /// <summary>
    /// Represents an inline rule for processing text with an underline formatting tag.
    /// </summary>
    /// <remarks>
    /// This rule is designed to match the "u" HTML tag during input processing, and converts it
    /// to the "u" tag with an optional output class of "post-underline".
    /// No additional attributes are allowed for this rule.
    /// </remarks>
    public static InlineRule Underline => new("u", null, "u", "post-underline", Array.Empty<string>());

    /// <summary>
    /// Represents an inline rule for processing text with a strikethrough formatting tag.
    /// </summary>
    /// <remarks>
    /// This rule matches the "s" HTML tag during input processing and converts it
    /// to the "s" tag with an optional output class of "post-strikethrough".
    /// No additional attributes are permitted for this rule.
    /// </remarks>
    public static InlineRule Strikethrough => new("s", null, "s", "post-strikethrough", Array.Empty<string>());

    /// <summary>
    /// Represents an inline rule for processing text with an anchor tag.
    /// </summary>
    /// <remarks>
    /// This rule is designed to match the "a" HTML tag with an optional "href" attribute during input processing,
    /// and converts it to the "a" tag with an output class of "post-link".
    /// No additional attributes apart from those specified are allowed for this rule.
    /// </remarks>
    public static InlineRule Anchor => new("a", "href", "a", "post-link", ["href"]);

    /// <summary>
    /// Represents an inline rule for processing text with a highlighting formatting tag.
    /// </summary>
    /// <remarks>
    /// This rule is designed to match the "mark" HTML tag during input processing and converts it
    /// to the "mark" tag with an optional output class of "post-mark".
    /// No additional attributes are allowed for this rule.
    /// </remarks>
    public static InlineRule Mark => new("mark", "cdx-marker", "mark", "post-mark", Array.Empty<string>());

    /// <summary>
    /// Represents an inline rule for processing text with a code formatting tag.
    /// </summary>
    /// <remarks>
    /// This rule matches the "code" HTML tag, specifically with the class "cdx-inline-code",
    /// and converts it to an output tag "code" with an optional output class of "post-inline-code".
    /// No additional attributes are allowed for this rule.
    /// </remarks>
    public static InlineRule Code => new("code", "inline-code", "code","post-inline-code", Array.Empty<string>());
}