using Azure.Core;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Models;
using static SFA.DAS.Employer.PR.Domain.Common.PermissionRequest;

namespace SFA.DAS.Employer.PR.Web.Controllers;

[Route("accounts/{accountId}/updatepermissions/{requestId}", Name = RouteNames.ReviewRequest)]
public sealed class UpdatePermissionsController(IOuterApiClient _outerApiClient, IValidator<ReviewPermissionRequestSubmitViewModel> _validator) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index([FromRoute] Guid requestId, CancellationToken cancellationToken)
    {
        GetPermissionRequestResponse? response = await _outerApiClient.GetRequest(requestId, cancellationToken);

        if (ValidateRequest(response))
        {
            return View(ViewNames.PageNotFound);
        }

        ReviewPermissionRequestViewModel model = new ReviewPermissionRequestViewModel()
        {
            ProviderName = response!.ProviderName
        };

        MapOperationsToDescriptions(ref model, response.Operations);

        return View(ViewNames.ReviewPermissionRequest, model);
    }

    private static bool ValidateRequest(GetPermissionRequestResponse? response)
    {
        return response is null || 
              (response is not null && response.RequestType != RequestType.Permission) ||
              (response is not null && (response.Status != RequestStatus.New || response.Status != RequestStatus.Sent));
    }

    [HttpPost]
    public async Task<IActionResult> Index([FromRoute] Guid requestId, ReviewPermissionRequestSubmitViewModel model, CancellationToken cancellationToken)
    {
        var result = _validator.Validate(model);

        if (!result.IsValid)
        {
            result.AddToModelState(ModelState);

            GetPermissionRequestResponse? response = await _outerApiClient.GetRequest(requestId, cancellationToken);

            if (response is null)
            {
                return View(ViewNames.CannotViewRequest);
            }

            var reviewPermissionModel = new ReviewPermissionRequestViewModel()
            {
                ProviderName = response!.ProviderName,
            };

            MapOperationsToDescriptions(ref reviewPermissionModel, response.Operations);

            return View(ViewNames.ReviewPermissionRequest, reviewPermissionModel);
        }

        if (model.AcceptPermissions!.Value)
        {
            // outer api accept permissions request
            return RedirectToAction(RouteNames.YourTrainingProviders); // need account hashed id
        }
        else
        {
            return RedirectToAction(RouteNames.YourTrainingProviders);
        }
    }

    private static void MapOperationsToDescriptions(ref ReviewPermissionRequestViewModel model, Operation[] operations)
    {
        model.AddApprenticeRecordsText = operations.Contains(Operation.CreateCohort)
            ? ReviewPermissions.Yes
            : ReviewPermissions.No;

        model.RecruitApprenticesText = ReviewPermissions.No;

        if (operations.Contains(Operation.Recruitment))
        {
            model.RecruitApprenticesText = ReviewPermissions.Yes;
        }
        else if (operations.Contains(Operation.RecruitmentRequiresReview))
        {
            model.RecruitApprenticesText = ReviewPermissions.YesWithEmployerReview;
        }
    }
}
