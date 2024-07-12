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

        var legalEntities = OrderPermissionsByProviderName(accountLegalEntities);

        YourTrainingProvidersViewModel model = InitialiseViewModel(legalEntities);
        model.IsOwner = User.IsOwner(employerAccountId);
        model.PermissionsUpdatedForProvider = TempData[TempDataKeys.NameOfProviderAdded]?.ToString();
        model.AddTrainingProviderUrl = Url.RouteUrl(RouteNames.SelectLegalEntity, new { employerAccountId })!;

        return View(model);
    }

    private static List<AccountLegalEntity> OrderPermissionsByProviderName(IEnumerable<AccountLegalEntity> accountLegalEntities)
    {
        var legalEntities = new List<AccountLegalEntity>();

        foreach (var ale in accountLegalEntities)
        {
            var permissions = new List<Permission>();
            permissions.AddRange(ale.Permissions.OrderBy(p => p.ProviderName));
            ale.Permissions = permissions;
            legalEntities.Add(ale);
        }

        return legalEntities;
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
