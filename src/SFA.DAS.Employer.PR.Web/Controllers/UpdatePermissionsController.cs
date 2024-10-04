﻿using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Common;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.OuterApi.Permissions;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Authentication;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Extensions;
using SFA.DAS.Employer.PR.Web.Helpers;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Models;
using System.Net;

namespace SFA.DAS.Employer.PR.Web.Controllers;

[Authorize(Policy = nameof(PolicyNames.HasEmployerOwnerAccount))]
[Route("accounts/{accountId}/updatepermissions/{requestId}", Name = RouteNames.UpdatePermissions)]
public sealed class UpdatePermissionsController(IOuterApiClient _outerApiClient, IValidator<ReviewPermissionsRequestSubmitViewModel> _validator) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index([FromRoute] Guid requestId, [FromRoute] string accountId, CancellationToken cancellationToken)
    {
        var response = await _outerApiClient.GetRequest(requestId, cancellationToken);

        if (!ReviewRequestHelper.IsValidRequest(response, RequestType.Permission))
        {
            return RedirectToAction("HttpStatusCodeHandler", RouteNames.Error, new { statusCode = (int)HttpStatusCode.NotFound });
        }

        var model = CreateReviewPermissionsRequestViewModel(response!, accountId);

        return View(ViewNames.ReviewPermissionsRequest, model);
    }

    [HttpPost]
    public async Task<IActionResult> Index([FromRoute] Guid requestId, [FromRoute]string accountId, ReviewPermissionsRequestSubmitViewModel model, CancellationToken cancellationToken)
    {
        GetPermissionRequestResponse? response = await _outerApiClient.GetRequest(requestId, cancellationToken);

        if (!ReviewRequestHelper.IsValidRequest(response, RequestType.Permission))
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

        ReviewRequestHelper.MapOperationsToDescriptions(ref viewModel, response.Operations);

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
}
