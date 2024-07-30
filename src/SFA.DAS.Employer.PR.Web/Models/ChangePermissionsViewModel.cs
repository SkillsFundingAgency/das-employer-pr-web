namespace SFA.DAS.Employer.PR.Web.Models;

public class ChangePermissionsViewModel : ChangePermissionsSubmitViewModel
{
    public string BackLink { get; set; }
    public string CancelLink { get; set; }
    public string ProviderName { get; set; }
    public string LegalName { get; set; }

    public ChangePermissionsViewModel(string backLink, string permissionToAddCohorts, string permissionToRecruit, string providerName, string legalName, long legalEntityId, long ukprn)
    {
        LegalEntityId = legalEntityId;
        LegalName = legalName;
        ProviderName = providerName;
        Ukprn = ukprn;
        BackLink = backLink;
        CancelLink = backLink;
        PermissionToAddCohorts = permissionToAddCohorts;
        PermissionToAddCohortsOriginal = permissionToAddCohorts;
        PermissionToRecruit = permissionToRecruit;
        PermissionToRecruitOriginal = permissionToRecruit;
    }
}

public class ChangePermissionsSubmitViewModel : PermissionDescriptionsViewModel
{
    public string PermissionToAddCohortsOriginal { get; set; } = null!;
    public string PermissionToRecruitOriginal { get; set; } = null!;
    public long LegalEntityId { get; set; } = 0;
    public long Ukprn { get; set; } = 0;
}