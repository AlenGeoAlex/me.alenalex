using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.Preview.Render;

internal static partial class Render
{
    
    private record Request(
        [property: FromRoute] Guid PostId
    );
    
    private record Response(
        byte[] Html,
        ResponseError[] Errors
        
    );

    private record ResponseError(string Code, string Message, string Type);

    class Validator : Validator<Request>
    {
        public Validator()
        {
            RuleFor(r => r.PostId)
                .NotEmpty();
        }
    }
    
}