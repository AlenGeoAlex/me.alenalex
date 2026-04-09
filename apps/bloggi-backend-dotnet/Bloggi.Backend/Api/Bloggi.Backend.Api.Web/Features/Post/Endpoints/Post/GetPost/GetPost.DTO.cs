using System.Text.Json.Serialization;
using Bloggi.Backend.Api.Web.Attributes;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.Post.GetPost;

internal static partial class GetPost
{
    private class Request
    {
        [FromRoute]
        public Guid Id { get; set; }
        
        [QueryEnum(typeof(GetPostIncludeProperty))]
        public string[] Include { get; set; } = Array.Empty<string>();
    }
    
    private enum GetPostIncludeProperty
    {
        Tags,
        Author
    }

    private record Response(
        Guid Id,
        string Title,
        string? Excerpt,
        string Slug,
        TagDto[] Tags,
        StatusDto Status,
        string? RenderedKey,
        DateTimeOffset CreatedAt,
        DateTimeOffset UpdatedAt,
        DateTimeOffset? PublishedAt,
        AuthorDto? Author
    );
    
    private record AuthorDto(Guid Id, string? DisplayName, string? AvatarUrl, string Email);
    
    private enum StatusDto
    {
        Published,
        Draft,
    }
    private record TagDto(Guid Id, string Slug, string DisplayName);

    class Validator : Validator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotNull()
                .NotEmpty()
                .WithMessage("Post ID is required");
        }
    }
}