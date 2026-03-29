using Bloggi.Backend.Api.Web.Database;
using Bloggi.Backend.Api.Web.Extensions;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Bloggi.Backend.Api.Web.Features.Post.Services;

public class TagsService(
    ILogger<TagsService> logger,
    IBloggiDbContext context
    )
{

    public async Task<ErrorOr<List<Tag>>> UpsertTagsAndGetIdAsync(
        UpsertTagsAndGetIdRequest request,
        CancellationToken ct = default)
    {
        logger.LogInformation("Upserting tags {Tags}", string.Join(", ", request.Tags));

        var tags = request.Tags
            .Select(name => new { DisplayName = name, Slug = name.Slugify() })
            .ToList();

        var displayNames = tags.Select(t => t.DisplayName).ToArray();
        var slugs        = tags.Select(t => t.Slug).ToArray();

        var sql = """
                  INSERT INTO post.tags (id, slug, display_name, created_at)
                  SELECT gen_random_uuid(), unnest(@slugs), unnest(@displayNames), now()
                  ON CONFLICT (slug) DO UPDATE SET slug = EXCLUDED.slug
                  RETURNING *;
                  """;

        var result = await context.Tags
            .FromSqlRaw(sql,
                new NpgsqlParameter("slugs",        slugs),
                new NpgsqlParameter("displayNames", displayNames))
            .ToListAsync(ct);

        List<Tag> mapped = result
            .Select(t => new Tag(t.Id, t.Slug, t.DisplayName))
            .ToList();

        return mapped;
    }

    #region Models

    public record UpsertTagsAndGetIdRequest(string[] Tags);

    public record Tag(Guid Id, string Slug, string DisplayName);

    #endregion

}