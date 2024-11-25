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
    public const string AddPermissions = nameof(AddPermissions);
    public const string ChangePermissions = nameof(ChangePermissions);
    public const string Requests = nameof(Requests);
    public const string UpdatePermissions = nameof(UpdatePermissions);
    public const string AddAccount = nameof(AddAccount);
    public const string DeclineAddAccount = nameof(DeclineAddAccount);
    public const string DeclineAddAccountConfirmation = nameof(DeclineAddAccountConfirmation);
    public const string Error = nameof(Error);
    public const string CreateAccountCheckDetails = nameof(CreateAccountCheckDetails);
    public const string CreateAccountChangeName = nameof(CreateAccountChangeName);
    public const string CreateAccountConfirmation = nameof(CreateAccountConfirmation);
    public const string DeclineCreateAccount = nameof(DeclineCreateAccount);
    public const string DeclineCreateAccountConfirmation = nameof(DeclineCreateAccountConfirmation);

    public static class StubAccount
    {
        public const string DetailsPost = "account-details-post";
        public const string DetailsGet = "account-details-get";
        public const string SignedIn = "stub-signedin-get";
    }
}
