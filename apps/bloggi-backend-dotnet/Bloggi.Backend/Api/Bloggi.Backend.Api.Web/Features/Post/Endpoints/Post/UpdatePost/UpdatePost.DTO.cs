using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.Post.UpdatePost;

internal static partial class UpdatePost
{
    private record Request(
        [FromRoute] Guid PostId,
        string Title,
        string Excerpt,
        string[] Tags
    );

    private record Response(
        Guid Id
    );

    class Validator : Validator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Title)
                .NotNull()
                .NotEmpty()
                .MinimumLength(5)
                .MaximumLength(200);
            
            RuleFor(x => x.Excerpt)
                .MaximumLength(200);
            
            RuleFor(x => x.Tags)
                .Must(x => x.Length <= 10)
                .WithMessage("No more than 10 tags allowed");
            
            RuleFor(x => x.Tags)
                .Must(x => x.Distinct().Count() == x.Length)
                .WithMessage("Duplicate tags are not allowed");
        }   
    }
}