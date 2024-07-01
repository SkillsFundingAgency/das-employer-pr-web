using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Web.Authentication;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Infrastructure.Services;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Employer.PR.Web.Models.Session;

namespace SFA.DAS.Employer.PR.Web.Controllers;

[Authorize(Policy = nameof(PolicyNames.HasEmployerOwnerAccount))]
[Route("accounts/{employerAccountId}/providers/new/selectProvider", Name = RouteNames.SelectTrainingProvider)]
public class SelectTrainingProviderController(ISessionService _sessionService, IValidator<SelectTrainingProviderSubmitViewModel> _validator) : Controller
{
    [HttpGet]
    public IActionResult Index([FromRoute] string employerAccountId)
    {
        var sessionModel = _sessionService.Get<AddTrainingProvidersSessionModel>();

        if (sessionModel == null)
        {
            return RedirectToAction("Index", "YourTrainingProviders", new { employerAccountId });
        }

        var backLink = SetBackLink(employerAccountId, sessionModel.AccountLegalEntities.Count);

        SelectTrainingProviderViewModel model = new SelectTrainingProviderViewModel(backLink!, sessionModel!.ProviderName, sessionModel!.Ukprn.ToString());
        return View(model);
    }

    [HttpPost]
    public IActionResult Index([FromRoute] string employerAccountId, SelectTrainingProviderSubmitViewModel submitModel)
    {
        var sessionModel = _sessionService.Get<AddTrainingProvidersSessionModel>();
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

        return RedirectToAction("Index", "SelectTrainingProvider");
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

    private SelectTrainingProviderViewModel GetViewModel(string employerAccountId, string? name, string? ukprn)
    {
        var backLink = Url.RouteUrl(RouteNames.YourTrainingProviders, new { employerAccountId });
        SelectTrainingProviderViewModel model = new SelectTrainingProviderViewModel(backLink!, name, ukprn);
        return model;
    }
}
