using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Extensions;
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
    public const string UserEmailDoesNotMatchRequestShutterPageViewPath = "~/Views/Requests/UserEmailDoesNotMatchRequestShutterPage.cshtml";
    public const string RequestsCheckDetailsViewPath = "~/Views/Requests/CheckDetails.cshtml";

    [AllowAnonymous]
    [HttpGet]
    [Route("{requestId:guid}")]
    public async Task<IActionResult> ValidateCreateAccountRequest(Guid requestId, CancellationToken cancellationToken)
    {
        ValidateCreateAccountRequestResponse response = await _outerApiClient.ValidateCreateAccountRequest(requestId, cancellationToken);
        var result = GetShutterPageIfInvalid(response);

        return result != null ? result : RedirectToRoute(RouteNames.CreateAccountCheckDetails, new { requestId });
    }

    [Authorize]
    [HttpGet]
    [Route("{requestId:guid}/createaccount", Name = RouteNames.CreateAccountCheckDetails)]
    public async Task<IActionResult> GetRequestDetails(Guid requestId, CancellationToken cancellationToken)
    {
        Request.HttpContext.Items.Add(SessionKeys.AccountTasksKey, true);
        ValidateCreateAccountRequestResponse response = await _outerApiClient.ValidateCreateAccountRequest(requestId, cancellationToken);
        var result = GetShutterPageIfInvalid(response);

        if (result != null) return result;

        GetPermissionRequestResponse permissionRequest = await _outerApiClient.GetPermissionRequest(requestId, cancellationToken);

        if (User.GetEmail() != permissionRequest.EmployerContactEmail) return View(UserEmailDoesNotMatchRequestShutterPageViewPath);

        return View(RequestsCheckDetailsViewPath);
    }

    private ViewResult? GetShutterPageIfInvalid(ValidateCreateAccountRequestResponse response)
    {
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
        return null;
    }
}
