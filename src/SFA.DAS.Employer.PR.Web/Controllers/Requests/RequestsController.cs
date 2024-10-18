using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Common;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Extensions;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Infrastructure.Services;
using SFA.DAS.Employer.PR.Web.Models.Requests;
using SFA.DAS.Employer.PR.Web.Models.Session;
using SFA.DAS.Employer.Shared.UI;

namespace SFA.DAS.Employer.PR.Web.Controllers.Requests;

[Route("[controller]")]
public class RequestsController(IOuterApiClient _outerApiClient, UrlBuilder _urlBuilder, ISessionService _sessionService, IValidator<EmployerAccountCreationSubmitModel> _validator) : Controller
{
    public const string PageNotFoundViewPath = "~/Views/Error/PageNotFound.cshtml";
    public const string InvalidRequestStatusShutterPageViewPath = "~/Views/Requests/InvalidRequestStatusShutterPage.cshtml";
    public const string AccountAlreadyExistsShutterPageViewPath = "~/Views/Requests/AccountAlreadyExistsShutterPage.cshtml";
    public const string UserEmailDoesNotMatchRequestShutterPageViewPath = "~/Views/Requests/UserEmailDoesNotMatchRequestShutterPage.cshtml";
    public const string RequestsCheckDetailsViewPath = "~/Views/Requests/CreateServiceAccountCheckDetails.cshtml";

    [AllowAnonymous]
    [HttpGet]
    [Route("{requestId:guid}")]
    public async Task<IActionResult> ValidateCreateAccountRequest(Guid requestId, CancellationToken cancellationToken)
    {
        Request.HttpContext.Items.Add(SessionKeys.AccountTasksKey, true);
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

        var sessionModel = _sessionService.Get<AccountCreationSessionModel>();
        if (sessionModel == null)
        {
            sessionModel = new AccountCreationSessionModel
            {
                FirstName = permissionRequest.EmployerContactFirstName,
                LastName = permissionRequest.EmployerContactLastName
            };
            _sessionService.Set(sessionModel);
        }

        SetNamesInSessionModel(sessionModel, permissionRequest);

        var changeNameLink = Url.RouteUrl(RouteNames.CreateAccountChangeName, new { requestId });
        var declineCreateAccountLink = Url.RouteUrl(RouteNames.DeclineCreateAccount, new { requestId });

        EmployerAccountCreationViewModel vm = GetViewModel(permissionRequest, changeNameLink!, declineCreateAccountLink!);
        return View(RequestsCheckDetailsViewPath, vm);
    }

    [Authorize]
    [HttpPost]
    [Route("{requestId:guid}/createaccount", Name = RouteNames.CreateAccountCheckDetails)]
    public async Task<IActionResult> PostRequestDetails([FromRoute] Guid requestId, EmployerAccountCreationSubmitModel submitModel, CancellationToken cancellationToken)
    {
        Request.HttpContext.Items.Add(SessionKeys.AccountTasksKey, true);
        var result = _validator.Validate(submitModel);
        if (!result.IsValid)
        {
            GetPermissionRequestResponse permissionRequest = await _outerApiClient.GetPermissionRequest(requestId, cancellationToken);
            var sessionModel = _sessionService.Get<AccountCreationSessionModel>();
            SetNamesInSessionModel(sessionModel, permissionRequest);


            var changeNameLink = Url.RouteUrl(RouteNames.CreateAccountChangeName, new { requestId });
            var declineCreateAccountLink = Url.RouteUrl(RouteNames.DeclineCreateAccount, new { requestId });

            EmployerAccountCreationViewModel viewModel = GetViewModel(permissionRequest, changeNameLink!, declineCreateAccountLink!);
            result.AddToModelState(ModelState);
            return View(RequestsCheckDetailsViewPath, viewModel);
        }

        _sessionService.Delete<AccountCreationSessionModel>();

        return RedirectToRoute(RouteNames.CreateAccountCheckDetails, new { requestId, submitModel.HasAcceptedTerms });
    }

    private static void SetNamesInSessionModel(AccountCreationSessionModel? sessionModel, GetPermissionRequestResponse? permissionRequest)
    {
        if (sessionModel != null && permissionRequest != null)
        {
            permissionRequest.EmployerContactFirstName = sessionModel.FirstName;
            permissionRequest.EmployerContactLastName = sessionModel.LastName;
        }
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

    private static EmployerAccountCreationViewModel GetViewModel(GetPermissionRequestResponse permissionRequest, string changeNameLink, string declineCreateAccountLink)
    {
        return new EmployerAccountCreationViewModel
        {
            RequestId = permissionRequest.RequestId,
            Ukprn = permissionRequest.Ukprn,
            ProviderName = permissionRequest.ProviderName!.ToUpper(),
            EmployerOrganisationName = permissionRequest.EmployerOrganisationName!.ToUpper(),
            EmployerContactFirstName = permissionRequest.EmployerContactFirstName,
            EmployerContactLastName = permissionRequest.EmployerContactLastName,
            EmployerPAYE = permissionRequest.EmployerPAYE,
            HasAcceptedTerms = false,
            EmployerAORN = permissionRequest.EmployerAORN,
            Operations = permissionRequest.Operations,
            ChangeNameLink = changeNameLink,
            DeclineCreateAccountLink = declineCreateAccountLink
        };
    }
}
