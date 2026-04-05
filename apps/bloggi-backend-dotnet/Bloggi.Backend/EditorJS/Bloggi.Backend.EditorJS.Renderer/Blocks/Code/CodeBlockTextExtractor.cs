using Bloggi.Backend.EditorJS.Core;
using Bloggi.Backend.EditorJS.Core.Models;
using Microsoft.Extensions.Logging;

namespace Bloggi.Backend.EditorJS.Renderer.Blocks.Code;

public class CodeBlockTextExtractor(ILogger<AbstractBlockTextExtractor> logger) : AbstractBlockTextExtractor<CodeData>(logger)
{
    public override BlockTypes BlockType => BlockTypes.Code;
    public override Task<string> ExtractAsync(EditorBlock<CodeData> block, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(string.Empty);
    }
}