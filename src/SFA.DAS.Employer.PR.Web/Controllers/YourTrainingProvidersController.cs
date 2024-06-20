using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Authentication;
using SFA.DAS.Employer.PR.Web.Extensions;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Models;

namespace SFA.DAS.Employer.PR.Web.Controllers;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
[Route("accounts/{employerAccountId}/providers", Name = RouteNames.YourTrainingProviders)]
public class YourTrainingProvidersController(IOuterApiClient _outerApiClient) : Controller
{
    public async Task<IActionResult> Index([FromRoute] string employerAccountId, CancellationToken cancellationToken)
    {
        var legalEntities = await _outerApiClient.GetAccountLegalEntities(employerAccountId, cancellationToken);
        YourTrainingProvidersViewModel model = InitialiseViewModel(legalEntities);
        model.IsOwner = User.IsOwner(employerAccountId);
        return View(model);
    }

    private static YourTrainingProvidersViewModel InitialiseViewModel(GetEmployerRelationshipsQueryResponse response)
    {
        var model = new YourTrainingProvidersViewModel();
        foreach (var legalEntity in response.AccountLegalEntities.Where(x => x.Permissions.Count > 0))
        {
            model.LegalEntities.Add(legalEntity);
        }

        return model;
    }
}
