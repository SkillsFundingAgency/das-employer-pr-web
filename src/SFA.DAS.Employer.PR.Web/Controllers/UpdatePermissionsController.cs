using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Common;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Models;

namespace SFA.DAS.Employer.PR.Web.Controllers;

//[Authorize(Policy = nameof(PolicyNames.HasEmployerOwnerAccount))]
[Route("accounts/{accountId}/updatepermissions/{requestId}", Name = RouteNames.UpdatePermissions)]
public sealed class UpdatePermissionsController(IOuterApiClient _outerApiClient, IValidator<ReviewPermissionsRequestSubmitViewModel> _validator) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index([FromRoute] Guid requestId, [FromRoute] string accountId, CancellationToken cancellationToken)
    {
        GetPermissionRequestResponse? response = await _outerApiClient.GetRequest(requestId, cancellationToken);

        if (!ValidateRequest(response))
        {
            return View(ViewNames.PageNotFound);
        }

        ReviewPermissionsRequestViewModel model = new ReviewPermissionsRequestViewModel()
        {
            ProviderName = response!.ProviderName,
            ViewYourTrainingProvidersLink = Url.RouteUrl(RouteNames.YourTrainingProviders, new { employerAccountId = accountId })!
        };

        MapOperationsToDescriptions(ref model, response.Operations);

        return View(ViewNames.ReviewPermissionsRequest, model);
    }

    private static bool ValidateRequest(GetPermissionRequestResponse? response)
    {
        return response is null || 
              (response is not null && response.RequestType != RequestType.Permission) ||
              (response is not null && (response.Status != RequestStatus.New || response.Status != RequestStatus.Sent));
    }

    [HttpPost]
    public async Task<IActionResult> Index([FromRoute] Guid requestId, [FromRoute]string accountId, ReviewPermissionsRequestSubmitViewModel model, CancellationToken cancellationToken)
    {
        GetPermissionRequestResponse? response = await _outerApiClient.GetRequest(requestId, cancellationToken);

        if (!ValidateRequest(response))
        {
            RedirectToRoute(RouteNames.YourTrainingProviders, new { employerAccountId = accountId });
        }

        var result = _validator.Validate(model);

        if (!result.IsValid)
        {
            result.AddToModelState(ModelState);

            var reviewPermissionModel = new ReviewPermissionsRequestViewModel()
            {
                ProviderName = response!.ProviderName,
                ViewYourTrainingProvidersLink = Url.RouteUrl(RouteNames.YourTrainingProviders, new { employerAccountId = accountId })!
            };

            MapOperationsToDescriptions(ref reviewPermissionModel, response.Operations);

            return View(ViewNames.ReviewPermissionsRequest, reviewPermissionModel);
        }

        if (model.AcceptPermissions!.Value)
        {
            // outer api accept permissions request

            return RedirectToRoute(RouteNames.YourTrainingProviders, new { employerAccountId = accountId });
        }
        else
        {
            return RedirectToRoute(RouteNames.YourTrainingProviders, new { employerAccountId = accountId });
        }
    }

    private static void MapOperationsToDescriptions(ref ReviewPermissionsRequestViewModel model, Operation[] operations)
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
