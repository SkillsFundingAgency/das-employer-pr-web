using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Employer.PR.Web.Infrastructure;

[ExcludeFromCodeCoverage]
public static class ViewNames
{
    public const string CannotViewRequest = nameof(CannotViewRequest);
    public const string ReviewPermissionsRequest = nameof(ReviewPermissionsRequest);
    public const string ReviewAddAccountsRequest = nameof(ReviewAddAccountsRequest);
    public const string DeclineAddAccountRequestShutter = nameof(DeclineAddAccountRequestShutter);

    public const string PageNotFound = nameof(PageNotFound);
    public const string ErrorInService = nameof(ErrorInService);
}
