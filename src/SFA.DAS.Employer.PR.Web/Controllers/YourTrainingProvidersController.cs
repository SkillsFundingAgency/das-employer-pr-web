using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.Models;
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
        _sessionService.Delete<AddTrainingProvidersSessionModel>();

        var response = await _outerApiClient.GetAccountLegalEntities(employerAccountId, cancellationToken);
        var accountLegalEntities = response.AccountLegalEntities.OrderBy(a => a.Name);

        var legalEntities = new List<AccountLegalEntity>();

        foreach (var ale in accountLegalEntities)
        {
            var permissions = new List<Permission>();
            permissions.AddRange(ale.Permissions.OrderBy(p => p.ProviderName));
            ale.Permissions = permissions;
            legalEntities.Add(ale);
        }


        var permissionsJustAdded = false;
        string? providerJustAdded = null;
        var sessionModel = _sessionService.Get<AddTrainingProvidersSessionModel>();
        if (sessionModel?.SuccessfulAddition != null && sessionModel.SuccessfulAddition)
        {
            permissionsJustAdded = true;
            providerJustAdded = sessionModel.ProviderName;
        }

        sessionModel = new AddTrainingProvidersSessionModel
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

    private static YourTrainingProvidersViewModel InitialiseViewModel(List<AccountLegalEntity> accountLegalEntities)
    {
        var model = new YourTrainingProvidersViewModel();
        foreach (var legalEntity in accountLegalEntities.Where(x => x.Permissions.Count > 0))
        {
            model.LegalEntities.Add(legalEntity);
        }

        return model;
    }
}
