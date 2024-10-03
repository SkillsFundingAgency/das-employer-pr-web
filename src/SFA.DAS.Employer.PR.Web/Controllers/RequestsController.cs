using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Common;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Infrastructure;

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
        return response.Status == RequestStatus.Sent || response.Status == RequestStatus.New;
    }
}
