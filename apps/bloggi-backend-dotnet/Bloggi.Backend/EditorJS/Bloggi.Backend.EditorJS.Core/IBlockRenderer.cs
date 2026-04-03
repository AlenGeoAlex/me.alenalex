using Bloggi.Backend.EditorJS.Core.Models;

namespace Bloggi.Backend.EditorJS.Core;

/// <summary>
/// 
/// </summary>
public interface IBlockRenderer
{
    /// <summary>
    /// Represents the type of an EditorJS content block. This property specifies the category or format
    /// of the block, such as Paragraph, Header, Image, List, and others.
    /// The <see cref="BlockTypes"/> enumeration provides a predefined set of possible values
    /// for the block types.
    /// </summary>
    BlockTypes BlockType { get; }

    /// <summary>
    /// Represents a collection of rules used to handle inline content transformations within an
    /// EditorJS block. These rules define mappings between input HTML tags or attributes and their
    /// corresponding output formats, enabling custom rendering or processing of inline-level elements.
    /// </summary>
    IReadOnlyList<InlineRule> InlineRules { get; }

    /// <summary>
    /// Asynchronously renders the content of the provided EditorJS block into a specific output format
    /// based on the associated rendering rules and supplied rendering options.
    /// </summary>
    /// <param name="block">
    /// The EditorJS block containing the data to be processed and rendered. This includes the block type,
    /// data, and associated metadata.
    /// </param>
    /// <param name="options">
    /// Configuration options that modify the rendering behavior, such as toggling preview mode or altering rendering styles.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to observe for cancellation requests, enabling cooperative cancellation of the render operation if needed.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task's result is a string containing the
    /// formatted output of the rendered block.
    /// </returns>
    Task<string> RenderAsync(EditorBlock block, RenderOptions options, CancellationToken cancellationToken = default);
}

/// <inheritdoc/>
public interface IBlockRenderer<TData> : IBlockRenderer where TData : class
{
    
    /// <summary>
    /// Asynchronously renders the provided block data into a specific output format based on rendering rules and options.
    /// </summary>
    /// <typeparam name="TData">The type of the block data being rendered. This type must be a class.</typeparam>
    /// <param name="block">The block containing data to be rendered, including its type and associated metadata.</param>
    /// <param name="options">Optional rendering settings that modify the behavior or appearance of the rendered output.</param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests, enabling the operation to be cancelled if needed.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task's result contains the rendered output as a string.
    /// </returns>
    Task<string> RenderAsync(EditorBlock<TData> block, RenderOptions options, CancellationToken cancellationToken = default);
    
}

public class RenderOptions
{
    public bool IsPreview { get; init; }
}