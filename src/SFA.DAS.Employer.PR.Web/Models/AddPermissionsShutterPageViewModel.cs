namespace SFA.DAS.Employer.PR.Web.Models;

public class AddPermissionsShutterPageViewModel
{
    public string ProviderName { get; }
    public long Ukprn { get; }
    public long LegalEntityId { get; }
    public string ChangePermissionsLink { get; }
    public string ReturnToYourTrainingProvidersLink { get; }

    public AddPermissionsShutterPageViewModel(string providerName, long ukprn, long legalEntityId, string changePermissionsLink, string returnToYourTrainingProvidersLink)
    {
        ProviderName = providerName;
        Ukprn = ukprn;
        LegalEntityId = legalEntityId;
        ChangePermissionsLink = changePermissionsLink;
        ReturnToYourTrainingProvidersLink = returnToYourTrainingProvidersLink;
    }
}
