using Bloggi.Backend.Api.Web.Features.User;
using Microsoft.Extensions.Primitives;

namespace Bloggi.Backend.Api.Web.Infrastructure.Context;

public interface IContextFactory
{
    public const string ApiKey = "Bloggi.Backend.Api.Context";
    
    public const string JobKey = "Bloggi.Backend.Job.Context";
    public IContext Context { get; }
}

public class JobContextFactory : IContextFactory
{
    public IContext Context { get; }
}

public class ApiContextFactory : IContextFactory
{
    private readonly IHttpContextAccessor _contextAccessor;

    public ApiContextFactory(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
        Context = CreateContext();
    }
    
    public IContext Context { get; }

    private IContext CreateContext()
    {
        var ctx = _contextAccessor.HttpContext;
        if (ctx == null || 
            (!ctx.Request.Cookies.TryGetValue(Constants.Auth.AccessTokenCookieName, out var tokenValue) || string.IsNullOrWhiteSpace(tokenValue)) ) return new NonValidatedContext();

        var ctxUser = ctx.User;
        return new RequestContext(ctxUser);
    }
}