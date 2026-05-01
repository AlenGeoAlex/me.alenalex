using Bloggi.Backend.Api.Database.Posts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Bloggi.Backend.Api.Web.Features.Post.Endpoints.Post.UpdatePostMeta;

internal static partial class UpdatePostMeta
{
    private record Request(
        [FromRoute] Guid PostId,
        string? OpenGraphTitle,
        string? OpenGraphDescription,
        string? OpenGraphImageUrl,
        string? CanonicalUrl,
        string Robot,
        string? SchemaOrgJson
    );

    class Validator : Validator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.PostId).NotNull()
                .NotEmpty();

            RuleFor(x => x.SchemaOrgJson)
                .Must(x =>
                {
                    if(x == null) return true;
                    
                    try
                    {
                        var json = JObject.Parse(x);
                        return json.Type == JTokenType.Object;
                    }
                    catch (JsonReaderException)
                    {
                        return false;
                    }
                })
                .WithMessage("Invalid schema.org JSON");

            RuleFor(x => x.Robot)
                .NotNull()
                .Must(x =>
                {
                    var robotStrings = x.Split(",");
                    foreach (var robotString in robotStrings)
                    {
                        if (!Enum.TryParse<PostMeta.MetaRobot>(robotString.Trim(), true, out var robot))
                        {
                            return false;
                        }
                    }

                    return true;
                }).WithMessage("Invalid robots.txt rules");
        }
    }
}