using AngleSharp;
using AngleSharp.Dom;

namespace Bloggi.Backend.EditorJS.Core;
public sealed class HtmlDocumentWriter : IHtmlDocumentWriter
{
    private readonly IDocument _document;
    private readonly IBrowsingContext _context;

    private HtmlDocumentWriter(IDocument document, IBrowsingContext context)
    {
        _document = document;
        _context = context;
    }

    public static HtmlDocumentWriter Create()
    {
        var context = BrowsingContext.New(Configuration.Default);
        var document = context.OpenAsync(r => r.Content("<html></html>"))
            .GetAwaiter().GetResult();
        return new HtmlDocumentWriter(document, context);
    }

    public string Create(string tagName, Action<IElement> configure)
    {
        return CreateElement(tagName, configure).OuterHtml;
    }

    public IElement CreateElement(string tagName, Action<IElement> configure)
    {
        var element = _document.CreateElement(tagName);
        configure(element);
        return element;
    }

    public IElement CreateElement(string tagName)
        => _document.CreateElement(tagName);

    public IHtmlDocumentWriter Append(IElement parent, IElement child)
    {
        parent.AppendChild(child);
        return this;
    }

    public string CleanScriptTags(string html)
    {
        return html;
    }
}