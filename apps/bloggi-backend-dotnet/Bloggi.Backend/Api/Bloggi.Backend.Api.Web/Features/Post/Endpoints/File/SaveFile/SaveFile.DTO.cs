using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.File.SaveFile;

internal static partial class SaveFile
{
    private record Request(
        [FromRoute] Guid PostId,
        string Name,
        string Hash,
        string ContentType,
        long Size
    );

    private record Response(
        string Url,
        string? SignedUrl,
        DateTimeOffset? ExpiresAt
    );

    private class Validator : Validator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .NotNull()
                .MaximumLength(230);
            
            RuleFor(x => x.Hash)
                .NotEmpty()
                .NotNull()
                .MaximumLength(255);
            
            RuleFor(x => x.ContentType)
                .NotEmpty()
                .NotNull()
                .MaximumLength(255);

            RuleFor(x => x.Size)
                .NotEmpty()
                .GreaterThan(0);
            
            RuleFor(x => x.ContentType)
                .Must(x => x.StartsWith("image/"))
                .WithMessage("Only images are allowed");

            RuleFor(x => x.PostId)
                .NotNull();
        }
    }
}