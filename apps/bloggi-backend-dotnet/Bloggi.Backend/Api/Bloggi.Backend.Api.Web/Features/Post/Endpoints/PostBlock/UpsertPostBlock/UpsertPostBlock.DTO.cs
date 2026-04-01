using System.Text.Json;
using Bloggi.Backend.EditorJS.Core;
using Bloggi.Backend.EditorJS.Core.Models;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.PostBlock.UpsertPostBlock;

internal static partial class UpsertPostBlock
{
    private record Request(
        [FromRoute] Guid PostId,
        OutputData EditorJsData
    );

    private record Response(
        Dictionary<string, Guid> BlockId
    );
    
    private class Validator : Validator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.EditorJsData)
                .NotNull();

            RuleFor(x => x.EditorJsData.Time)
                .NotEmpty();

            RuleFor(x => x.EditorJsData.Version)
                .NotEmpty();

            RuleForEach(x => x.EditorJsData.Blocks)
                .NotNull()
                .ChildRules(block =>
                {
                    block.RuleFor(x => x.Id)
                        .NotEmpty()
                        .WithMessage("Block must have an Id.");

                    block.RuleFor(x => x.Type)
                        .IsInEnum();

                    block.RuleFor(x => x.Type)
                        .NotEqual(BlockTypes.Unknown)
                        .WithMessage("Block has an unrecognised or missing type.");

                    block.RuleFor(x => x.Data)
                        .Must(data => data.ValueKind != JsonValueKind.Null && 
                                      data.ValueKind != JsonValueKind.Undefined)
                        .WithMessage("Block data must not be null or undefined.");
                });
        }
    }
}