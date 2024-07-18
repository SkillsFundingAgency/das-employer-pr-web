using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Web.Authentication;
using SFA.DAS.Employer.PR.Web.Constants;
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
        _sessionService.Delete<AddTrainingProvidersSessionModel>();

        var response = await _outerApiClient.GetAccountLegalEntities(employerAccountId, cancellationToken);
        var accountLegalEntities = response.AccountLegalEntities.OrderBy(a => a.Name);

        var legalEntityModels = OrderPermissionsAndAddLinks(employerAccountId, accountLegalEntities);

        YourTrainingProvidersViewModel model = InitialiseViewModel(legalEntityModels);
        model.IsOwner = User.IsOwner(employerAccountId);
        SetSuccessBanner(model);

        model.AddTrainingProviderUrl = Url.RouteUrl(RouteNames.SelectLegalEntity, new { employerAccountId })!;

        return View(model);
    }

    private void SetSuccessBanner(YourTrainingProvidersViewModel model)
    {
        var nameOfProviderAdded = TempData[TempDataKeys.NameOfProviderAdded];
        if (nameOfProviderAdded != null)
        {
            model.PermissionsUpdatedForProvider = nameOfProviderAdded.ToString()!.ToUpper();
            model.PermissionsUpdatedForProviderText =
                $"You've added {model.PermissionsUpdatedForProvider} and set their permissions.";
            return;
        }

        var nameOfProviderUpdated = TempData[TempDataKeys.NameOfProviderUpdated];

        if (nameOfProviderUpdated != null)
        {
            model.PermissionsUpdatedForProvider = nameOfProviderUpdated.ToString()!.ToUpper();
            model.PermissionsUpdatedForProviderText =
                $"You've set permissions for {model.PermissionsUpdatedForProvider}";
        }
    }

    private List<LegalEntityModel> OrderPermissionsAndAddLinks(string employerAccountId, IOrderedEnumerable<AccountLegalEntity> accountLegalEntities)
    {
        var legalEntityModels = new List<LegalEntityModel>();

        foreach (var ale in accountLegalEntities)
        {
            var legalEntity = (LegalEntityModel)ale;

            var permissions = new List<PermissionModel>();

            foreach (var p in ale.Permissions.OrderBy(p => p.ProviderName))
            {
                var pm = (PermissionModel)p;
                pm.ChangePermissionsLink = Url.RouteUrl(RouteNames.ChangePermissions,
                    new { employerAccountId, legalEntity.LegalEntityId, pm.Ukprn })!;
                permissions.Add(pm);
            }

            legalEntity.Permissions = permissions;
            legalEntityModels.Add(legalEntity);
        }

        return legalEntityModels;
    }

    private static YourTrainingProvidersViewModel InitialiseViewModel(List<LegalEntityModel> accountLegalEntities)
    {
        var model = new YourTrainingProvidersViewModel();
        foreach (var legalEntity in accountLegalEntities.Where(x => x.Permissions.Count > 0))
        {
            model.LegalEntities.Add(legalEntity);
        }

        return model;
    }
}
