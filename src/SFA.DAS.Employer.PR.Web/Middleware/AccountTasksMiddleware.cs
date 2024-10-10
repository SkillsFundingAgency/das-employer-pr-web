using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Infrastructure.Services;

namespace SFA.DAS.Employer.PR.Web.Middleware;

public class AccountTasksMiddleware(RequestDelegate _next, ISessionService _sessionService)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            if (_sessionService.Contains(SessionKeys.AccountTasksKey))
            {
                context.Items[SessionKeys.AccountTasksKey] = true;
            }
            else
            {
                // Check if referrer URL contains accountTasks=true
                if (context.Request.Query.ContainsKey("AccountTasks"))
                {
                    _sessionService.Set(SessionKeys.AccountTasksKey, true.ToString());
                    context.Items.TryAdd(SessionKeys.AccountTasksKey, true);
                }
            }
        }

        // Call the next delegate/middleware in the pipeline
        await _next(context);
    }
}
