using SFA.DAS.Employer.PR.Web.Infrastructure;

namespace SFA.DAS.Employer.PR.Web.Middleware;

public class AccountTasksMiddleware
{
    private readonly RequestDelegate _next;

    public AccountTasksMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            if (context.Session.TryGetValue(SessionKeys.AccountTasksKey, out var val))
            {
                context.Items[SessionKeys.AccountTasksKey] = true;
            }
            else
            {
                // Check if referrer URL contains accountTasks=true
                if (context.Request.Query.ContainsKey("AccountTasks"))
                {
                    context.Session.SetString(SessionKeys.AccountTasksKey, "true");
                    context.Items.TryAdd(SessionKeys.AccountTasksKey, true);
                }
            }
        }

        // Call the next delegate/middleware in the pipeline
        await _next(context);
    }
}
