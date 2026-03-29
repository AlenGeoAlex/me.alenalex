using FastEndpoints;
using FluentValidation;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.Post.CreatePost;

internal static partial class CreatePost
{
    private record Request(
        string Title,
        string? Excerpt,
        string[] Tags
    );

    private record Response(
        Guid Id
    );

    private class Validator : Validator<Request>
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