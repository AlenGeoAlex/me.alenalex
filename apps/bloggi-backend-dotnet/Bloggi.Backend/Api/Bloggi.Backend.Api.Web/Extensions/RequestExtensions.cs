namespace Bloggi.Backend.Api.Web.Extensions;

public static class RequestExtensions
{
    public static IEnumerable<TEnum> ParseEnumArray<TEnum>(this string[] values)
        where TEnum : struct, Enum =>
        values
            .Select(x => Enum.TryParse<TEnum>(x, true, out var result) ? result : (TEnum?)null)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .ToArray();
    
    public static TEnum[] ParseEnumArray<TEnum>(this IQueryCollection queryCollection, string key)
        where TEnum : struct, Enum =>
        queryCollection[key]
            .Select(x => Enum.TryParse<TEnum>(x, true, out var result) ? result : (TEnum?)null)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .ToArray();
}