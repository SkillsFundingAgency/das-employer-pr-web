using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Extensions;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Infrastructure.Services;
using SFA.DAS.Employer.PR.Web.Models.Requests;
using SFA.DAS.Employer.PR.Web.Models.Session;

namespace SFA.DAS.Employer.PR.Web.Controllers.Requests;

[Authorize]
[Route("request/{requestId:guid}/createaccount/decline", Name = RouteNames.DeclineCreateAccount)]
public class DeclineCreateAccountController(IOuterApiClient _outerApiClient, ISessionService _sessionService) : Controller
{
    [HttpGet]
    public IActionResult Index(Guid requestId, CancellationToken cancellationToken)
    {
        Request.HttpContext.Items.Add(SessionKeys.AccountTasksKey, true);

        var sessionModel = _sessionService.Get<AccountCreationSessionModel>();
        if (string.IsNullOrEmpty(sessionModel?.ProviderName))
        {
            return RedirectToRoute(RouteNames.CreateAccountCheckDetails, new { requestId });
        }

        var backLink = Url.RouteUrl(RouteNames.CreateAccountCheckDetails, new { requestId });

        var vm = new DeclineCreateAccountViewModel
        {
            ProviderName = sessionModel.ProviderName.ToUpper(),
            BackLink = backLink!
        };

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromRoute] Guid requestId, CancellationToken cancellationToken)
    {
        // we need to check if session is timed out here, as the redirect will break if the
        // session has expired, and the check needs to happen before the decline request is updated
        var sessionModel = _sessionService.Get<AccountCreationSessionModel>();
        if (string.IsNullOrEmpty(sessionModel?.ProviderName))
        {
            return RedirectToRoute(RouteNames.CreateAccountCheckDetails, new { requestId });
        }

        await _outerApiClient.DeclineRequest(
            requestId,
            new Domain.OuterApi.Permissions.DeclineRequestModel(User.GetUserId().ToString()),
            cancellationToken
        );

        return RedirectToRoute(RouteNames.DeclineCreateAccountConfirmation, new { requestId });
    }
}
