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
[Route("accounts/{employerAccountId}/providers/changePermissions", Name = RouteNames.ChangePermissions)]
public class ChangePermissionsController(IOuterApiClient _outerApiClient, ISessionService _sessionService) : Controller
{
    [HttpGet]
    public IActionResult Index([FromRoute] string employerAccountId, [FromQuery] long legalEntityId, [FromQuery] long ukprn)
    {
        var sessionModel = _sessionService.Get<TrainingProvidersSessionModel>();
        var accountLegalEntity = sessionModel.AccountLegalEntities.FirstOrDefault(a => a.LegalEntityId == legalEntityId);
        var provider = accountLegalEntity?.Permissions.FirstOrDefault(p => p.Ukprn == ukprn);

        if (sessionModel.EmployerAccountId != employerAccountId && accountLegalEntity != null && provider != null)
        {
            return RedirectToAction("Index", "YourTrainingProviders", new { employerAccountId });
        }


        var backLink = Url.RouteUrl(RouteNames.YourTrainingProviders, new { employerAccountId });
        var cancelLink = Url.RouteUrl(RouteNames.YourTrainingProviders, new { employerAccountId });

        ChangePermissionsViewModel viewModel = new ChangePermissionsViewModel(legalEntityId!,
            accountLegalEntity!.Name, provider!.ProviderName!, provider.Ukprn!, backLink!, cancelLink!);




        // var model = GetViewModel(employerAccountId);
        // if (model == null)
        // {
        //     return RedirectToAction("Index", "YourTrainingProviders", new { employerAccountId });
        // }
        //

        return View(viewModel);
    }
}