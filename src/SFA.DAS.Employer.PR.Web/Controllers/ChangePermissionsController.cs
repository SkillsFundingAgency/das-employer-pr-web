using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.OuterApi.Requests;
using SFA.DAS.Employer.PR.Web.Authentication;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Extensions;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Employer.PR.Web.Services;
using SFA.DAS.Encoding;
using System.Net;

namespace SFA.DAS.Employer.PR.Web.Controllers;

[Authorize(Policy = nameof(PolicyNames.HasEmployerOwnerAccount))]
[Route("accounts/{employerAccountId}/providers/{ukprn}/changePermissions", Name = RouteNames.ChangePermissions)]
public class ChangePermissionsController(IOuterApiClient _outerApiClient, IEncodingService _encodingService, IValidator<ChangePermissionsSubmitModel> _validator) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index([FromRoute] string employerAccountId, [FromQuery] string legalEntityPublicHashedId,
         [FromRoute] long ukprn, CancellationToken cancellationToken)
    {
        var legalEntityId = _encodingService.Decode(legalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId);

        var viewModel = await GetViewModel(employerAccountId, legalEntityId, ukprn, cancellationToken);
        if (viewModel == null)
        {
            return RedirectToRoute(RouteNames.YourTrainingProviders, new { employerAccountId });
        }

        return View(viewModel);
    }


    [HttpPost]
    public async Task<IActionResult> Index([FromRoute] string employerAccountId,
        ChangePermissionsSubmitModel submitViewModel, CancellationToken cancellationToken)
    {
        var model = await GetViewModel(employerAccountId, submitViewModel.LegalEntityId, submitViewModel.Ukprn, cancellationToken);

        if (model == null)
        {
            return RedirectToRoute(RouteNames.YourTrainingProviders, new { employerAccountId });
        }

        var result = _validator.Validate(submitViewModel);

        if (!result.IsValid)
        {
            result.AddToModelState(ModelState);
            return View(model);
        }

        var permissionDescriptions = (PermissionDescriptionsModel)submitViewModel;

        var operationsToSet = OperationsMappingService.MapDescriptionsToOperations(permissionDescriptions);


        var userRef = User.GetUserId();

        var command = new PostPermissionsRequest(userRef, submitViewModel.Ukprn, submitViewModel.LegalEntityId, operationsToSet);

        await _outerApiClient.PostPermissions(command, cancellationToken);

        TempData[TempDataKeys.NameOfProviderUpdated] = model.ProviderName;

        return RedirectToRoute(RouteNames.YourTrainingProviders, new { employerAccountId });
    }

    private async Task<ChangePermissionsViewModel?> GetViewModel(string employerAccountId, long legalEntityId, long ukprn,
        CancellationToken cancellationToken)
    {
        var relationshipDetailsResponse = await _outerApiClient.GetPermissions(ukprn, legalEntityId, cancellationToken);

        if (relationshipDetailsResponse.ResponseMessage.StatusCode != HttpStatusCode.OK)
        {
            return null;
        }

        var relationshipDetails = relationshipDetailsResponse.GetContent();

        var accountLegalEntityName = relationshipDetails.AccountLegalEntityName;
        var providerName = relationshipDetails.ProviderName;

        var currentOperations = relationshipDetails.Operations.ToList();

        var permissionDescriptions = OperationsMappingService.MapOperationsToDescriptions(currentOperations);

        var backLink = Url.RouteUrl(RouteNames.YourTrainingProviders, new { employerAccountId });

        ChangePermissionsViewModel viewModel = new ChangePermissionsViewModel(backLink!,
            permissionDescriptions.PermissionToAddCohorts!, permissionDescriptions.PermissionToRecruit!, providerName, accountLegalEntityName, legalEntityId, ukprn);
        return viewModel;
    }
}