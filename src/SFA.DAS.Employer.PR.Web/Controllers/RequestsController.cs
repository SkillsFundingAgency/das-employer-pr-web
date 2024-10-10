using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Common;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Models.Requests;
using SFA.DAS.Employer.Shared.UI;

namespace SFA.DAS.Employer.PR.Web.Controllers;

[Route("[controller]")]
public class RequestsController(IOuterApiClient _outerApiClient, UrlBuilder _urlBuilder) : Controller
{
    public const string PageNotFoundViewPath = "~/Views/Error/PageNotFound.cshtml";
    public const string InvalidRequestStatusShutterPageViewPath = "~/Views/Requests/InvalidRequestStatusShutterPage.cshtml";
    public const string AccountAlreadyExistsShutterPageViewPath = "~/Views/Requests/AccountAlreadyExistsShutterPage.cshtml";

    [AllowAnonymous]
    [Route("{requestId:guid}")]
    public async Task<IActionResult> ValidateCreateAccountRequest(Guid requestId, CancellationToken cancellationToken)
    {
        var response = await _outerApiClient.ValidateCreateAccountRequest(requestId, cancellationToken);
        if (!response.IsRequestValid) return View(PageNotFoundViewPath);

        InvalidCreateAccountRequestShutterPageViewModel vm = new(_urlBuilder.AccountsLink());

        if (response.Status != RequestStatus.Sent || !response.HasValidaPaye.GetValueOrDefault())
        {
            return View(InvalidRequestStatusShutterPageViewPath, vm);
        }

        if (response.HasEmployerAccount.GetValueOrDefault())
        {
            return View(AccountAlreadyExistsShutterPageViewPath, vm);
        }

        return RedirectToRoute(RouteNames.CreateAccountCheckDetails, new { requestId });
    }

    [Authorize]
    [Route("{requestId:guid}/createaccount", Name = RouteNames.CreateAccountCheckDetails)]
    public IActionResult CreateAccountCheckDetails(Guid requestId, CancellationToken cancellationToken)
    {
        return Ok();
    }
}
