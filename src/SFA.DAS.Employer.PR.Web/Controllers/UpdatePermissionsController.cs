﻿using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Common;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Domain.OuterApi.Permissions;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Authentication;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Extensions;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Models;

namespace SFA.DAS.Employer.PR.Web.Controllers;

[Authorize(Policy = nameof(PolicyNames.HasEmployerOwnerAccount))]
[Route("accounts/{accountId}/updatepermissions/{requestId}", Name = RouteNames.UpdatePermissions)]
public sealed class UpdatePermissionsController(IOuterApiClient _outerApiClient, IValidator<ReviewPermissionsRequestSubmitViewModel> _validator) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index([FromRoute] Guid requestId, [FromRoute] string accountId, CancellationToken cancellationToken)
    {
        var response = await _outerApiClient.GetRequest(requestId, cancellationToken);

        if (!IsValidRequest(response))
        {
            return View(ViewNames.PageNotFound);
        }

        var model = CreateReviewPermissionsRequestViewModel(response!, accountId);

        return View(ViewNames.ReviewPermissionsRequest, model);
    }

    [HttpPost]
    public async Task<IActionResult> Index([FromRoute] Guid requestId, [FromRoute]string accountId, ReviewPermissionsRequestSubmitViewModel model, CancellationToken cancellationToken)
    {
        GetPermissionRequestResponse? response = await _outerApiClient.GetRequest(requestId, cancellationToken);

        if (!IsValidRequest(response))
        {
            return RedirectToRoute(RouteNames.YourTrainingProviders, new { employerAccountId = accountId });
        }

        if (!IsModelValid(model))
        {
            var reviewPermissionModel = CreateReviewPermissionsRequestViewModel(response!, accountId);
            return View(ViewNames.ReviewPermissionsRequest, reviewPermissionModel);
        }

        var userId = User.GetUserId().ToString();

        TempData[TempDataKeys.NameOfProviderUpdated] = response!.ProviderName;
        TempData[TempDataKeys.RequestTypeActioned] = response.RequestType.ToString();

        bool acceptRequest = model.AcceptPermissions!.Value;
        await HandlePermissionsRequest(requestId, userId, acceptRequest, cancellationToken);

        return RedirectToRoute(RouteNames.YourTrainingProviders, new { employerAccountId = accountId });
    }

    private ReviewPermissionsRequestViewModel CreateReviewPermissionsRequestViewModel(GetPermissionRequestResponse response, string accountId)
    {
        var viewModel = new ReviewPermissionsRequestViewModel
        {
            ProviderName = response.ProviderName,
            ViewYourTrainingProvidersLink = Url.RouteUrl(RouteNames.YourTrainingProviders, new { employerAccountId = accountId })!
        };

        MapOperationsToDescriptions(ref viewModel, response.Operations);

        return viewModel;
    }

    private async Task HandlePermissionsRequest(Guid requestId, string userId, bool acceptRequest, CancellationToken cancellationToken)
    {
        if (acceptRequest)
        {
            await _outerApiClient.AcceptPermissionsRequest(requestId, new AcceptPermissionsRequestModel(userId), cancellationToken);
            TempData[TempDataKeys.RequestAction] = RequestAction.Accepted.ToString();
        }
        else
        {
            await _outerApiClient.DeclineRequest(requestId, new DeclineRequestModel(userId), cancellationToken);
            TempData[TempDataKeys.RequestAction] = RequestAction.Declined.ToString();
        }
    }

    private bool IsModelValid(ReviewPermissionsRequestSubmitViewModel model)
    {
        var result = _validator.Validate(model);
        if (!result.IsValid)
        {
            result.AddToModelState(ModelState);
            return false;
        }
        return true;
    }

    private static bool IsValidRequest(GetPermissionRequestResponse? response)
    {
        return response != null &&
               response.RequestType == RequestType.Permission &&
              (response!.Status == RequestStatus.New || response.Status == RequestStatus.Sent);
    }

    private static void MapOperationsToDescriptions(ref ReviewPermissionsRequestViewModel model, Operation[] operations)
    {
        model.AddApprenticeRecordsText = operations.Contains(Operation.CreateCohort) ? ReviewPermissions.Yes : ReviewPermissions.No;
        model.RecruitApprenticesText = operations.Contains(Operation.Recruitment)
            ? ReviewPermissions.Yes
            : operations.Contains(Operation.RecruitmentRequiresReview)
                ? ReviewPermissions.YesWithEmployerReview
                : ReviewPermissions.No;
    }
}