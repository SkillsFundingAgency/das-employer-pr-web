using Microsoft.AspNetCore.Authorization;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Constants;
using System.Text.Json;

namespace SFA.DAS.Employer.PR.Web.Authentication;

public class EmployerOwnerAuthorizationHandler : AuthorizationHandler<EmployerOwnerRoleRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EmployerOwnerAuthorizationHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, EmployerOwnerRoleRequirement requirement)
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
        if (!_httpContextAccessor.HttpContext!.Request.RouteValues.ContainsKey(RouteValueKeys.EncodedAccountId))
        {
            return false;
        }

        var accountIdFromUrl = _httpContextAccessor.HttpContext.Request.RouteValues[RouteValueKeys.EncodedAccountId]!.ToString()!;
        var employerAccountClaim = context.User.FindFirst(c => c.Type.Equals(EmployerClaims.AccountsClaimsTypeIdentifier));

        if (employerAccountClaim?.Value == null)
        {
            return false;
        }

        Dictionary<string, EmployerUserAccountItem> employerAccounts;
        EmployerUserAccountItem? employerAccount = null;

        employerAccounts = JsonSerializer.Deserialize<Dictionary<string, EmployerUserAccountItem>>(employerAccountClaim.Value)!;

        employerAccount = employerAccounts.TryGetValue(accountIdFromUrl, out EmployerUserAccountItem? value) ? value : null;

        if (employerAccount == null)
        {
            return false;
        }

        if (!CheckUserRoleForAccessAndOwner(employerAccount))
        {
            return false;
        }

        return true;
    }

    private static bool CheckUserRoleForAccessAndOwner(EmployerUserAccountItem employerIdentifier)
    {
        if (!Enum.TryParse<EmployerUserRole>(employerIdentifier.Role, true, out var userRole))
        {
            return false;
        }

        return userRole == EmployerUserRole.Owner;
    }
}

