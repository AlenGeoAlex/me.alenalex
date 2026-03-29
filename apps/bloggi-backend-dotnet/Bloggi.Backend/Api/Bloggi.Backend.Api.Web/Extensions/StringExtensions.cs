namespace Bloggi.Backend.Api.Web.Extensions;

using System.Globalization;
using System.Text;

public static class StringExtensions
{
    public static string Slugify(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return value
            .ToLowerInvariant()
            .Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c)
                        != UnicodeCategory.NonSpacingMark)
            .Aggregate(new StringBuilder(), (sb, c) => sb.Append(c))
            .ToString()
            .Normalize(NormalizationForm.FormC)
            .Replace("'", "")
            .Replace("&", "and")
            .ToCharArray()
            .Select(c => char.IsLetterOrDigit(c) ? c : '-')
            .Aggregate(new StringBuilder(), (sb, c) => sb.Append(c))
            .ToString()
            .Split('-', StringSplitOptions.RemoveEmptyEntries)
            .Aggregate((a, b) => $"{a}-{b}")
            .Trim('-');
    }
}