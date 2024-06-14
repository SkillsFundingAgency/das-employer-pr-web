﻿using FluentValidation;
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
using SFA.DAS.Employer.PR.Web.Models.Session;
using System.Security.Claims;

namespace SFA.DAS.Employer.PR.Web.Controllers;

[Authorize(Policy = nameof(PolicyNames.HasEmployerOwnerAccount))]
[Route("accounts/{employerAccountId}/providers/new/setPermissions", Name = RouteNames.SetPermissions)]
public class SetPermissionsController(IOuterApiClient _outerApiClient, ISessionService _sessionService, IValidator<SetPermissionsSubmitViewModel> _validator) : Controller
{
    [HttpGet]
    public IActionResult Index([FromRoute] string employerAccountId)
    {
        var model = GetViewModel(employerAccountId);
        if (model == null)
        {
            return RedirectToAction("Index", "YourTrainingProviders", new { employerAccountId });
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Index([FromRoute] string employerAccountId, SetPermissionsSubmitViewModel submitModel, CancellationToken cancellationToken)
    {
        var result = _validator.Validate(submitModel);

        if (!result.IsValid)
        {
            var model = GetViewModel(employerAccountId);
            result.AddToModelState(ModelState);
            return View(model);
        }

        var sessionModel = _sessionService.Get<AddTrainingProvidersSessionModel>();
        if (sessionModel == null || sessionModel!.Ukprn == null || sessionModel!.LegalEntityId == null || sessionModel.EmployerAccountId != employerAccountId)
        {
            return RedirectToAction("Index", "YourTrainingProviders", new { employerAccountId });
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

        var command = new PostPermissionsCommand(userRef!, sessionModel.Ukprn.Value, sessionModel.LegalEntityId.Value, operationsToSet);

        await _outerApiClient.PostPermissions(command, cancellationToken);

        sessionModel.AddRecords = submitModel.AddRecords;
        sessionModel.RecruitApprentices = submitModel.RecruitApprentices;
        sessionModel.SuccessfulAddition = true;
        _sessionService.Set(sessionModel);

        return RedirectToAction("Index", "YourTrainingProviders", new { employerAccountId });
    }

    private SetPermissionsViewModel? GetViewModel(string employerAccountId)
    {
        var sessionModel = _sessionService.Get<AddTrainingProvidersSessionModel>();

        if (sessionModel?.Ukprn == null || sessionModel.EmployerAccountId != employerAccountId)
        {
            return null;
        }

        var backLink = Url.RouteUrl(RouteNames.SelectTrainingProvider, new { employerAccountId });
        var cancelLink = Url.RouteUrl(RouteNames.YourTrainingProviders, new { employerAccountId });

        SetPermissionsViewModel model = new SetPermissionsViewModel(sessionModel.LegalEntityId!.Value,
            sessionModel.LegalName!, sessionModel.ProviderName!, sessionModel.Ukprn!.Value, backLink!, cancelLink!);
        return model;
    }
}
