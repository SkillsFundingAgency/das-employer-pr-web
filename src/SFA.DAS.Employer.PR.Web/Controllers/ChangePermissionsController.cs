using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Domain.OuterApi.Permissions;
using SFA.DAS.Employer.PR.Web.Authentication;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Infrastructure.Services;
using SFA.DAS.Employer.PR.Web.Models;
using System.Security.Claims;

namespace SFA.DAS.Employer.PR.Web.Controllers;

[Authorize(Policy = nameof(PolicyNames.HasEmployerOwnerAccount))]
[Route("accounts/{employerAccountId}/providers/changePermissions", Name = RouteNames.ChangePermissions)]
public class ChangePermissionsController(IOuterApiClient _outerApiClient, ISessionService _sessionService,
    IValidator<ChangePermissionsSubmitViewModel> _validator) : Controller
{
    [HttpGet]
    public IActionResult Index([FromRoute] string employerAccountId, [FromQuery] long legalEntityId,
        [FromQuery] long ukprn)
    {


        // get the ukprn/legal entity details....

        var accountLegalEntityName = "legal entity name";
        var providerName = " provider name";
        var currentAddRecords = SetPermissions.AddRecords.Yes;
        var currentRecruitApprentices = SetPermissions.RecruitApprentices.YesWithReview;


        var backLink = Url.RouteUrl(RouteNames.YourTrainingProviders, new { employerAccountId });
        var cancelLink = Url.RouteUrl(RouteNames.YourTrainingProviders, new { employerAccountId });

        ChangePermissionsViewModel viewModel = new ChangePermissionsViewModel(legalEntityId!,
            accountLegalEntityName, providerName, ukprn, backLink!, cancelLink!);

        viewModel.AddRecords = currentAddRecords;
        viewModel.AddRecordsOriginal = currentAddRecords;
        viewModel.RecruitApprentices = currentRecruitApprentices;
        viewModel.RecruitApprenticesOriginal = currentRecruitApprentices;

        // var model = GetViewModel(employerAccountId);
        // if (model == null)
        // {
        //     return RedirectToAction("Index", "YourTrainingProviders", new { employerAccountId });
        // }
        //

        return View(viewModel);

    }

    [HttpPost]
    public async Task<IActionResult> Index([FromRoute] string employerAccountId,
        ChangePermissionsSubmitViewModel submitModel, CancellationToken cancellationToken)
    {
        var result = _validator.Validate(submitModel);

        if (!result.IsValid)
        {
            var model = GetViewModel(employerAccountId, submitModel);
            result.AddToModelState(ModelState);
            return View(model);
        }

        var operationsToSet = new List<Operation>();

        if (submitModel.AddRecords == SetPermissions.AddRecords.Yes)
        {
            operationsToSet.Add(Operation.CreateCohort);
        }

        if (submitModel.RecruitApprentices == SetPermissions.RecruitApprentices.Yes)
        {
            operationsToSet.Add(Operation.Recruitment);
        }
        else if (submitModel.RecruitApprentices == SetPermissions.RecruitApprentices.YesWithReview)
        {
            operationsToSet.Add(Operation.RecruitmentRequiresReview);
        }


        var userRef = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var command = new PostPermissionsCommand(userRef!, submitModel.Ukprn, submitModel.LegalEntityId, operationsToSet);

        await _outerApiClient.PostPermissions(command, cancellationToken);

        return RedirectToAction("Index", "YourTrainingProviders", new { employerAccountId });
    }


    private ChangePermissionsViewModel GetViewModel(string employerAccountId,
        ChangePermissionsSubmitViewModel submitModel)
    {
        var backLink = Url.RouteUrl(RouteNames.YourTrainingProviders, new { employerAccountId });
        var cancelLink = Url.RouteUrl(RouteNames.YourTrainingProviders, new { employerAccountId });
        ChangePermissionsViewModel viewModel = new ChangePermissionsViewModel(submitModel.LegalEntityId,
            submitModel.LegalName, submitModel.ProviderName, submitModel.Ukprn, backLink!, cancelLink!);

        viewModel.AddRecords = submitModel.AddRecords;
        viewModel.AddRecordsOriginal = submitModel.AddRecordsOriginal;
        viewModel.RecruitApprentices = submitModel.RecruitApprentices;
        viewModel.RecruitApprenticesOriginal = submitModel.RecruitApprenticesOriginal;

        return viewModel;
    }
}