using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.Revisions.CreateRevision;

internal static partial class CreateRevision
{
    private record Request(
        [FromRoute] Guid PostId
    );
    
    private record Response(
        Guid Id
    );
    
}