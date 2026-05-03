using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.Revisions.FromExistingRevision;

internal static partial class FromExistingRevision
{
    private record Request(
        [FromRoute] Guid PostId,
        [FromRoute] Guid RevisionId,
        [property: FastEndpoints.FromQuery] bool Confirmation
        );

    class Validator : Validator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Confirmation)
                .NotNull().WithMessage("Confirmation is required");
            
            RuleFor(x => x.Confirmation)
                .Must(x => x)
                .WithMessage("Acknowledgement is required since the action is destructive.");
        }
    }
}