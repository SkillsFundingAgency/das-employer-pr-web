namespace SFA.DAS.Employer.PR.Web.Infrastructure;

public static class EmployerClaims
{
    public static string AccountsClaimsTypeIdentifier => "http://das/employer/identity/claims/associatedAccounts";
    public static string UserIdClaimTypeIdentifier => "http://das/employer/identity/claims/id";
    public static string UserDisplayNameClaimTypeIdentifier => "http://das/employer/identity/claims/display_name";
    public static string UserEmailClaimTypeIdentifier => "http://das/employer/identity/claims/email_address";
    public const string GivenName = "http://das/employer/identity/claims/given_name";
    public const string FamilyName = "http://das/employer/identity/claims/family_name";
    public static string Account => "http://das/employer/identity/claims/account";
}