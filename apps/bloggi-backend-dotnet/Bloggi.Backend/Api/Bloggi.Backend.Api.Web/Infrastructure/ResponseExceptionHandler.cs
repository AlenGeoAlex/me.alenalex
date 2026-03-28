using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Bloggi.Backend.Api.Web.Infrastructure;

public class ResponseExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception exception, CancellationToken ct)
    {
        var problem = new ProblemDetails
        {
            Title  = "Unexpected Error",
            Detail = exception.Message,
            Status = StatusCodes.Status500InternalServerError,
            Type   = "https://httpstatuses.com/500"
        };

        if (problem.Detail == "An exception has been raised that is likely due to a transient failure")
        {
            problem.Detail = "Seems like the database is down. Please try again later.";
        }

        ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
        ctx.Response.Headers.Append("Content-Type", "application/problem+json");
        await ctx.Response.WriteAsJsonAsync(problem, ct);
        return true; // true = exception is handled, stop propagation
    }
}