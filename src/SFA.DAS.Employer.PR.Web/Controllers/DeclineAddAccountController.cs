using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Common;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Web.Authentication;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Extensions;
using SFA.DAS.Employer.PR.Web.Helpers;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Models;

namespace SFA.DAS.Employer.PR.Web.Controllers;

[Authorize(Policy = nameof(PolicyNames.HasEmployerOwnerAccount))]
[Route("accounts/{employerAccountId}/requests/{requestId}/decline", Name = RouteNames.DeclineAddAccount)]
public sealed class DeclineAddAccountController(IOuterApiClient _outerApiClient) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index([FromRoute] Guid requestId, [FromRoute] string employerAccountId, [FromQuery]bool? acceptAddAccountRequest, CancellationToken cancellationToken)
    {
        var response = await _outerApiClient.GetRequest(requestId, cancellationToken);

        if (!ReviewRequestHelper.IsValidRequest(response, RequestType.AddAccount))
        {
            return RedirectToAction(RouteNames.Requests, new { requestId });
        }

        var model = new DeclineAddAccountRequestViewModel() { 
            ProviderName = response!.ProviderName,
            BackLink = Url.RouteUrl(RouteNames.AddAccounts, new { employerAccountId, requestId, acceptAddAccountRequest })!
        };
        
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Index([FromRoute] Guid requestId, [FromRoute] string employerAccountId, CancellationToken cancellationToken)
    {
        await _outerApiClient.DeclineRequest(
            requestId,
            new Domain.OuterApi.Permissions.DeclineRequestModel(User.GetUserId().ToString()),
            cancellationToken
        );

        TempData[TempDataKeys.RequestDeclinedConfirmation] = requestId.ToString();

        return RedirectToRoute(RouteNames.DeclineAddAccountConfirmation, new { requestId, employerAccountId });
    }
}
