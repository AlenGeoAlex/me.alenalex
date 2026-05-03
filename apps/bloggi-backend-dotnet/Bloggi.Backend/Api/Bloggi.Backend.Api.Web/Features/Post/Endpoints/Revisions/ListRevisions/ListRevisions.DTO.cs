using Microsoft.AspNetCore.Mvc;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.Revisions.ListRevisions;

internal static partial class ListRevisions
{
    private record Request([FromRoute] Guid PostId);

    private record Revision(
        Guid Id,
        DateTimeOffset CreatedAt,
        int RevisionNumber,
        string? Key,
        bool IsPublished,
        bool IsCurrent,
        DateTimeOffset? PublishedAt
    );
    
    private record Response(
        Guid PostId,
        Revision[] Revisions
        );
}