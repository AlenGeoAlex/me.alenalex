using System.Net;
using AngleSharp;
using AngleSharp.Dom;
using Bloggi.Backend.EditorJS.Core.Models;

namespace Bloggi.Backend.EditorJS.Core;

public class InlineProcessor : IDisposable, IAsyncDisposable
{
    private readonly IBrowsingContext _context = BrowsingContext.New(Configuration.Default);

    // rules empty → HtmlEncode (plain text field)
    // rules non-empty → parse, transform, strip unknowns
    public async Task<string> ProcessAsync(string raw, IReadOnlyList<InlineRule> rules)
    {
        if (string.IsNullOrEmpty(raw))
            return string.Empty;

        if (rules.Count == 0)
            return WebUtility.HtmlEncode(raw);

        var doc = await _context.OpenAsync(r => r.Content($"<body>{raw}</body>"));
        Transform(doc.Body!, rules, doc);
        return doc.Body!.InnerHtml;
    }

    private static void Transform(IElement parent, IReadOnlyList<InlineRule> rules, IDocument doc)
    {
        foreach (var node in parent.ChildNodes.ToList())
        {
            if (node is IText) continue;  // text nodes always pass through

            if (node is not IElement el)
            {
                node.Parent?.RemoveChild(node);
                continue;
            }

            var rule = Match(el, rules);

            if (rule is null)
            {
                // unknown element — keep its text, strip the tag
                var text = doc.CreateTextNode(el.TextContent);
                parent.ReplaceChild(text, el);
                continue;
            }

            // recurse before replacing so children are already transformed
            Transform(el, rules, doc);

            var output = doc.CreateElement(rule.OutputTag);

            if (rule.OutputClass is not null)
                output.ClassName = rule.OutputClass;

            // pass through only declared attributes
            foreach (var attr in rule.AllowedAttributes)
            {
                var value = el.GetAttribute(attr);
                if (value is not null)
                    output.SetAttribute(attr, value);
            }

            // href safety — strip anything that is not a safe scheme
            if (output.HasAttribute("href"))
            {
                var href = output.GetAttribute("href")!;
                var safe = href.StartsWith("https://")
                        || href.StartsWith("http://")
                        || href.StartsWith("/")
                        || href.StartsWith("#");
                if (!safe) output.RemoveAttribute("href");
            }

            // move children into output element
            while (el.FirstChild is not null)
                output.AppendChild(el.FirstChild);

            parent.ReplaceChild(output, el);
        }
    }

    private static InlineRule? Match(IElement el, IReadOnlyList<InlineRule> rules)
    {
        var tag = el.TagName.ToLowerInvariant();
        var cls = el.ClassName ?? string.Empty;

        return rules.FirstOrDefault(r =>
            r.MatchTag == tag &&
            (r.MatchClass is null || cls.Split(' ').Contains(r.MatchClass)));
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_context is IAsyncDisposable contextAsyncDisposable)
            await contextAsyncDisposable.DisposeAsync();
        else
            _context.Dispose();
    }
}