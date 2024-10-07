using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Common;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.OuterApi.Permissions;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Extensions;
using SFA.DAS.Employer.PR.Web.Helpers;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Models;
using System.Net;

namespace SFA.DAS.Employer.PR.Web.Controllers;

[Route("accounts/{accountId}/addaccount/{requestId}", Name = RouteNames.AddAccounts)]
public sealed class AddAccountsController(IOuterApiClient _outerApiClient, IValidator<ReviewAddAccountRequestSubmitViewModel> _validator) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index([FromRoute] Guid requestId, [FromRoute] string accountId, CancellationToken cancellationToken)
    {
        var response = await _outerApiClient.GetRequest(requestId, cancellationToken);

        if (!ReviewRequestHelper.IsValidRequest(response, RequestType.AddAccount))
        {
            return RedirectToAction("HttpStatusCodeHandler", RouteNames.Error, new { statusCode = (int)HttpStatusCode.NotFound });
        }

        var model = CreateReviewAddAccountRequestViewModel(response!, accountId);

        return View(ViewNames.ReviewAddAccountsRequest, model);
    }

    [HttpPost]
    public async Task<IActionResult> Index([FromRoute] Guid requestId, [FromRoute] string accountId, ReviewAddAccountRequestSubmitViewModel model, CancellationToken cancellationToken)
    {
        GetPermissionRequestResponse? response = await _outerApiClient.GetRequest(requestId, cancellationToken);

        if (!ReviewRequestHelper.IsValidRequest(response, RequestType.AddAccount))
        {
            return RedirectToRoute(RouteNames.YourTrainingProviders, new { employerAccountId = accountId });
        }

        if (!IsModelValid(model))
        {
            var reviewPermissionModel = CreateReviewAddAccountRequestViewModel(response!, accountId);
            return View(ViewNames.ReviewAddAccountsRequest, reviewPermissionModel);
        }

        var userId = User.GetUserId().ToString();

        TempData[TempDataKeys.NameOfProviderUpdated] = response!.ProviderName;
        TempData[TempDataKeys.RequestTypeActioned] = response.RequestType.ToString();

        bool acceptRequest = model.AcceptAddAccountRequest!.Value;
        await HandleAddAccountRequest(requestId, userId, acceptRequest, cancellationToken);

        return RedirectToRoute(RouteNames.YourTrainingProviders, new { employerAccountId = accountId });
    }

    private ReviewAddAccountRequestViewModel CreateReviewAddAccountRequestViewModel(GetPermissionRequestResponse response, string accountId)
    {
        var viewModel = new ReviewAddAccountRequestViewModel
        {
            ProviderName = response.ProviderName,
            ViewYourTrainingProvidersLink = Url.RouteUrl(RouteNames.YourTrainingProviders, new { employerAccountId = accountId })!
        };

        ReviewRequestHelper.MapOperationsToDescriptions(ref viewModel, response.Operations);

        return viewModel;
    }

    private async Task HandleAddAccountRequest(Guid requestId, string userId, bool acceptRequest, CancellationToken cancellationToken)
    {
        if (acceptRequest)
        {
            await _outerApiClient.AcceptAddAccountRequest(requestId, new AcceptAddAccountRequestModel(userId), cancellationToken); // TO-DO update
            TempData[TempDataKeys.RequestAction] = RequestAction.Accepted.ToString();
        }
        else
        {
            await _outerApiClient.DeclineRequest(requestId, new DeclineRequestModel(userId), cancellationToken);
            TempData[TempDataKeys.RequestAction] = RequestAction.Declined.ToString();
        }
    }

    private bool IsModelValid(ReviewAddAccountRequestSubmitViewModel model)
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
