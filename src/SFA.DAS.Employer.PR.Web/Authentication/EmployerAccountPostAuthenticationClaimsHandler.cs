using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.GovUK.Auth.Services;

namespace SFA.DAS.Employer.PR.Web.Authentication;

[ExcludeFromCodeCoverage]
public class EmployerAccountPostAuthenticationClaimsHandler : ICustomClaims
{
    private readonly IEmployerAccountsService _employerAccountsService;

    public EmployerAccountPostAuthenticationClaimsHandler(IEmployerAccountsService employerAccountsService)
    {
        _employerAccountsService = employerAccountsService;
    }

    public async Task<IEnumerable<Claim>> GetClaims(TokenValidatedContext tokenValidatedContext)
    {
        var claims = new List<Claim>();
        var userId = tokenValidatedContext.Principal?.Claims
            .First(c => c.Type.Equals(ClaimTypes.NameIdentifier))
            .Value;
        var email = tokenValidatedContext.Principal?.Claims
            .First(c => c.Type.Equals(ClaimTypes.Email))
            .Value;

        var result = await _employerAccountsService.GetEmployerUserAccounts(userId!, email!);

        var accountsAsJson = JsonSerializer.Serialize(result.UserAccounts.ToDictionary(k => k.AccountId));
        var associatedAccountsClaim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, accountsAsJson,
            JsonClaimValueTypes.Json);

        if (result.IsSuspended)
        {
            claims.Add(new Claim(ClaimTypes.AuthorizationDecision, "Suspended"));
        }

        if (!string.IsNullOrEmpty(result.FirstName) && !string.IsNullOrEmpty(result.LastName))
        {
            claims.Add(new Claim(EmployerClaims.GivenName, result.FirstName));
            claims.Add(new Claim(EmployerClaims.FamilyName, result.LastName));
            claims.Add(new Claim(EmployerClaims.UserDisplayNameClaimTypeIdentifier,
                result.FirstName + " " + result.LastName));
        }

        claims.Add(new Claim(EmployerClaims.UserIdClaimTypeIdentifier, result.EmployerUserId));
        claims.Add(new Claim(EmployerClaims.UserEmailClaimTypeIdentifier, email!));

        result.UserAccounts
            .Where(c => c.Role.Equals("owner", StringComparison.CurrentCultureIgnoreCase) ||
                        c.Role.Equals("transactor", StringComparison.CurrentCultureIgnoreCase))
            .ToList().ForEach(u => claims.Add(new Claim(EmployerClaims.Account, u.AccountId)));

        claims.Add(associatedAccountsClaim);

        return claims;
    }
}
