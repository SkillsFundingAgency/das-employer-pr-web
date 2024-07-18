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
using SFA.DAS.Employer.PR.Web.Services;
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
            return RedirectToRoute(RouteNames.YourTrainingProviders, new { employerAccountId });
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Index([FromRoute] string employerAccountId, SetPermissionsSubmitViewModel submitModel, CancellationToken cancellationToken)
    {
        var sessionModel = _sessionService.Get<AddTrainingProvidersSessionModel>();
        if (sessionModel == null || sessionModel!.Ukprn == null || sessionModel!.SelectedLegalEntityId == null || sessionModel.EmployerAccountId != employerAccountId)
        {
            return RedirectToRoute(RouteNames.YourTrainingProviders, new { employerAccountId });
        }

        var result = _validator.Validate(submitModel);

        if (!result.IsValid)
        {
            var model = GetViewModel(employerAccountId);
            result.AddToModelState(ModelState);
            return View(model);
        }

        var permissionDescriptions = (PermissionDescriptionsModel)submitModel;

        var operationsToSet = OperationsMappingService.MapDescriptionsToOperations(permissionDescriptions);

        var userRef = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var command = new PostPermissionsCommand(userRef!, sessionModel.Ukprn.Value, sessionModel.SelectedLegalEntityId.Value, operationsToSet);

        await _outerApiClient.PostPermissions(command, cancellationToken);

        TempData[TempDataKeys.NameOfProviderAdded] = sessionModel.ProviderName;

        return RedirectToRoute(RouteNames.YourTrainingProviders, new { employerAccountId });
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

        SetPermissionsViewModel model = new SetPermissionsViewModel(sessionModel.SelectedLegalEntityId!.Value,
            sessionModel.SelectedLegalName!, sessionModel.ProviderName!, sessionModel.Ukprn!.Value, backLink!, cancelLink!);
        return model;
    }
}
