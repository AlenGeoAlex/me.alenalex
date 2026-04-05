using Bloggi.Backend.Api.Web.Extensions;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.Editor.LinkMeta;

internal static partial class LinkMeta
{
    private record Request(
        [property: FromRoute] Guid PostId, 
        string Url
        );
    
    private record Response(
        string? Title,
        string? Description,
        string? ImageUrl,
        string? Favicon
    );

    class Validator : Validator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Url)
                .NotNull()
                .IsUrl()
                .WithMessage("Url is required");
        }
    }
    
}