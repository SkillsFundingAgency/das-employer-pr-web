using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Common;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Web.Authentication;
using SFA.DAS.Employer.PR.Web.Extensions;
using SFA.DAS.Employer.PR.Web.Helpers;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Models;

namespace SFA.DAS.Employer.PR.Web.Controllers;

[Authorize(Policy = nameof(PolicyNames.HasEmployerOwnerAccount))]
[Route("accounts/{employerAccountId}/requests/{requestId}/decline/confirmed", Name = RouteNames.DeclineAddAccountConfirmation)]
public sealed class DeclineAddAccountConfirmationController(IOuterApiClient _outerApiClient) : Controller
{
    public async Task<IActionResult> Index([FromRoute] Guid requestId, [FromRoute] string employerAccountId, CancellationToken cancellationToken)
    {
        var requestResponse = await _outerApiClient.GetRequest(requestId, cancellationToken);

        if (!ReviewRequestHelper.IsValidRequest(requestResponse, RequestType.AddAccount))
        {
            return RedirectToRoute(RouteNames.YourTrainingProviders, new { employerAccountId });
        }

        await _outerApiClient.DeclineRequest(
            requestId, 
            new Domain.OuterApi.Permissions.DeclineRequestModel(User.GetUserId().ToString()),
            cancellationToken
        );

        DeclineAddAccountConfirmationViewModel model = new()
        {
            ManageTrainingProvidersUrl = Url.RouteUrl(RouteNames.YourTrainingProviders, new { employerAccountId })!,
            ProviderName = requestResponse!.ProviderName
        };

        return View(model);
    }
}
