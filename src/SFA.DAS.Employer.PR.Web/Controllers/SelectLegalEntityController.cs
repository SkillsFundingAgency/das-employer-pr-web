using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Web.Authentication;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Infrastructure.Services;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Employer.PR.Web.Models.Session;

namespace SFA.DAS.Employer.PR.Web.Controllers;

[Authorize(Policy = nameof(PolicyNames.HasEmployerOwnerAccount))]
[Route("accounts/{employerAccountId}/providers/new/selectOrganisation", Name = RouteNames.SelectLegalEntity)]
public class SelectLegalEntityController(ISessionService _sessionService, IValidator<SelectLegalEntitiesSubmitViewModel> _validator) : Controller
{
    public const string ViewPath = "~/Views/SelectLegalEntity/Index.cshtml";

    [HttpGet]
    public IActionResult Index([FromRoute] string employerAccountId)
    {
        var sessionModel = _sessionService.Get<AddTrainingProvidersSessionModel>();

        if (sessionModel == null)
        {
            return RedirectToAction("Index", "YourTrainingProviders", new { employerAccountId });

        }

        ResetSessionForJourney(sessionModel);

        SelectLegalEntitiesViewModel model = GetViewModel(employerAccountId, sessionModel);

        if (model.LegalEntities.Count == 1)
        {
            var legalEntity = model.LegalEntities[0];
            sessionModel.LegalEntityId = legalEntity.LegalEntityId;
            sessionModel.LegalName = legalEntity.Name;
            _sessionService.Set(sessionModel);

            return RedirectToAction("Index", "SelectTrainingProvider", new { employerAccountId });
        }
        return View(ViewPath, model);
    }

    [HttpPost]
    public IActionResult Index([FromRoute] string employerAccountId, SelectLegalEntitiesSubmitViewModel submitModel)
    {
        var sessionModel = _sessionService.Get<AddTrainingProvidersSessionModel>();

        ValidationResult result = _validator.Validate(submitModel);

        if (!result.IsValid)
        {
            var model = GetViewModel(employerAccountId, sessionModel);
            result.AddToModelState(ModelState);
            return View(ViewPath, model);
        }

        SetSessionForSelectionMade(submitModel.LegalEntityId!.Value, sessionModel);
        return RedirectToAction("Index", "SelectTrainingProvider", new { employerAccountId });
    }


    private SelectLegalEntitiesViewModel GetViewModel(string employerAccountId, AddTrainingProvidersSessionModel sessionModel)
    {
        var backLink = Url.RouteUrl(RouteNames.YourTrainingProviders, new { employerAccountId });
        var cancelUrl = Url.RouteUrl(RouteNames.YourTrainingProviders, new { employerAccountId });

        var model = new SelectLegalEntitiesViewModel(cancelUrl!, backLink!);

        var legalEntityId = sessionModel.LegalEntityId;

        foreach (var legalEntity in sessionModel.AccountLegalEntities)
        {
            LegalEntityModel legalEntityToAdd = legalEntity;

            if (legalEntity.Id == legalEntityId)
            {
                legalEntityToAdd.IsSelected = true;
            }

            model.LegalEntities.Add(legalEntityToAdd);
        }

        return model;
    }

    private void SetSessionForSelectionMade(long legalEntityId, AddTrainingProvidersSessionModel sessionModel)
    {
        var legalEntity = sessionModel.AccountLegalEntities.First(l => l.Id == legalEntityId);
        sessionModel.LegalEntityId = legalEntityId;
        sessionModel.LegalName = legalEntity.Name;
        _sessionService.Set(sessionModel);

    }

    private void ResetSessionForJourney(AddTrainingProvidersSessionModel sessionModel)
    {
        sessionModel.ProviderName = null;
        sessionModel.Ukprn = null;
        _sessionService.Set(sessionModel);
    }
}
