﻿namespace SFA.DAS.Employer.PR.Web.Models;

public class ChangePermissionsViewModel : ChangePermissionsSubmitModel
{
    public string BackLink { get; set; }
    public string ProviderName { get; set; }
    public string LegalName { get; set; }

    public ChangePermissionsViewModel(string backLink, string permissionToAddCohorts, string permissionToRecruit, string providerName, string legalName, long legalEntityId, long ukprn)
    {
        LegalEntityId = legalEntityId;
        LegalName = legalName;
        ProviderName = providerName;
        Ukprn = ukprn;
        BackLink = backLink;
        PermissionToAddCohorts = permissionToAddCohorts;
        PermissionToAddCohortsOriginal = permissionToAddCohorts;
        PermissionToRecruit = permissionToRecruit;
        PermissionToRecruitOriginal = permissionToRecruit;
    }
}

public class ChangePermissionsSubmitModel : PermissionDescriptionsModel
{
    public string PermissionToAddCohortsOriginal { get; set; } = null!;
    public string PermissionToRecruitOriginal { get; set; } = null!;
    public long LegalEntityId { get; set; } = 0;
    public long Ukprn { get; set; } = 0;
}
