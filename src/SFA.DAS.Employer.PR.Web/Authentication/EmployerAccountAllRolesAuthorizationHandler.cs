using Microsoft.AspNetCore.Authorization;

namespace SFA.DAS.Employer.PR.Web.Authentication;

public class EmployerAccountAllRolesAuthorizationHandler(IEmployerAccountAuthorisationHandler _handler) : AuthorizationHandler<EmployerAccountAllRolesRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, EmployerAccountAllRolesRequirement requirement)
    {
        if (!await _handler.IsEmployerAuthorised(context, true))
        {
            return;
        }

        context.Succeed(requirement);
    }
}
