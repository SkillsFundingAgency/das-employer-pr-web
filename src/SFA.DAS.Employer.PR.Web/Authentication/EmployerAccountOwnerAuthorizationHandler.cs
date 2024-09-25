using Microsoft.AspNetCore.Authorization;

namespace SFA.DAS.Employer.PR.Web.Authentication;

public class EmployerAccountOwnerAuthorizationHandler(IEmployerAccountAuthorisationHandler _handler) : AuthorizationHandler<EmployerAccountOwnerRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, EmployerAccountOwnerRequirement ownerRequirement)
    {
        if (!(await _handler.IsEmployerAuthorised(context, false)))
        {
            return;
        }

        context.Succeed(ownerRequirement);
    }
}
