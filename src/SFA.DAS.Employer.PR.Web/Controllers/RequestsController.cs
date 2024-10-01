using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using static SFA.DAS.Employer.PR.Web.Infrastructure.RouteNames;

namespace SFA.DAS.Employer.PR.Web.Controllers;

[Route("requests/{requestId}", Name = RouteNames.Requests)]

public class RequestsController(IOuterApiClient _outerApiClient) : Controller
{
    [AllowAnonymous]
    public async Task<IActionResult> Index([FromRoute] Guid requestId, CancellationToken cancellationToken)
    {
        GetRequestResponse? requestResponse = await _outerApiClient.GetRequest(requestId, cancellationToken);

        if (requestResponse is null)
        {
            return View(RequestViews.CannotViewRequest);
        }
        else
        {
            if(requestResponse.ValidateRequest())
            {
                // Happy Path: To Do: CSP-1499
                return RedirectToAction();
            }
            else
            {
                return View(RequestViews.CannotViewRequest);
            }
        }
    }
}
