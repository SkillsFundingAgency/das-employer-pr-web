using Microsoft.AspNetCore.Authorization;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;

namespace SFA.DAS.Employer.PR.Web.Authentication;

[ExcludeFromCodeCoverage]
public class EmployerAccountAuthorizationHandler : AuthorizationHandler<EmployerAccountRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IOuterApiClient _outerApiClient;

    public EmployerAccountAuthorizationHandler(IHttpContextAccessor httpContextAccessor, IOuterApiClient outerApiClient)
    {
        _httpContextAccessor = httpContextAccessor;
        _outerApiClient = outerApiClient;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, EmployerAccountRequirement requirement)
    {
        if (!IsEmployerAuthorised(context))
        {
            return Task.CompletedTask;
        }

        context.Succeed(requirement);

        return Task.CompletedTask;
    }

    private bool IsEmployerAuthorised(AuthorizationHandlerContext context)
    {
        if (_httpContextAccessor.HttpContext == null || !_httpContextAccessor.HttpContext!.Request.RouteValues.ContainsKey(RouteValueKeys.EncodedAccountId))
        {
            return false;
        }

        var employerAccountClaim = context.User.FindFirst(c => c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier));
        if (employerAccountClaim?.Value == null)
        {
            return false;
        }


        Dictionary<string, EmployerUserAccountItem> employerAccounts;
        employerAccounts = JsonSerializer.Deserialize<Dictionary<string, EmployerUserAccountItem>>(employerAccountClaim.Value)!;

        string accountIdFromUrl = _httpContextAccessor.HttpContext!.Request.RouteValues[RouteValueKeys.EncodedAccountId]!.ToString()!;

        EmployerUserAccountItem? employerUserAccount = null;

        if (employerAccounts != null)
        {
            employerUserAccount = employerAccounts.TryGetValue(accountIdFromUrl, out EmployerUserAccountItem? value) ? value : null;
        }

        if (employerAccounts == null || !employerAccounts.ContainsKey(accountIdFromUrl))
        {
            var requiredIdClaim = ClaimTypes.NameIdentifier;

            if (!context.User.HasClaim(c => c.Type.Equals(requiredIdClaim)))
            {
                return false;
            }

            var userClaim = context.User.FindFirst(requiredIdClaim)!;
            var userId = userClaim.Value.ToString();

            var email = context.User.FindFirstValue(ClaimTypes.Email);

            var result = _outerApiClient.GetUserAccounts(userId, email!, CancellationToken.None).Result;

            var accountsAsJson = JsonSerializer.Serialize(result.UserAccounts.ToDictionary(k => k.EncodedAccountId));

            var associatedAccountsClaim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, accountsAsJson, JsonClaimValueTypes.Json);

            var updatedEmployerAccounts = JsonSerializer.Deserialize<Dictionary<string, EmployerUserAccountItem>>(associatedAccountsClaim.Value);

            userClaim.Subject!.AddClaim(associatedAccountsClaim);

            if (!updatedEmployerAccounts!.TryGetValue(accountIdFromUrl, out EmployerUserAccountItem? value))
            {
                return false;
            }

            employerUserAccount = value;
        }

        if (!_httpContextAccessor.HttpContext.Items.ContainsKey("Employer"))
        {
            _httpContextAccessor.HttpContext.Items.Add("Employer", employerAccounts!.GetValueOrDefault(accountIdFromUrl));
        }

        if (!CheckUserRoleForAccess(employerUserAccount!))
        {
            return false;
        }

        return true;
    }

    private static bool CheckUserRoleForAccess(EmployerUserAccountItem employerIdentifier)
    {
        if (!Enum.TryParse<EmployerUserRole>(employerIdentifier.Role, true, out _))
        {
            return false;
        }

        return true;
    }
}
