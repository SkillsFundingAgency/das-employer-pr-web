using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Extensions;

namespace SFA.DAS.Employer.PR.Web.Authentication;

public interface IEmployerAccountAuthorisationHandler
{
    Task<bool> IsEmployerAuthorised(AuthorizationHandlerContext context, bool allowAllUserRoles);
    bool CheckUserAccountAccess(ClaimsPrincipal user, EmployerUserRole userRoleRequired);
}

[ExcludeFromCodeCoverage]
public class EmployerAccountAuthorisationHandler(IHttpContextAccessor _httpContextAccessor, IOuterApiClient _outerApiClient, ILogger<EmployerAccountAuthorisationHandler> _logger) : IEmployerAccountAuthorisationHandler
{
    public async Task<bool> IsEmployerAuthorised(AuthorizationHandlerContext context, bool allowAllUserRoles)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        // If the user is redirected to a controller action from another site (very likely) and this is method is executed, the claims will be empty until the middleware has
        // re-authenticated the user. Once authentication is confirmed this method will be executed again with the claims populated and will run properly.
        if (user == null || user.ClaimsAreEmpty())
        {
            return false;
        }

        if (!_httpContextAccessor.HttpContext!.Request.RouteValues.ContainsKey(RouteValueKeys.EncodedAccountId))
        {
            return false;
        }
        var accountIdFromUrl = _httpContextAccessor.HttpContext!.Request.RouteValues[RouteValueKeys.EncodedAccountId]!.ToString()!.ToUpper();
        var employerAccountClaim = context.User.FindFirst(c => c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier));

        if (employerAccountClaim?.Value == null)
            return false;

        Dictionary<string, EmployerUserAccountItem> employerAccounts;

        try
        {
            employerAccounts = JsonSerializer.Deserialize<Dictionary<string, EmployerUserAccountItem>>(employerAccountClaim.Value)!;
        }
        catch (JsonException e)
        {
            _logger.LogError(e, "Could not deserialize employer account claim for user");
            return false;
        }

        EmployerUserAccountItem? employerIdentifier = null;

        if (employerAccounts != null)
        {
            employerIdentifier = employerAccounts.TryGetValue(accountIdFromUrl, out var account)
                ? account : null;
        }

        if (employerAccounts == null || !employerAccounts.ContainsKey(accountIdFromUrl))
        {
            var requiredIdClaim = ClaimTypes.NameIdentifier;

            if (!context.User.HasClaim(c => c.Type.Equals(requiredIdClaim)))
                return false;

            var userClaim = context.User.Claims
                .First(c => c.Type.Equals(requiredIdClaim));

            var email = context.User.Claims.FirstOrDefault(c => c.Type.Equals(ClaimTypes.Email))?.Value;

            var userId = userClaim.Value;

            var result = await _outerApiClient.GetUserAccounts(userId, email!, CancellationToken.None);

            var accountsAsJson = JsonSerializer.Serialize(result.UserAccounts.ToDictionary(k => k.EncodedAccountId));
            var associatedAccountsClaim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, accountsAsJson, JsonClaimValueTypes.Json);

            var updatedEmployerAccounts = JsonSerializer.Deserialize<Dictionary<string, EmployerUserAccountItem>>(associatedAccountsClaim.Value);

            userClaim.Subject!.AddClaim(associatedAccountsClaim);

            if (!updatedEmployerAccounts!.ContainsKey(accountIdFromUrl))
            {
                return false;
            }
            employerIdentifier = updatedEmployerAccounts[accountIdFromUrl];
        }

        if (!_httpContextAccessor.HttpContext.Items.ContainsKey("Employer"))
        {
            _httpContextAccessor.HttpContext.Items.Add("Employer", employerAccounts!.GetValueOrDefault(accountIdFromUrl));
        }

        if (!CheckUserRoleForAccess(employerIdentifier!, allowAllUserRoles))
        {
            return false;
        }

        return true;
    }

    public bool CheckUserAccountAccess(ClaimsPrincipal user, EmployerUserRole userRoleRequired)
    {
        if (!_httpContextAccessor.HttpContext!.Request.RouteValues.ContainsKey(RouteValueKeys.EncodedAccountId))
        {
            return false;
        }

        Dictionary<string, EmployerUserAccountItem> employerAccounts;
        var accountIdFromUrl = _httpContextAccessor.HttpContext!.Request.RouteValues[RouteValueKeys.EncodedAccountId]!.ToString()!.ToUpper();
        Claim employerAccountClaim = user.FindFirst(c => c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier))!;
        try
        {
            employerAccounts = JsonSerializer.Deserialize<Dictionary<string, EmployerUserAccountItem>>(employerAccountClaim.Value)!;
        }
        catch (JsonException e)
        {
            _logger.LogError(e, "Could not deserialize employer account claim for user");
            return false;
        }

        if (employerAccounts == null)
        {
            return false;
        }

        var employerIdentifier = employerAccounts.ContainsKey(accountIdFromUrl)
                ? employerAccounts[accountIdFromUrl] : null;

        if (employerIdentifier == null)
        {
            return false;
        }

        if (!Enum.TryParse<EmployerUserRole>(employerIdentifier.Role, true, out var claimUserRole))
        {
            return false;
        }

        switch (userRoleRequired)
        {
            case EmployerUserRole.Owner when claimUserRole == EmployerUserRole.Owner:
            case EmployerUserRole.Transactor when claimUserRole is EmployerUserRole.Owner or EmployerUserRole.Transactor:
            case EmployerUserRole.Viewer when claimUserRole is EmployerUserRole.Owner or EmployerUserRole.Transactor or EmployerUserRole.Viewer:
                return true;
            default:
                return false;
        }
    }

    private static bool CheckUserRoleForAccess(EmployerUserAccountItem employerIdentifier, bool allowAllUserRoles)
    {
        if (!Enum.TryParse<EmployerUserRole>(employerIdentifier.Role, true, out var userRole))
        {
            return false;
        }

        if (userRole == EmployerUserRole.None)
        {
            return false;
        }

        return allowAllUserRoles || userRole == EmployerUserRole.Owner;
    }
}
