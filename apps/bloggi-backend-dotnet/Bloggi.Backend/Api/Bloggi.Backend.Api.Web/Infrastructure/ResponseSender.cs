using System.Collections.Concurrent;
using System.Linq.Expressions;
using ErrorOr;
using FastEndpoints;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using ProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;

namespace Bloggi.Backend.Api.Web.Infrastructure;

public class ResponseSender : IGlobalPostProcessor
{
    private static readonly ConcurrentDictionary<Type, Func<object, object>> _valueAccessors = new();

    public Task PostProcessAsync(IPostProcessorContext ctx, CancellationToken ct)
    {
        if (ctx.HttpContext.ResponseStarted() || ctx.Response is not IErrorOr errorOr)
            return Task.CompletedTask;

        if (!errorOr.IsError)
            return ctx.HttpContext.Response.SendAsync(GetValueFromErrorOr(errorOr), cancellation: ct);

        if (errorOr.Errors?.All(e => e.Type == ErrorType.Validation) is true)
        {
            return ctx.HttpContext.Response.SendErrorsAsync(
                failures: [..errorOr.Errors.Select(e => new ValidationFailure(e.Code, e.Description))],
                cancellation: ct);
        }

        var problem = errorOr.Errors?.FirstOrDefault(e => e.Type != ErrorType.Validation);

        if (!problem.HasValue)
            throw new InvalidOperationException();

        var error = problem.Value;

        return error.Type switch
        {
            ErrorType.NotFound => ctx.HttpContext.Response.SendAsync(
                ToProblemDetails(error, StatusCodes.Status404NotFound),
                statusCode: StatusCodes.Status404NotFound,
                cancellation: ct),

            ErrorType.Failure => ctx.HttpContext.Response.SendAsync(
                ToProblemDetails(error, StatusCodes.Status400BadRequest),
                statusCode: StatusCodes.Status400BadRequest,
                cancellation: ct),

            ErrorType.Conflict => ctx.HttpContext.Response.SendAsync(
                ToProblemDetails(error, StatusCodes.Status409Conflict),
                statusCode: StatusCodes.Status409Conflict,
                cancellation: ct),

            ErrorType.Unauthorized => ctx.HttpContext.Response.SendAsync(
                ToProblemDetails(error, StatusCodes.Status401Unauthorized),
                statusCode: StatusCodes.Status401Unauthorized,
                cancellation: ct),

            ErrorType.Forbidden => ctx.HttpContext.Response.SendAsync(
                ToProblemDetails(error, StatusCodes.Status403Forbidden),
                statusCode: StatusCodes.Status403Forbidden,
                cancellation: ct),

            ErrorType.Validation => ctx.HttpContext.Response.SendErrorsAsync(
                failures: [new ValidationFailure(error.Code, error.Description)],
                cancellation: ct),

            _ => ctx.HttpContext.Response.SendAsync(
                ToProblemDetails(error, StatusCodes.Status500InternalServerError),
                statusCode: StatusCodes.Status500InternalServerError,
                cancellation: ct),
        };
    }

    private static string FormatErrorType(ErrorType type) => type switch
    {
        ErrorType.NotFound   => "Not Found",
        ErrorType.Unexpected => "Unexpected",
        ErrorType.Failure    => "Failure",
        ErrorType.Conflict   => "Conflict",
        ErrorType.Unauthorized => "Unauthorized",
        ErrorType.Forbidden  => "Forbidden",
        ErrorType.Validation => "Validation",
        _                    => type.ToString()
    };

    private static ProblemDetails ToProblemDetails(Error error, int statusCode) => new()
    {
        Title = $"{FormatErrorType(error.Type)} Error",
        Detail = error.Description,
        Status = statusCode,
        Type = $"https://httpstatuses.com/{statusCode}",
        Extensions = { ["code"] = error.Code }
    };

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