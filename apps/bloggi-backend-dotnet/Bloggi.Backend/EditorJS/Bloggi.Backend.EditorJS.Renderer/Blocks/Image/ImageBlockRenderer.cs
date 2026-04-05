using Bloggi.Backend.EditorJS.Core;
using Bloggi.Backend.EditorJS.Core.Models;
using Microsoft.Extensions.Logging;

namespace Bloggi.Backend.EditorJS.Renderer.Blocks.Image;

public class ImageBlockRenderer(ILogger<AbstractBlockRenderer> logger, InlineProcessor inlineProcessor, IHtmlDocumentWriter htmlDocumentWriter) : AbstractBlockRenderer<ImageData>(logger, inlineProcessor, htmlDocumentWriter)
{
    public override BlockTypes BlockType => BlockTypes.Image;
    public override IReadOnlyList<InlineRule> InlineRules => Array.Empty<InlineRule>();
    protected override async Task<string> RenderCoreAsync(EditorBlock<ImageData> block, RenderOptions options, CancellationToken ct)
    {
        var imageData = block.TypedData;
        var image = HtmlDocumentWriter.CreateElement("img", img =>
        {
            img.SetAttribute("src", imageData.File.Url);
            img.SetAttribute("loading", "lazy");
            if(!string.IsNullOrWhiteSpace(imageData.Caption))
                img.SetAttribute("alt", imageData.Caption);
            
        });
        var figureClasses = new List<string> { "post-figure" };
        if (block.TypedData.Stretched)       figureClasses.Add("post-figure--stretched");
        if (block.TypedData.WithBorder)      figureClasses.Add("post-figure--bordered");
        if (block.TypedData.WithBackground)  figureClasses.Add("post-figure--bg");

        var figure = HtmlDocumentWriter.CreateElement("figure", f =>
        {
            f.SetAttribute("class", string.Join(" ", figureClasses));
        });
        
        HtmlDocumentWriter.Append(figure, image);
        
        if(string.IsNullOrWhiteSpace(imageData.Caption))
            return image.OuterHtml;

        var figCaption = HtmlDocumentWriter.CreateElement("figcaption", element =>
        {
            element.SetAttribute("class", "post-figcaption");
            element.InnerHtml = imageData.Caption;
        });
        
        HtmlDocumentWriter.Append(figure, figCaption);
        
        return figure.OuterHtml;
    }
}