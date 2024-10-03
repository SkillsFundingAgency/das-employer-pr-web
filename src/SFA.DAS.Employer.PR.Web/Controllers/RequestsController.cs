using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using static SFA.DAS.Employer.PR.Domain.Common.PermissionRequest;
using static SFA.DAS.Employer.PR.Web.Infrastructure.RouteNames;

namespace SFA.DAS.Employer.PR.Web.Controllers;

[Route("requests/{requestId}", Name = RouteNames.Requests)]

public class RequestsController(IOuterApiClient _outerApiClient) : Controller
{
    [AllowAnonymous]
    public async Task<IActionResult> Index([FromRoute] Guid requestId, CancellationToken cancellationToken)
    {
        GetPermissionRequestResponse? response = await _outerApiClient.GetRequest(requestId, cancellationToken);

        if (response is null)
        {
            return View(ViewNames.CannotViewRequest);
        }
        else
        {
            if(ValidatePermissionRequest(response))
            {
                // Happy Path: To Do: CSP-1499
                return RedirectToAction();
            }
            else
            {
                return View(ViewNames.CannotViewRequest);
            }
        }
    }

    public static bool ValidatePermissionRequest(GetPermissionRequestResponse response)
    {
        return response.Status == nameof(RequestStatus.Sent) || response.Status == nameof(RequestStatus.New);
    }

    [AllowAnonymous]
    [Route("review")]
    public IActionResult ReviewPermissionsRequest([FromRoute] Guid requestId, CancellationToken cancellationToken)
    {
        return View(RequestViews.ReviewPermissionsRequest);
    }
}
