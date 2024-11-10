using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Common;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.OuterApi.Requests;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Extensions;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Infrastructure.Services;
using SFA.DAS.Employer.PR.Web.Models.Requests;
using SFA.DAS.Employer.PR.Web.Models.Session;
using SFA.DAS.Employer.PR.Web.Services;
using SFA.DAS.Encoding;

namespace SFA.DAS.Employer.PR.Web.Controllers.Requests;

[Route("[controller]")]
public class RequestsController(IOuterApiClient _outerApiClient, ISessionService _sessionService, IValidator<EmployerAccountCreationSubmitModel> _validator, IAccountsLinkService _accountsLinkService, IEncodingService _encodingService) : Controller
{
    public const string PageNotFoundViewPath = "~/Views/Error/PageNotFound.cshtml";
    public const string InvalidRequestStatusShutterPageViewPath = "~/Views/Requests/InvalidRequestStatusShutterPage.cshtml";
    public const string AccountAlreadyExistsShutterPageViewPath = "~/Views/Requests/AccountAlreadyExistsShutterPage.cshtml";
    public const string UserEmailDoesNotMatchRequestShutterPageViewPath = "~/Views/Requests/UserEmailDoesNotMatchRequestShutterPage.cshtml";
    public const string RequestsCheckDetailsViewPath = "~/Views/Requests/CreateServiceAccountCheckDetails.cshtml";
    public const string RequestsChangeNameViewPath = "~/Views/Requests/CreateServiceAccountChangeName.cshtml";
    public const string AccountCreatedConfirmationViewPath = "~/Views/Requests/CreateServiceAccountConfirmation.cshtml";

    [AllowAnonymous]
    [HttpGet]
    [Route("{requestId:guid}")]
    public async Task<IActionResult> ValidateCreateAccountRequest(Guid requestId, CancellationToken cancellationToken)
    {
        Request.HttpContext.Items.Add(SessionKeys.AccountTasksKey, true);
        var shutterPage = await GetShutterPageIfRequestIsInvalid(requestId, cancellationToken);
        return shutterPage != null ? shutterPage : RedirectToRoute(RouteNames.CreateAccountCheckDetails, new { requestId });
    }

    [Authorize]
    [HttpGet]
    [Route("{requestId:guid}/createAccount", Name = RouteNames.CreateAccountCheckDetails)]
    public async Task<IActionResult> GetRequestDetails(Guid requestId, CancellationToken cancellationToken)
    {
        Request.HttpContext.Items.Add(SessionKeys.AccountTasksKey, true);
        var shutterPage = await GetShutterPageIfRequestIsInvalid(requestId, cancellationToken);
        if (shutterPage != null) return shutterPage;

        GetPermissionRequestResponse permissionRequest = await _outerApiClient.GetPermissionRequest(requestId, cancellationToken);

        if (User.GetEmail() != permissionRequest.EmployerContactEmail) return View(UserEmailDoesNotMatchRequestShutterPageViewPath);

        var sessionModel = _sessionService.Get<AccountCreationSessionModel>();
        if (sessionModel == null)
        {
            sessionModel = new AccountCreationSessionModel
            {
                FirstName = permissionRequest.EmployerContactFirstName!,
                LastName = permissionRequest.EmployerContactLastName,
                ProviderName = permissionRequest.ProviderName
            };
            _sessionService.Set(sessionModel);
        }

        GetNamesFromSessionModel(sessionModel, permissionRequest);

        var changeNameLink = Url.RouteUrl(RouteNames.CreateAccountChangeName, new { requestId });
        var declineCreateAccountLink = Url.RouteUrl(RouteNames.DeclineCreateAccount, new { requestId });

        EmployerAccountCreationViewModel vm = GetViewModel(permissionRequest, changeNameLink!, declineCreateAccountLink!);
        return View(RequestsCheckDetailsViewPath, vm);
    }

