using System.Net;
using FluentValidation;
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

namespace SFA.DAS.Employer.PR.Web.Controllers;

[Authorize(Policy = nameof(PolicyNames.HasEmployerOwnerAccount))]
[Route("accounts/{employerAccountId}/addaccount/{requestId}", Name = RouteNames.AddAccounts)]
public sealed class AddAccountsController(IOuterApiClient _outerApiClient, IValidator<ReviewAddAccountRequestSubmitViewModel> _validator) : Controller
{
    public const string CannotViewRequestShutterPageViewPath = "~/Views/Requests/CannotViewRequest.cshtml";

    [HttpGet]
    public async Task<IActionResult> Index([FromRoute] Guid requestId, [FromRoute] string employerAccountId, [FromQuery] bool? acceptAddAccountRequest, CancellationToken cancellationToken)
    {
        var response = await _outerApiClient.GetRequest(requestId, cancellationToken);

        if (!ReviewRequestHelper.IsValidRequest(response, RequestType.AddAccount))
        {
            return View(CannotViewRequestShutterPageViewPath);
        }

        var model = CreateReviewAddAccountRequestViewModel(response!, employerAccountId, acceptAddAccountRequest);

        return View(ViewNames.ReviewAddAccountsRequest, model);
    }

    [HttpPost]
    public async Task<IActionResult> Index([FromRoute] Guid requestId, [FromRoute] string employerAccountId, ReviewAddAccountRequestSubmitViewModel model, CancellationToken cancellationToken)
    {
        GetPermissionRequestResponse? response = await _outerApiClient.GetRequest(requestId, cancellationToken);

        if (!ReviewRequestHelper.IsValidRequest(response, RequestType.AddAccount))
        {
            return RedirectToRoute(RouteNames.YourTrainingProviders, new { employerAccountId });
        }

        if (!IsModelValid(model))
        {
            var reviewPermissionModel = CreateReviewAddAccountRequestViewModel(response!, employerAccountId, null);
            return View(ViewNames.ReviewAddAccountsRequest, reviewPermissionModel);
        }

        var userId = User.GetUserId().ToString();

        TempData[TempDataKeys.NameOfProviderUpdated] = response!.ProviderName;
        TempData[TempDataKeys.RequestTypeActioned] = response.RequestType.ToString();

        bool acceptRequest = model.AcceptAddAccountRequest!.Value;

        return await HandleAddAccountRequest(requestId, employerAccountId, userId, acceptRequest, cancellationToken);
    }

    private ReviewAddAccountRequestViewModel CreateReviewAddAccountRequestViewModel(GetPermissionRequestResponse response, string accountId, bool? acceptAddAccountRequest)
    {
        var viewModel = new ReviewAddAccountRequestViewModel
        {
            ProviderName = response.ProviderName,
            ViewYourTrainingProvidersLink = Url.RouteUrl(RouteNames.YourTrainingProviders, new { employerAccountId = accountId })!,
            AcceptAddAccountRequest = acceptAddAccountRequest
        };

        ReviewRequestHelper.MapOperationsToDescriptions(ref viewModel, response.Operations);

        return viewModel;
    }

    private async Task<IActionResult> HandleAddAccountRequest(Guid requestId, string employerAccountId, string userId, bool acceptRequest, CancellationToken cancellationToken)
    {
        if (acceptRequest)
        {
            await _outerApiClient.AcceptAddAccountRequest(requestId, new AcceptAddAccountRequestModel(userId), cancellationToken);
            TempData[TempDataKeys.RequestAction] = RequestAction.Accepted.ToString();

            return RedirectToRoute(RouteNames.YourTrainingProviders, new { employerAccountId });
        }
        else
        {
            return RedirectToRoute(RouteNames.DeclineAddAccount, new { employerAccountId, requestId });
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
