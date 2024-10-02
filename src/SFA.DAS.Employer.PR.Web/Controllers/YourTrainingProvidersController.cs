using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Constants;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Authentication;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Extensions;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Infrastructure.Services;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Employer.PR.Web.Models.Session;
using SFA.DAS.Encoding;

namespace SFA.DAS.Employer.PR.Web.Controllers;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
[Route("accounts/{employerAccountId}/providers", Name = RouteNames.YourTrainingProviders)]
public class YourTrainingProvidersController(IOuterApiClient _outerApiClient, ISessionService _sessionService, IEncodingService _encodingService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index([FromRoute] string employerAccountId, CancellationToken cancellationToken)
    {
        _sessionService.Delete<AddTrainingProvidersSessionModel>();

        long accountId = _encodingService.Decode(employerAccountId, EncodingType.AccountId);

        GetEmployerRelationshipsQueryResponse response = await _outerApiClient.GetEmployerRelationships(accountId, cancellationToken);

        IOrderedEnumerable<LegalEntity> accountLegalEntities = response.AccountLegalEntities.OrderBy(a => a.Name);

        YourTrainingProvidersViewModel model = PopulateYourTrainingProvidersViewModel(employerAccountId, accountLegalEntities);

        return View(model);
    }

    private YourTrainingProvidersViewModel PopulateYourTrainingProvidersViewModel(string employerAccountId, IOrderedEnumerable<LegalEntity> accountLegalEntities)
    {
        var legalEntityModels = new List<LegalEntityModel>();

        foreach (var legalEntityModel in accountLegalEntities.Select(ale => (LegalEntityModel)ale))
        {
            var orderedPermissions = new List<PermissionModel>();

            foreach (var permissionModel in legalEntityModel.Permissions.OrderBy(p => p.ProviderName))
            {
                var providerRequestModel = legalEntityModel.Requests.Find(a => a.Ukprn == permissionModel.Ukprn);

                if(providerRequestModel is not null)
                {
                    permissionModel.ActionLink = Url.RouteUrl(RouteNames.Requests, new { requestId = providerRequestModel?.RequestId })!;
                    permissionModel.ActionLinkText = YourTrainingProviders.ViewRequestActionText;
                }
                else
                {
                    permissionModel.ActionLink = Url.RouteUrl(RouteNames.ChangePermissions, new { employerAccountId, legalEntityModel.LegalEntityPublicHashedId, permissionModel.Ukprn })!;
                    permissionModel.ActionLinkText = YourTrainingProviders.ChangePermissionsActionText;
                }

                orderedPermissions.Add(permissionModel);
            }

            legalEntityModel.Permissions = orderedPermissions;

            if(legalEntityModel.Permissions.Count > 0)
            {
                legalEntityModels.Add(legalEntityModel);
            }
        }

        YourTrainingProvidersViewModel yourTrainingProvidersViewModel = new YourTrainingProvidersViewModel(legalEntityModels)
        {
            IsOwner = User.IsOwner(employerAccountId),
            AddTrainingProviderUrl = Url.RouteUrl(RouteNames.SelectLegalEntity, new { employerAccountId })!
        };

        SetSuccessBanner(yourTrainingProvidersViewModel);

        return yourTrainingProvidersViewModel;
    }

    private void SetSuccessBanner(YourTrainingProvidersViewModel model)
    {
        var nameOfProviderAdded = TempData[TempDataKeys.NameOfProviderAdded]?.ToString()!.ToUpper();
        if (nameOfProviderAdded != null)
        {
            model.PermissionsUpdatedForProvider = nameOfProviderAdded;
            model.PermissionsUpdatedForProviderText = $"You've added {model.PermissionsUpdatedForProvider} and set their permissions.";
            return;
        }

        var nameOfProviderUpdated = TempData[TempDataKeys.NameOfProviderUpdated]?.ToString()!.ToUpper();

        if (nameOfProviderUpdated != null)
        {
            model.PermissionsUpdatedForProvider = nameOfProviderUpdated;
            model.PermissionsUpdatedForProviderText = $"You've set permissions for {model.PermissionsUpdatedForProvider}";
        }
    }
}
