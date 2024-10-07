using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Common;
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
                var permissionRequestModel = legalEntityModel.Requests.Find(a => a.Ukprn == permissionModel.Ukprn);

                if(permissionRequestModel is not null)
                {
                    permissionModel.ActionLink = MapRequestTypeToRoute(permissionRequestModel.RequestType, permissionRequestModel.RequestId, employerAccountId);
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

        YourTrainingProvidersViewModel yourTrainingProvidersViewModel = new(legalEntityModels)
        {
            IsOwner = User.IsOwner(employerAccountId),
            AddTrainingProviderUrl = Url.RouteUrl(RouteNames.SelectLegalEntity, new { employerAccountId })!
        };

        SetSuccessBanner(yourTrainingProvidersViewModel);

        return yourTrainingProvidersViewModel;
    }

    private string MapRequestTypeToRoute(RequestType requestType, Guid requestId, string employerAccountId)
    {
        string? routeName = requestType switch
        {
            RequestType.Permission => RouteNames.UpdatePermissions,
            RequestType.CreateAccount => RouteNames.CreateAccounts,
            RequestType.AddAccount => RouteNames.AddAccounts,
            _ => null
        };

        return Url.RouteUrl(routeName, new { requestId, employerAccountId })!;
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

        var requestTypeActioned = TempData[TempDataKeys.RequestTypeActioned]?.ToString();
        if (requestTypeActioned != null)
        {
            SetPermissionActionedBannerDetails(model, requestTypeActioned);
            return;
        }

        var nameOfProviderUpdated = TempData[TempDataKeys.NameOfProviderUpdated];
        if (nameOfProviderUpdated != null)
        {
            model.PermissionsUpdatedForProvider = nameOfProviderUpdated.ToString()!.ToUpper();
            model.PermissionsUpdatedForProviderText = $"You've set permissions for {model.PermissionsUpdatedForProvider}";
        }
    }

    private void SetPermissionActionedBannerDetails(YourTrainingProvidersViewModel model, string requestTypeActioned)
    {
        string? providerName = TempData[TempDataKeys.NameOfProviderUpdated]?.ToString()!.ToUpper();
        string requestActionTempValue = TempData[TempDataKeys.RequestAction]!.ToString()!;

        RequestAction requestAction = (RequestAction)Enum.Parse(typeof(RequestAction), requestActionTempValue);
        RequestType requestType = (RequestType)Enum.Parse(typeof(RequestType), requestTypeActioned);

        switch (requestType)
        {
            case RequestType.Permission:
                {
                    model.PermissionsUpdatedForProvider = string.IsNullOrWhiteSpace(providerName) ? null : providerName;
                    model.PermissionsUpdatedForProviderText =
                        requestAction == RequestAction.Accepted ?
                            $"You've set {model.PermissionsUpdatedForProvider}’s permissions." :
                            $"You've declined {model.PermissionsUpdatedForProvider}’s permission request.";
                }
                break;
            case RequestType.AddAccount:
                {
                    model.PermissionsUpdatedForProvider = string.IsNullOrWhiteSpace(providerName) ? null : providerName;
                    model.PermissionsUpdatedForProviderText = $"You've added {model.PermissionsUpdatedForProvider} and set their permissions.";
                }
            break;
        }
    }
}