    [Authorize]
    [HttpPost]
    [Route("{requestId:guid}/createAccount", Name = RouteNames.CreateAccountCheckDetails)]
    public async Task<IActionResult> PostRequestDetails([FromRoute] Guid requestId, EmployerAccountCreationSubmitModel submitModel, CancellationToken cancellationToken)
    {
        Request.HttpContext.Items.Add(SessionKeys.AccountTasksKey, true);
        var shutterPage = await GetShutterPageIfRequestIsInvalid(requestId, cancellationToken);
        if (shutterPage != null) return shutterPage;

        var sessionModel = _sessionService.Get<AccountCreationSessionModel>();
        if (sessionModel == null) return RedirectToRoute(RouteNames.CreateAccountCheckDetails, new { requestId });

        var result = _validator.Validate(submitModel);
        if (!result.IsValid)
        {
            GetPermissionRequestResponse permissionRequest = await _outerApiClient.GetPermissionRequest(requestId, cancellationToken);
            GetNamesFromSessionModel(sessionModel, permissionRequest);

            var changeNameLink = Url.RouteUrl(RouteNames.CreateAccountChangeName, new { requestId });
            var declineCreateAccountLink = Url.RouteUrl(RouteNames.DeclineCreateAccount, new { requestId });

            EmployerAccountCreationViewModel viewModel = GetViewModel(permissionRequest, changeNameLink!, declineCreateAccountLink!);
            result.AddToModelState(ModelState);
            return View(RequestsCheckDetailsViewPath, viewModel);
        }

        AcceptCreateAccountRequest body = new(sessionModel.FirstName!, sessionModel.LastName!, User.GetEmail(), User.GetUserId());
        AcceptCreateAccountResponse response = await _outerApiClient.AcceptCreateAccountRequest(requestId, body, cancellationToken);
        sessionModel.AccountId = response.AccountId;
        _sessionService.Set(sessionModel);

        return RedirectToRoute(RouteNames.CreateAccountConfirmation);
    }

    [Authorize]
    [HttpGet]
    [Route("/createAccount/confirmation", Name = RouteNames.CreateAccountConfirmation)]
    public IActionResult AccountCreatedConfirmation()
    {
        var sessionModel = _sessionService.Get<AccountCreationSessionModel>();
        if (sessionModel == null) throw new InvalidOperationException();

        var employerAccountId = _encodingService.Encode(sessionModel.AccountId, EncodingType.AccountId);

        var model = new AccountCreatedConfirmationViewModel(_accountsLinkService.GetAccountsHomeLink(), sessionModel.ProviderName!, _accountsLinkService.GetAccountsLink(EmployerAccountRoutes.AccountsAgreements, employerAccountId), Url.RouteUrl(RouteNames.YourTrainingProviders, new { employerAccountId })!);

        Request.HttpContext.Items.Remove(SessionKeys.AccountTasksKey);

        _sessionService.Delete<AccountCreationSessionModel>();
        return View(AccountCreatedConfirmationViewPath, model);
    }

    private static void GetNamesFromSessionModel(AccountCreationSessionModel? sessionModel, GetPermissionRequestResponse permissionRequest)
    {
        if (sessionModel != null)
        {
            permissionRequest.EmployerContactFirstName = sessionModel.FirstName;
            permissionRequest.EmployerContactLastName = sessionModel.LastName;
        }
    }

    private async Task<ViewResult?> GetShutterPageIfRequestIsInvalid(Guid requestId, CancellationToken cancellationToken)
    {
        ValidateCreateAccountRequestResponse response = await _outerApiClient.ValidateCreateAccountRequest(requestId, cancellationToken);

        if (!response.IsRequestValid) return View(PageNotFoundViewPath);

        InvalidCreateAccountRequestShutterPageViewModel vm = new(_accountsLinkService.GetAccountsHomeLink());

        if (response.Status != RequestStatus.Sent || !response.HasValidPaye.GetValueOrDefault())
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
