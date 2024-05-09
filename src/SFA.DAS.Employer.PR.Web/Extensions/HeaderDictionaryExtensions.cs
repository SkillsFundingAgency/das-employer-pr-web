using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Employer.PR.Web.Extensions;

[ExcludeFromCodeCoverage]
public static class HeaderDictionaryExtensions
{
    public static void ReplaceHeader(this IHeaderDictionary headers, string key, string values)
    {
        if (headers.ContainsKey(key)) headers.Remove(key);

        headers.Append(key, values);
    }
}
