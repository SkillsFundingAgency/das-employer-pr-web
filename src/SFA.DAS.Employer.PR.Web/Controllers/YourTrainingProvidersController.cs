using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Web.Authentication;
using SFA.DAS.Employer.PR.Web.Extensions;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Infrastructure.Services;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Employer.PR.Web.Models.Session;

namespace SFA.DAS.Employer.PR.Web.Controllers;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
[Route("accounts/{employerAccountId}/providers", Name = RouteNames.YourTrainingProviders)]
public class YourTrainingProvidersController(IOuterApiClient _outerApiClient, ISessionService _sessionService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index([FromRoute] string employerAccountId, CancellationToken cancellationToken)
    {
        _sessionService.Delete<TrainingProvidersSessionModel>();

        var response = await _outerApiClient.GetAccountLegalEntities(employerAccountId, cancellationToken);
        var accountLegalEntities = response.AccountLegalEntities.OrderBy(a => a.Name);

        var legalEntities = new List<LegalEntityModel>();

        foreach (var ale in accountLegalEntities)
        {
            var legalEntity = (LegalEntityModel)ale;

            var permissions = new List<PermissionModel>();

            foreach (var p in ale.Permissions.OrderBy(p => p.ProviderName))
            {
                var pm = (PermissionModel)p;
                pm.ChangePermissionsLink = Url.RouteUrl(RouteNames.ChangePermissions, new { employerAccountId, legalEntity.LegalEntityId, pm.Ukprn })!;
                permissions.Add(pm);
            }
            //permissions.AddRange(ale.Permissions.OrderBy(p => p.ProviderName));

            legalEntity.Permissions = permissions;
            legalEntities.Add(legalEntity);
        }


        var permissionsJustAdded = false;
        string? providerJustAdded = null;
        var sessionModel = _sessionService.Get<TrainingProvidersSessionModel>();
        if (sessionModel?.SuccessfulAddition != null && sessionModel.SuccessfulAddition)
        {
            permissionsJustAdded = true;
            providerJustAdded = sessionModel.ProviderName;
        }

        sessionModel = new TrainingProvidersSessionModel
        {
            AccountLegalEntities = legalEntities
        };
        sessionModel.EmployerAccountId = employerAccountId;
        _sessionService.Set(sessionModel);

        YourTrainingProvidersViewModel model = InitialiseViewModel(legalEntities);
        model.IsOwner = User.IsOwner(employerAccountId);
        model.ShowPermissionsJustAddedBanner = permissionsJustAdded;
        model.ProviderWithChangedPermissions = providerJustAdded;
        model.AddTrainingProviderUrl = Url.RouteUrl(RouteNames.SelectLegalEntity, new { employerAccountId })!;

        return View(model);
    }

    private static YourTrainingProvidersViewModel InitialiseViewModel(List<LegalEntityModel> accountLegalEntities)
    {
        var model = new YourTrainingProvidersViewModel();
        foreach (var legalEntity in accountLegalEntities.Where(x => x.Permissions.Count > 0))
        {
            foreach (var permission in legalEntity.Permissions)
            {

            }
            model.LegalEntities.Add(legalEntity);


        }

        return model;
    }
}
