using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Web.Authentication;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Infrastructure.Services;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Employer.PR.Web.Models.Session;
using SFA.DAS.Encoding;
using System.Net;

namespace SFA.DAS.Employer.PR.Web.Controllers;

[Authorize(Policy = nameof(PolicyNames.HasEmployerOwnerAccount))]
[Route("accounts/{employerAccountId}/providers/new/selectProvider", Name = RouteNames.SelectTrainingProvider)]
public class SelectTrainingProviderController(IOuterApiClient _outerApiClient, ISessionService _sessionService, IEncodingService _encodingService, IValidator<SelectTrainingProviderSubmitModel> _validator) : Controller
{
    public const string ShutterPageViewPath = "~/Views/AddPermissions/ShutterPage.cshtml";

    [HttpGet]
    public IActionResult Index([FromRoute] string employerAccountId)
    {
        var sessionModel = _sessionService.Get<AddTrainingProvidersSessionModel>();

        if (sessionModel?.SelectedLegalEntityId == null || sessionModel.EmployerAccountId != employerAccountId)
        {
            return RedirectToRoute(RouteNames.YourTrainingProviders, new { employerAccountId });
        }

        var backLink = SetBackLink(employerAccountId, sessionModel.AccountLegalEntities.Count);

        SelectTrainingProviderModel model = new SelectTrainingProviderModel(backLink!, sessionModel!.ProviderName, sessionModel!.Ukprn.ToString());
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Index([FromRoute] string employerAccountId, SelectTrainingProviderSubmitModel submitModel, CancellationToken cancellationToken)
    {
        var sessionModel = _sessionService.Get<AddTrainingProvidersSessionModel>();

        if (sessionModel?.EmployerAccountId != employerAccountId)
        {
            return RedirectToRoute(RouteNames.YourTrainingProviders, new { employerAccountId });
        }

        submitModel.SearchTerm = submitModel.Name;
        var result = _validator.Validate(submitModel);

        if (!result.IsValid)
        {
            var model = GetViewModel(employerAccountId, null, null);
            model.BackLink = SetBackLink(employerAccountId, sessionModel.AccountLegalEntities.Count);
            result.AddToModelState(ModelState);
            return View(model);
        }

        sessionModel.Ukprn = Convert.ToInt64(submitModel.Ukprn);
        sessionModel.ProviderName = submitModel.Name;
        _sessionService.Set(sessionModel);

        var existingPermissions = await _outerApiClient.GetPermissions(sessionModel.Ukprn.Value, sessionModel.SelectedLegalEntityId!.Value, cancellationToken);

        if (existingPermissions.ResponseMessage.StatusCode == HttpStatusCode.NotFound)
        {
            return RedirectToRoute(RouteNames.AddPermissions, new { employerAccountId });
        }

        var hashedId = _encodingService.Encode(sessionModel.SelectedLegalEntityId!.Value, EncodingType.PublicAccountLegalEntityId);

        var shutterPageViewModel = new AddPermissionsShutterPageViewModel
        (
            sessionModel.ProviderName!,
            sessionModel.Ukprn.Value,
            hashedId,
            Url.RouteUrl(RouteNames.ChangePermissions, new { employerAccountId })!,
            Url.RouteUrl(RouteNames.YourTrainingProviders, new { employerAccountId })!
            );

        return View(ShutterPageViewPath, shutterPageViewModel);
    }

    private string SetBackLink(string employerAccountId, int numberOfLegalEntities)
    {
        var backLink = Url.RouteUrl(RouteNames.SelectLegalEntity, new { employerAccountId });

        if (numberOfLegalEntities == 1)
        {
            backLink = Url.RouteUrl(RouteNames.YourTrainingProviders, new { employerAccountId });
        }

        return backLink!;
    }

    private SelectTrainingProviderModel GetViewModel(string employerAccountId, string? name, string? ukprn)
    {
        var backLink = Url.RouteUrl(RouteNames.YourTrainingProviders, new { employerAccountId });
        SelectTrainingProviderModel model = new SelectTrainingProviderModel(backLink!, name, ukprn);
        return model;
    }
}
