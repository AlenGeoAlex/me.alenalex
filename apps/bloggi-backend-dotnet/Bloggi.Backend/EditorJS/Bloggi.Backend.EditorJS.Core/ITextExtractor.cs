using Bloggi.Backend.EditorJS.Core.Models;

namespace Bloggi.Backend.EditorJS.Core;

/// <summary>
/// Defines an interface for extracting text content from an EditorJS block.
/// Implementations of this interface are responsible for handling specific
/// block types and extracting their textual representation.
/// </summary>
public interface ITextExtractor
{
    /// <summary>
    /// Represents the specific type of an EditorJS block.
    /// This property is used to identify and categorize the block type,
    /// enabling the implementation of parsing or handling logic
    /// tailored to the block's characteristics.
    /// </summary>
    BlockTypes BlockType { get; }

    /// <summary>
    /// Asynchronously extracts the textual content from a given EditorJS block.
    /// </summary>
    /// <param name="block">
    /// The EditorJS block from which text content will be extracted. The block contains
    /// information such as its type, ID, and associated data specific to its type.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests, allowing the operation to be cancelled.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous operation. The task result is a string containing
    /// the extracted text content from the specified EditorJS block.
    /// </returns>
    Task<string> ExtractAsync(EditorBlock block, CancellationToken cancellationToken = default);
}

/// <inheritdoc/>
public interface ITextExtractor<TData> : ITextExtractor where TData : class
{
    /// <summary>
    /// Asynchronously extracts the textual content from a specified EditorJS block.
    /// </summary>
    /// <param name="block">
    /// The EditorJS block that contains the content to be extracted. This block may
    /// include metadata, ID, and other data depending on its type.
    /// </param>
    /// <param name="cancellationToken">
    /// A token to monitor for cancellation requests, enabling the operation to be cancelled
    /// if necessary.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result is a string
    /// containing the extracted text content from the given EditorJS block.
    /// </returns>
    Task<string> ExtractAsync(EditorBlock<TData> block, CancellationToken cancellationToken = default);
}
