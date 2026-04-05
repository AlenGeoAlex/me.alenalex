using Bloggi.Backend.Api.Web.Extensions;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.Editor.SaveUrl;

internal static partial class SaveUrl
{
    private record Request(
        [property: FromRoute] Guid PostId,
        string Url);
    
    private record Response(string Url);

    class Validator : Validator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Url)
                .NotNull()
                .NotEmpty()
                .IsUrl()
                .WithMessage("Url is required");
        }
    }
}