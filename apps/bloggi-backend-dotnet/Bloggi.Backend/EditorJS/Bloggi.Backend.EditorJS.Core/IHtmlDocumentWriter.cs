using AngleSharp.Dom;

namespace Bloggi.Backend.EditorJS.Core;

public interface IHtmlDocumentWriter
{
    /// <summary>
    /// Creates an element, configures it via the action, and returns its outer HTML string.
    /// Use this for simple single-element blocks.
    /// </summary>
    string Create(string tagName, Action<IElement> configure);

    /// <summary>
    /// Creates an HTML element with the specified tag name, applies the provided configuration action,
    /// and returns the element.
    /// </summary>
    /// <param name="tagName">The name of the tag for the HTML element to be created.</param>
    /// <param name="configure">An action to configure the properties or children of the created element.</param>
    /// <returns>The created HTML element.</returns>
    IElement CreateElement(string tagName, Action<IElement> configure);

    /// <summary>
    /// Creates a bare element you can mutate and nest children into before
    /// calling OuterHtml yourself. Use this for complex nested structures.
    /// </summary>
    IElement CreateElement(string tagName);

    /// <summary>
    /// Appends child into parent and returns parent — for fluent nesting.
    /// </summary>
    IHtmlDocumentWriter Append(IElement parent, IElement child);

    /// <summary>
    /// Removes all script tags and their content from the provided HTML string
    /// to ensure it is sanitized and free from potentially harmful scripts.
    /// </summary>
    /// <param name="html">The HTML string to be sanitized by removing script tags.</param>
    /// <returns>The sanitized HTML string with all script tags removed.</returns>
    string CleanScriptTags(string html);
}