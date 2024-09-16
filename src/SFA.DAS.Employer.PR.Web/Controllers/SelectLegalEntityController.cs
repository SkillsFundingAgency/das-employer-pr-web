using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Web.Authentication;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Infrastructure.Services;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Employer.PR.Web.Models.Session;
using SFA.DAS.Encoding;

namespace SFA.DAS.Employer.PR.Web.Controllers;

[Authorize(Policy = nameof(PolicyNames.HasEmployerOwnerAccount))]
[Route("accounts/{employerAccountId}/providers/new/selectOrganisation", Name = RouteNames.SelectLegalEntity)]
public class SelectLegalEntityController(IOuterApiClient _outerApiClient, ISessionService _sessionService, IValidator<SelectLegalEntitiesSubmitViewModel> _validator, IEncodingService _encodingService) : Controller
{
    public const string ViewPath = "~/Views/SelectLegalEntity/Index.cshtml";

    [HttpGet]
    public async Task<IActionResult> Index([FromRoute] string employerAccountId, CancellationToken cancellationToken)
    {
        var sessionModel = _sessionService.Get<AddTrainingProvidersSessionModel>();

        if (sessionModel == null)
        {
            var accountId = _encodingService.Decode(employerAccountId, EncodingType.AccountId);
            var response = await _outerApiClient.GetEmployerRelationships(accountId, cancellationToken);

            sessionModel = new AddTrainingProvidersSessionModel
            {
                AccountLegalEntities = response.AccountLegalEntities.OrderBy(a => a.Name).ToList(),
                EmployerAccountId = employerAccountId

            };
            _sessionService.Set(sessionModel);
        }

        if (sessionModel.EmployerAccountId != employerAccountId)
        {
            return RedirectToRoute(RouteNames.YourTrainingProviders, new { employerAccountId });
        }

        SelectLegalEntitiesViewModel model = GetViewModel(employerAccountId, sessionModel);

        if (model.LegalEntities.Count != 1)
        {
            return View(ViewPath, model);
        }

        var legalEntity = model.LegalEntities[0];
        sessionModel.SelectedLegalEntityId = legalEntity.LegalEntityId;
        sessionModel.SelectedLegalName = legalEntity.Name;
        _sessionService.Set(sessionModel);

        return RedirectToRoute(RouteNames.SelectTrainingProvider, new { employerAccountId });
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
        return RedirectToRoute(RouteNames.SelectTrainingProvider, new { employerAccountId });
    }

    private SelectLegalEntitiesViewModel GetViewModel(string employerAccountId, AddTrainingProvidersSessionModel sessionModel)
    {
        var backLink = Url.RouteUrl(RouteNames.YourTrainingProviders, new { employerAccountId });
        var cancelUrl = Url.RouteUrl(RouteNames.YourTrainingProviders, new { employerAccountId });

        var model = new SelectLegalEntitiesViewModel(cancelUrl!, backLink!);

        var legalEntityId = sessionModel.SelectedLegalEntityId;

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
        sessionModel.SelectedLegalEntityId = legalEntityId;
        sessionModel.SelectedLegalName = legalEntity.Name;
        _sessionService.Set(sessionModel);

    }
}
