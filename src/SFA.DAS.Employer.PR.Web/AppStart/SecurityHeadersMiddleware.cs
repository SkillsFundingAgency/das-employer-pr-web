using System.Diagnostics.CodeAnalysis;
using SFA.DAS.Employer.PR.Web.Extensions;

namespace SFA.DAS.Employer.PR.Web.AppStart;

[ExcludeFromCodeCoverage]
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers.ReplaceHeader("x-frame-options", "DENY");
        context.Response.Headers.ReplaceHeader("x-content-type-options", "nosniff");
        context.Response.Headers.ReplaceHeader("X-Permitted-Cross-Domain-Policies", "none");
        context.Response.Headers.ReplaceHeader("x-xss-protection", "0");

        await _next(context);
    }
}
