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
using System.Configuration.Provider;
using System.Reflection;

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
        List<LegalEntityModel> legalEntityModels = new List<LegalEntityModel>();

        foreach (LegalEntity legalEntity in accountLegalEntities)
        {
            HashSet<long> addedUkprns = [];

            LegalEntityModel legalEntityModel = (LegalEntityModel)legalEntity;

            foreach (var permissions in legalEntity.Permissions)
            {
                addedUkprns.Add(permissions.Ukprn);

                var outstandingRequest = legalEntity.Requests.FirstOrDefault(a => a.Ukprn == permissions.Ukprn);

                var permissionDetailsModel = CreatePermissionDetailsModel(
                    permissions.Ukprn, 
                    permissions.ProviderName, 
                    legalEntity, 
                    permissions.Operations.ToArray(), 
                    employerAccountId,
                    outstandingRequest is not null
                );

                if (outstandingRequest is not null)
                {
                    SetViewRequestNavigationLink(ref permissionDetailsModel, outstandingRequest, employerAccountId);
                }
                else
                {
                    SetChangePermissionsNavigationLink(ref permissionDetailsModel, employerAccountId, legalEntity.PublicHashedId);
                }

                legalEntityModel.PermissionDetails.Add(permissionDetailsModel);
            }

            foreach (var request in legalEntity.Requests.Where(a => !addedUkprns.Contains(a.Ukprn)))
            {
                PermissionDetailsModel permissionDetailsModel = CreatePermissionDetailsModel(
                    request.Ukprn,
                    request.ProviderName,
                    legalEntity,
                    request.Operations.ToArray(),
                    employerAccountId,
                    true
                );

                SetViewRequestNavigationLink(ref permissionDetailsModel, request, employerAccountId);

                legalEntityModel.PermissionDetails.Add(permissionDetailsModel);
            }

            if (legalEntityModel.PermissionDetails.Count > 0)
            {
                legalEntityModel.PermissionDetails = legalEntityModel.PermissionDetails.OrderBy(a => a.ProviderName).ToList();
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

    private PermissionDetailsModel CreatePermissionDetailsModel(long ukprn, string providerName, LegalEntity legalEntity, Operation[] operations, string employerAccountId, bool hasOutstandingRequest = false)
    {
        var permissionDetailsModel = new PermissionDetailsModel
        {
            Ukprn = ukprn,
            ProviderName = providerName,
            HasOutstandingRequest = hasOutstandingRequest
        };

        SetPermissions(ref permissionDetailsModel, operations);

        return permissionDetailsModel;
    }

    public void SetViewRequestNavigationLink(ref PermissionDetailsModel permissionDetails, PermissionRequest permissionRequest, string employerAccountId)
    {
        permissionDetails.ActionLink = MapRequestTypeToRoute(permissionRequest.RequestType, permissionRequest.RequestId, employerAccountId);
        permissionDetails.ActionLinkText = YourTrainingProviders.ViewRequestActionText;
    }

    public void SetChangePermissionsNavigationLink(ref PermissionDetailsModel permissionDetails, string employerAccountId, string LegalEntityPublicHashedId)
    {
        permissionDetails.ActionLink = Url.RouteUrl(
            RouteNames.ChangePermissions, 
            new { 
                employerAccountId, 
                LegalEntityPublicHashedId,
                permissionDetails.Ukprn
            }
        )!;
        permissionDetails.ActionLinkText = YourTrainingProviders.ChangePermissionsActionText;
    }

    private void SetPermissions(ref PermissionDetailsModel permissionDetails, Operation[] operations)
    {
        permissionDetails.PermissionToAddRecords = operations.Any(x => x == Operation.CreateCohort) ?
            ManageRequests.YesWithEmployerRecordReview :
            ManageRequests.No;

        permissionDetails.PermissionToRecruitApprentices = operations.Any(x => x == Operation.Recruitment) ? ManageRequests.Yes :
            operations.Any(x => x == Operation.RecruitmentRequiresReview) ?
                ManageRequests.YesWithEmployerAdvertReview :
                ManageRequests.No;
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
            var requestActionTempValue = TempData[TempDataKeys.RequestAction];
            if(requestActionTempValue != null)
            {
                SetPermissionActionedBannerDetails(model, requestTypeActioned, requestActionTempValue.ToString()!);
            }
                
            return;
        }

        var nameOfProviderUpdated = TempData[TempDataKeys.NameOfProviderUpdated];
        if (nameOfProviderUpdated != null)
        {
            model.PermissionsUpdatedForProvider = nameOfProviderUpdated.ToString()!.ToUpper();
            model.PermissionsUpdatedForProviderText = $"You've set permissions for {model.PermissionsUpdatedForProvider}";
        }
    }

    private void SetPermissionActionedBannerDetails(YourTrainingProvidersViewModel model, string requestTypeActioned, string requestAction)
    {
        string? providerName = TempData[TempDataKeys.NameOfProviderUpdated]?.ToString()!.ToUpper();
  
        RequestAction requestActionEnum = Enum.Parse<RequestAction>(requestAction);
        RequestType requestType = Enum.Parse<RequestType>(requestTypeActioned);

        switch (requestType)
        {
            case RequestType.Permission:
                {
                    model.PermissionsUpdatedForProvider = string.IsNullOrWhiteSpace(providerName) ? null : providerName;
                    model.PermissionsUpdatedForProviderText =
                        requestActionEnum == RequestAction.Accepted ?
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
