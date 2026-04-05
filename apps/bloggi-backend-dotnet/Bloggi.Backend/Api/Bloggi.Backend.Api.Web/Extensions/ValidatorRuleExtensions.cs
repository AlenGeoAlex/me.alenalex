using FluentValidation;

namespace Bloggi.Backend.Api.Web.Extensions;

public static class ValidatorRuleExtensions
{
    public static IRuleBuilderOptions<T, string> IsUrl<T>(this IRuleBuilderOptions<T, string> ruleBuilder)
    {
        return ruleBuilder.Must(url => Uri.TryCreate(url, UriKind.Absolute, out _));
    }
}