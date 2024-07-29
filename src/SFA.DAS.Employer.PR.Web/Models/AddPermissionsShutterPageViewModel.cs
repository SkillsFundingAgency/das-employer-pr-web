namespace SFA.DAS.Employer.PR.Web.Models;

public class AddPermissionsShutterPageViewModel
{
    public string ProviderName { get; }
    public long Ukprn { get; }
    public string LegalEntityPublicHashedId { get; }
    public string ChangePermissionsLink { get; }
    public string ReturnToYourTrainingProvidersLink { get; }

    public AddPermissionsShutterPageViewModel(string providerName, long ukprn, string legalEntityPublicHashedId, string changePermissionsLink, string returnToYourTrainingProvidersLink)
    {
        ProviderName = providerName;
        Ukprn = ukprn;
        LegalEntityPublicHashedId = legalEntityPublicHashedId;
        ChangePermissionsLink = changePermissionsLink;
        ReturnToYourTrainingProvidersLink = returnToYourTrainingProvidersLink;
    }
}
