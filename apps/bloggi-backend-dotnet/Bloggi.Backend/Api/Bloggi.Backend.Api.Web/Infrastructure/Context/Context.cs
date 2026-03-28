using System.Security.Claims;
using Bloggi.Backend.Api.Web.Exceptions.Common;

namespace Bloggi.Backend.Api.Web.Infrastructure.Context;

public class NonValidatedContext : IContext
{
    public IContext.ContextKind Kind => IContext.ContextKind.NonValidatedApiRequest;
    public Guid UserId { get; } = Guid.Empty;
}

public class Context : IContext
{
    public IContext.ContextKind Kind => IContext.ContextKind.ContextAwareJob;
    public Guid UserId { get; }

    public Context(IContext context)
    {
        UserId = context.UserId;
    }
}

public class RequestContext : IContext
{
    public IContext.ContextKind Kind => IContext.ContextKind.ValidatedApiRequest;
    public Guid UserId { get; }

    public RequestContext(ClaimsPrincipal claimsPrincipal)
    {
        UserId = GetUserId(claimsPrincipal);
    }

    private Guid GetUserId(ClaimsPrincipal claimsPrincipal)
    {
        var value = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(value))
            throw new FailedToIdentifyUserException();
        
        if (Guid.TryParse(value, out var result))
        {
            return result;
        }
        
        throw new FailedToIdentifyUserException();
    }
}