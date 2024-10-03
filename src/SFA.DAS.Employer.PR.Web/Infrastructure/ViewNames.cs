using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Employer.PR.Web.Infrastructure;

[ExcludeFromCodeCoverage]
public static class ViewNames
{
    public const string CannotViewRequest = nameof(CannotViewRequest);

    public const string PageNotFound = nameof(PageNotFound);
    public const string ErrorInService = nameof(ErrorInService);
}
