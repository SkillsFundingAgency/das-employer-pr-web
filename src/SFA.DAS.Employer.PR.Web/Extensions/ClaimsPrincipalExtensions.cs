using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Json;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Web.Authentication;

namespace SFA.DAS.Employer.PR.Web.Extensions;

[ExcludeFromCodeCoverage]
public static class ClaimsPrincipalExtensions
{
    private static Guid GetClaimValue(ClaimsPrincipal principal, string claimType)
    {
        var memberId = principal.FindFirstValue(claimType);
        var hasParsed = Guid.TryParse(memberId, out Guid value);
        return hasParsed ? value : Guid.Empty;
    }

    public static Dictionary<string, EmployerIdentifier> GetEmployerAccounts(this ClaimsPrincipal user)
    => JsonSerializer.Deserialize<Dictionary<string, EmployerIdentifier>>(user.FindFirstValue(EmployerClaims.AccountsClaimsTypeIdentifier)!)!;

    public static EmployerIdentifier GetEmployerAccount(this ClaimsPrincipal user, string employerAccountId)
        => GetEmployerAccounts(user)[employerAccountId.ToUpper()];

    public static Guid GetUserId(this ClaimsPrincipal principal) => GetClaimValue(principal, EmployerClaims.UserIdClaimTypeIdentifier);
    public static string GetUserDisplayName(this ClaimsPrincipal principal) => principal.FindFirstValue(EmployerClaims.UserDisplayNameClaimTypeIdentifier)!;
    public static string GetGivenName(this ClaimsPrincipal principal) => principal.FindFirstValue(EmployerClaims.GivenName)!;
    public static string GetFamilyName(this ClaimsPrincipal principal) => principal.FindFirstValue(EmployerClaims.FamilyName)!;
    public static string GetEmail(this ClaimsPrincipal principal) => principal.FindFirstValue(EmployerClaims.UserEmailClaimTypeIdentifier)!;
}
