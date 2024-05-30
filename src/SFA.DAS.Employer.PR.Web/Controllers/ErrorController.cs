using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Models;

namespace SFA.DAS.Employer.PR.Web.Controllers;

[Route("[controller]")]
public class ErrorController(ILogger<ErrorController> _logger) : Controller
{
    [AllowAnonymous]
    [Route("{statusCode}")]
    public IActionResult HttpStatusCodeHandler(int statusCode)
    {
        switch (statusCode)
        {
            case 403:
            case 404:
                return View("PageNotFound");
            default:
                var feature = HttpContext!.Features!.Get<IExceptionHandlerPathFeature>();
                ErrorViewModel errorViewModel = new(Url.RouteUrl(RouteNames.Home, new { employerAccountId = feature!.RouteValues!["employerAccountId"]!.ToString()! })!);
                return View("ErrorInService", errorViewModel);
        }
    }

    [AllowAnonymous]
    public IActionResult ErrorInService()
    {
        var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

        ErrorViewModel errorViewModel = new(Url.RouteUrl(RouteNames.Home, new { employerAccountId = feature!.RouteValues!["employerAccountId"]!.ToString()! })!);

        if (User.Identity!.IsAuthenticated)
        {
            _logger.LogError(feature!.Error, "Unexpected error occurred during request to path: {Path} by user: {User}", feature.Path, User.FindFirstValue(ClaimTypes.NameIdentifier));
        }
        else
        {
            _logger.LogError(feature!.Error, "Unexpected error occurred during request to {Path}", feature.Path);
        }
        return View(errorViewModel);
    }
}
