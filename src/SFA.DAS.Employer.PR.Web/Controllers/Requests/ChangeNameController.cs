using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Extensions;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Infrastructure.Services;
using SFA.DAS.Employer.PR.Web.Models.Requests;
using SFA.DAS.Employer.PR.Web.Models.Session;

namespace SFA.DAS.Employer.PR.Web.Controllers.Requests;

[Route("requests")]
public class ChangeNameController(IOuterApiClient _outerApiClient, ISessionService _sessionService, IValidator<ChangeNamesViewModel> _validator) : Controller
{
    public const string UserEmailDoesNotMatchRequestShutterPageViewPath = "~/Views/Requests/UserEmailDoesNotMatchRequestShutterPage.cshtml";
    public const string RequestsChangeNameViewPath = "~/Views/Requests/CreateServiceAccountChangeName.cshtml";

    [Authorize]
    [HttpGet]
    [Route("{requestId:guid}/createaccount/changename", Name = RouteNames.CreateAccountChangeName)]
    public async Task<IActionResult> Index(Guid requestId, CancellationToken cancellationToken)
    {
        Request.HttpContext.Items.Add(SessionKeys.AccountTasksKey, true);
        GetPermissionRequestResponse permissionRequest = await _outerApiClient.GetPermissionRequest(requestId, cancellationToken);

        if (User.GetEmail() != permissionRequest.EmployerContactEmail) return View(UserEmailDoesNotMatchRequestShutterPageViewPath);

        var sessionModel = _sessionService.Get<AccountCreationSessionModel>();
        GetNamesFromSessionModel(sessionModel, permissionRequest);

        ChangeNamesViewModel vm = GetChangeNamesViewModel(permissionRequest.EmployerContactFirstName!, permissionRequest.EmployerContactLastName!);

        return View(RequestsChangeNameViewPath, vm);
    }

    [Authorize]
    [HttpPost]
    [Route("{requestId:guid}/createaccount/changename", Name = RouteNames.CreateAccountChangeName)]
    public IActionResult Index(Guid requestId, ChangeNamesViewModel submitModel, CancellationToken cancellationToken)
    {
        Request.HttpContext.Items.Add(SessionKeys.AccountTasksKey, true);
        var result = _validator.Validate(submitModel);
        if (!result.IsValid)
        {
            ChangeNamesViewModel viewModel = GetChangeNamesViewModel(submitModel.EmployerContactFirstName!,
                submitModel.EmployerContactLastName!);
            result.AddToModelState(ModelState);
            return View(RequestsChangeNameViewPath, viewModel);
        }

        var sessionModel = _sessionService.Get<AccountCreationSessionModel>();
        if (sessionModel == null)
        {
            return RedirectToRoute(RouteNames.CreateAccountCheckDetails, new { requestId });
        }

        sessionModel.FirstName = submitModel.EmployerContactFirstName;
        sessionModel.LastName = submitModel.EmployerContactLastName;

        _sessionService.Set(sessionModel);

        return RedirectToRoute(RouteNames.CreateAccountCheckDetails, new { requestId });
    }

    private static void GetNamesFromSessionModel(AccountCreationSessionModel? sessionModel, GetPermissionRequestResponse permissionRequest)
    {
        if (sessionModel != null)
        {
            permissionRequest.EmployerContactFirstName = sessionModel.FirstName;
            permissionRequest.EmployerContactLastName = sessionModel.LastName;
        }
    }


    private static ChangeNamesViewModel GetChangeNamesViewModel(string firstName, string lastName)
    {
        return new ChangeNamesViewModel
        {
            EmployerContactFirstName = firstName,
            EmployerContactLastName = lastName
        };
    }
}
