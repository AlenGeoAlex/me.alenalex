using System.Collections.Concurrent;
using System.Linq.Expressions;
using ErrorOr;
using FastEndpoints;
using FluentValidation.Results;

namespace Bloggi.Backend.Api.Web.Infrastructure;

public class ResponseSender : IGlobalPostProcessor
{
    private static readonly ConcurrentDictionary<Type, Func<object, object>> _valueAccessors = new();
    
    public Task PostProcessAsync(IPostProcessorContext ctx, CancellationToken ct)
    {
        if (ctx.HttpContext.ResponseStarted() || ctx.Response is not IErrorOr errorOr)
        {
            return Task.CompletedTask;
        }
        
        if (!errorOr.IsError)
            return ctx.HttpContext.Response.SendAsync(GetValueFromErrorOr(errorOr), cancellation: ct);

        if (errorOr.Errors?.All(e => e.Type == ErrorType.Validation) is true)
        {
            return ctx.HttpContext.Response.SendErrorsAsync(
                failures: [..errorOr.Errors.Select(e => new ValidationFailure(e.Code, e.Description))],
                cancellation: ct);
        }

        var problem = errorOr.Errors?.FirstOrDefault(e => e.Type != ErrorType.Validation);

        if(!problem.HasValue)
            throw new InvalidOperationException();

        var error = problem.Value;
        var errorDescription = error.Description;
        switch (error.Type)
        {
            case ErrorType.NotFound:
            {
                return ctx.HttpContext.Response.SendAsync(errorDescription, statusCode: StatusCodes.Status404NotFound, cancellation: ct);
            }
            case ErrorType.Failure:
            {
                return ctx.HttpContext.Response.SendAsync(errorDescription, statusCode: StatusCodes.Status400BadRequest, cancellation: ct);
            }
            case ErrorType.Conflict:
            {
                return ctx.HttpContext.Response.SendAsync(errorDescription, statusCode: StatusCodes.Status409Conflict, cancellation: ct);
            }
            case ErrorType.Unauthorized:
            {
                return ctx.HttpContext.Response.SendAsync(errorDescription, statusCode: StatusCodes.Status401Unauthorized, cancellation: ct);
            }
            case ErrorType.Forbidden:
            {
                return ctx.HttpContext.Response.SendAsync(errorDescription, statusCode: StatusCodes.Status403Forbidden, cancellation: ct);
            }
            case ErrorType.Validation:
            {
                return ctx.HttpContext.Response.SendErrorsAsync(
                    failures:
                    [
                        new ValidationFailure(error.Code, error.Description)
                    ],
                    cancellation: ct
                );
            }
            case ErrorType.Unexpected:
            default:
                return ctx.HttpContext.Response.SendAsync(errorDescription, statusCode: StatusCodes.Status500InternalServerError, cancellation: ct);
        }
    }
    
    static object GetValueFromErrorOr(object errorOr)
    {
        ArgumentNullException.ThrowIfNull(errorOr);
        var tErrorOr = errorOr.GetType();

        if (!tErrorOr.IsGenericType || tErrorOr.GetGenericTypeDefinition() != typeof(ErrorOr<>))
            throw new InvalidOperationException("The provided object is not an instance of ErrorOr<>.");

        return _valueAccessors.GetOrAdd(tErrorOr, CreateValueAccessor)(errorOr);

        static Func<object, object> CreateValueAccessor(Type errorOrType)
        {
            var parameter = Expression.Parameter(typeof(object), "errorOr");

            return Expression.Lambda<Func<object, object>>(
                    Expression.Convert(
                        Expression.Property(
                            Expression.Convert(parameter, errorOrType),
                            "Value"),
                        typeof(object)),
                    parameter)
                .Compile();
        }
    }
}