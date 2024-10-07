using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Web.Models.Requests;
using SFA.DAS.Employer.Shared.UI;

namespace SFA.DAS.Employer.PR.Web.Controllers;

[Route("[controller]")]
public class RequestsController(IOuterApiClient _outerApiClient, UrlBuilder _urlBuilder) : Controller
{
    public const string PageNotFoundViewPath = "~/Views/Error/PageNotFound.cshtml";
    public const string InvalidRequestStatusShutterPageViewPath = "~/Views/Requests/InvalidRequestStatusShutterPage.cshtml";

    [AllowAnonymous]
    [Route("{requestId:guid}")]
    public async Task<IActionResult> ValidateCreateAccountRequest(Guid requestId, CancellationToken cancellationToken)
    {
        var response = await _outerApiClient.ValidateCreateAccountRequest(requestId, cancellationToken);
        if (!response.IsRequestValid) return View(PageNotFoundViewPath);

        if (response.Status != RequestStatus.Sent || !response.HasValidaPaye.GetValueOrDefault())
        {
            InvalidRequestStatusShutterPageViewModel vm = new(_urlBuilder.AccountsLink());
            return View(InvalidRequestStatusShutterPageViewPath, vm);
        }

        return Ok();
    }
}
