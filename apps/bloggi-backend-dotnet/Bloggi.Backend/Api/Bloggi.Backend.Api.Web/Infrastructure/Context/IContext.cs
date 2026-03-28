namespace Bloggi.Backend.Api.Web.Infrastructure.Context;

public interface IContext
{
    
    public ContextKind Kind { get; }
    
    public Guid UserId { get; }
    
    
    
    public enum ContextKind
    {
        NonValidatedApiRequest,
        ValidatedApiRequest,
        BackgroundJob,
        ContextAwareJob,
    } 
}