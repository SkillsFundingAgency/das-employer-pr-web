using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Employer.PR.Web.Infrastructure;

[ExcludeFromCodeCoverage]
public static class RouteNames
{
    public const string SignOut = "sign-out";
    public const string SignedOut = "signed-out";
    public const string AccountUnavailable = "account-unavailable";
    public const string Home = nameof(Home);
    public const string YourTrainingProviders = nameof(YourTrainingProviders);
    public const string SelectLegalEntity = nameof(SelectLegalEntity);
    public const string SelectTrainingProvider = nameof(SelectTrainingProvider);
    public const string GetRegisteredProviders = nameof(GetRegisteredProviders);
    public const string SetPermissions = nameof(SetPermissions);

    public static class StubAccount
    {
        public const string DetailsPost = "account-details-post";
        public const string DetailsGet = "account-details-get";
        public const string SignedIn = "stub-signedin-get";
    }
}
