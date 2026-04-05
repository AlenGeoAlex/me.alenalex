using Bloggi.Backend.EditorJS.Core;
using Bloggi.Backend.EditorJS.Core.Models;
using Microsoft.Extensions.Logging;

namespace Bloggi.Backend.EditorJS.Renderer.Blocks.Link;

public class LinkBlockRenderer(ILogger<AbstractBlockRenderer> logger, InlineProcessor inlineProcessor, IHtmlDocumentWriter htmlDocumentWriter) : AbstractBlockRenderer<LinkData>(logger, inlineProcessor, htmlDocumentWriter)
{
    public override BlockTypes BlockType => BlockTypes.Link;
    public override IReadOnlyList<InlineRule> InlineRules => Array.Empty<InlineRule>();
    protected override async Task<string> RenderCoreAsync(EditorBlock<LinkData> block, RenderOptions options, CancellationToken ct)
    {
        var linkData = block.TypedData;
        var url  = linkData.Link;
        var meta = linkData.Meta;
        var host = Uri.TryCreate(url, UriKind.Absolute, out var uri)
                   ? uri.Host.Replace("www.", "")
                   : url;

        var hasTitle       = !string.IsNullOrWhiteSpace(meta?.Title);
        var hasDescription = !string.IsNullOrWhiteSpace(meta?.Description);
        var hasImage       = !string.IsNullOrWhiteSpace(meta?.Image?.Url);
        var hasMeta        = hasTitle || hasDescription;

        var modifier = !hasMeta  ? " post-link-card--bare"
                     : !hasImage ? " post-link-card--no-image"
                     : string.Empty;

        var anchor = HtmlDocumentWriter.CreateElement("a", a =>
        {
            a.SetAttribute("class", $"post-link-card{modifier}");
            a.SetAttribute("href", EncodePlain(url));
            a.SetAttribute("target", "_blank");
            a.SetAttribute("rel", "noopener noreferrer");
        });

        var body = HtmlDocumentWriter.CreateElement("div", d =>
        {
            d.SetAttribute("class", "post-link-card-body");
        });

        var hostSpan = HtmlDocumentWriter.CreateElement("span", s =>
        {
            s.SetAttribute("class", "post-link-card-host");
            s.InnerHtml = host;
        });
        HtmlDocumentWriter.Append(body, hostSpan);

        if (hasTitle)
        {
            var titleSpan = HtmlDocumentWriter.CreateElement("span", s =>
            {
                s.SetAttribute("class", "post-link-card-title");
                s.InnerHtml = meta!.Title!;
            });
            HtmlDocumentWriter.Append(body, titleSpan);
        }

        if (hasDescription)
        {
            var descSpan = HtmlDocumentWriter.CreateElement("span", s =>
            {
                s.SetAttribute("class", "post-link-card-description");
                s.InnerHtml = meta!.Description!;
            });
            HtmlDocumentWriter.Append(body, descSpan);
        }

        var urlSpan = HtmlDocumentWriter.CreateElement("span", s =>
        {
            s.SetAttribute("class", "post-link-card-url");
        });
        var linkIcon = HtmlDocumentWriter.CreateElement("i", i =>
        {
            i.SetAttribute("class", "pi pi-link");
        });
        var urlText = HtmlDocumentWriter.CreateElement("span", s =>
        {
            s.TextContent = url;
        });
        HtmlDocumentWriter.Append(urlSpan, linkIcon);
        HtmlDocumentWriter.Append(urlSpan, urlText);
        HtmlDocumentWriter.Append(body, urlSpan);

        HtmlDocumentWriter.Append(anchor, body);

        if (hasImage)
        {
            var imageDiv = HtmlDocumentWriter.CreateElement("div", d =>
            {
                d.SetAttribute("class", "post-link-card-image");
            });
            var img = HtmlDocumentWriter.CreateElement("img", i =>
            {
                i.SetAttribute("src", EncodePlain(meta!.Image!.Url!));
                i.SetAttribute("alt", EncodePlain(meta.Title ?? host));
                i.SetAttribute("loading", "lazy");
            });
            HtmlDocumentWriter.Append(imageDiv, img);
            HtmlDocumentWriter.Append(anchor, imageDiv);
        }

        return anchor.OuterHtml;
    }
}