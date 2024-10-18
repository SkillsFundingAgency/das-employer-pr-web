using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Extensions;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Infrastructure.Services;
using SFA.DAS.Employer.PR.Web.Models.Requests;
using SFA.DAS.Employer.PR.Web.Models.Session;

namespace SFA.DAS.Employer.PR.Web.Controllers.Requests;

[Route("requests")]
public class ChangeNameController(IOuterApiClient _outerApiClient, ISessionService _sessionService, IValidator<EmployerAccountNamesSubmitModel> _validator) : Controller
{
    public const string UserEmailDoesNotMatchRequestShutterPageViewPath = "~/Views/Requests/UserEmailDoesNotMatchRequestShutterPage.cshtml";
    public const string RequestsChangeNameViewPath = "~/Views/Requests/CreateServiceAccountChangeName.cshtml";

    [Authorize]
    [HttpGet]
    [Route("{requestId:guid}/createaccount/changename", Name = RouteNames.CreateAccountChangeName)]
    public async Task<IActionResult> Index(Guid requestId, CancellationToken cancellationToken)
    {
        GetPermissionRequestResponse permissionRequest = await _outerApiClient.GetPermissionRequest(requestId, cancellationToken);

        if (User.GetEmail() != permissionRequest.EmployerContactEmail) return View(UserEmailDoesNotMatchRequestShutterPageViewPath);

        var sessionModel = _sessionService.Get<AccountCreationSessionModel>();
        SetNamesInSessionModel(sessionModel, permissionRequest);

        EmployerUserNamesViewModel vm = GetChangeNameViewModel(permissionRequest.EmployerContactFirstName!, permissionRequest.EmployerContactLastName!);

        return View(RequestsChangeNameViewPath, vm);
    }

    [Authorize]
    [HttpPost]
    [Route("{requestId:guid}/createaccount/changename", Name = RouteNames.CreateAccountChangeName)]
    public IActionResult Index(Guid requestId, EmployerAccountNamesSubmitModel submitModel, CancellationToken cancellationToken)
    {
        var result = _validator.Validate(submitModel);
        if (!result.IsValid)
        {
            EmployerUserNamesViewModel viewModel = GetChangeNameViewModel(submitModel.EmployerContactFirstName!,
                submitModel.EmployerContactLastName!);
            result.AddToModelState(ModelState);
            return View(RequestsChangeNameViewPath, viewModel);
        }

        var sessionModel = new AccountCreationSessionModel
        {
            FirstName = submitModel.EmployerContactFirstName,
            LastName = submitModel.EmployerContactLastName
        };
        _sessionService.Set(sessionModel);

        return RedirectToRoute(RouteNames.CreateAccountCheckDetails, new { requestId });
    }

    private static void SetNamesInSessionModel(AccountCreationSessionModel? sessionModel, GetPermissionRequestResponse? permissionRequest)
    {
        if (sessionModel != null && permissionRequest != null)
        {
            permissionRequest.EmployerContactFirstName = sessionModel.FirstName;
            permissionRequest.EmployerContactLastName = sessionModel.LastName;
        }
    }


    private static EmployerUserNamesViewModel GetChangeNameViewModel(string firstName, string lastName)
    {
        return new EmployerUserNamesViewModel
        {
            EmployerContactFirstName = firstName,
            EmployerContactLastName = lastName
        };
    }
}
