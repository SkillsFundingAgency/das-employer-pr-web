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

namespace SFA.DAS.Employer.PR.Web.Controllers;

[Authorize(Policy = nameof(PolicyNames.HasEmployerOwnerAccount))]
[Route("accounts/{employerAccountId}/providers/new/selectOrganisation", Name = RouteNames.SelectLegalEntity)]
public class SelectLegalEntityController(IOuterApiClient _outerApiClient, ISessionService _sessionService, IValidator<SelectLegalEntitiesSubmitViewModel> _validator) : Controller
{
    public const string ViewPath = "~/Views/SelectLegalEntity/Index.cshtml";

    [HttpGet]
    public async Task<IActionResult> Index([FromRoute] string employerAccountId, CancellationToken cancellationToken)
    {
        SelectLegalEntitiesViewModel model = await GetViewModel(employerAccountId, null, cancellationToken);
        return View(ViewPath, model);
    }

    [HttpPost]
    public async Task<IActionResult> Index([FromRoute] string employerAccountId, SelectLegalEntitiesSubmitViewModel submitModel, CancellationToken cancellationToken)
    {

        ValidationResult result = _validator.Validate(submitModel);

        if (!result.IsValid)
        {
            var model = await GetViewModel(employerAccountId, null, cancellationToken);
            result.AddToModelState(ModelState);
            return View(ViewPath, model);
        }

        SelectLegalEntitiesViewModel currentModel = await GetViewModel(employerAccountId, submitModel.LegalEntityId, cancellationToken);
        SetSessionModel(currentModel);
        return View(ViewPath, currentModel);
    }

    private void SetSessionModel(SelectLegalEntitiesViewModel currentModel)
    {
        var sessionModel = new AddTrainingProvidersSessionModel();
        var legalEntity = currentModel.LegalEntities.Find(legalEntity => legalEntity.IsSelected);
        if (legalEntity == null) return;

        sessionModel.LegalEntityId = legalEntity.LegalEntityId;
        sessionModel.LegalName = legalEntity.Name;
        _sessionService.Set(sessionModel);
    }

    private async Task<SelectLegalEntitiesViewModel> GetViewModel(string employerAccountId, long? legalEntityId,
        CancellationToken cancellationToken)
    {
        var backLink = Url.RouteUrl(RouteNames.YourTrainingProviders, new { employerAccountId });
        var cancelUrl = Url.RouteUrl(RouteNames.YourTrainingProviders, new { employerAccountId });

        var model = new SelectLegalEntitiesViewModel(cancelUrl!, backLink!);

        if (legalEntityId == null)
        {
            var sessionModel = _sessionService.Get<AddTrainingProvidersSessionModel>();
            legalEntityId = sessionModel?.LegalEntityId;
        }

        var response = await _outerApiClient.GetAccountLegalEntities(employerAccountId, cancellationToken);

        foreach (var legalEntity in response.AccountLegalEntities)
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
}
