namespace SFA.DAS.Employer.PR.Web.Models;

public class AddPermissionsViewModel : AddPermissionsSubmitViewViewModel
{
    public string BackLink { get; set; }
    public string CancelLink { get; set; }
    public long LegalEntityId { get; set; }
    public string LegalName { get; set; }
    public string ProviderName { get; set; }
    public long Ukprn { get; set; }

    public AddPermissionsViewModel(long legalEntityId, string legalName, string providerName, long ukprn, string backLink, string cancelLink)
    {
        LegalEntityId = legalEntityId;
        LegalName = legalName;
        ProviderName = providerName;
        Ukprn = ukprn;
        BackLink = backLink;
        CancelLink = cancelLink;
    }
}

public class AddPermissionsSubmitViewViewModel : PermissionDescriptionsViewModel
{
}