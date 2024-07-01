namespace SFA.DAS.Employer.PR.Web.Models;

public class AddPermissionsShutterPageViewModel
{
    public string ProviderName { get; }
    public long Ukprn { get; }
    public string SetPermissionsLink { get; }
    public string ReturnToYourTrainingProvidersLink { get; }

    public AddPermissionsShutterPageViewModel(string providerName, long ukprn, string setPermissionsLink, string returnToYourTrainingProvidersLink)
    {
        ProviderName = providerName;
        Ukprn = ukprn;
        SetPermissionsLink = setPermissionsLink;
        ReturnToYourTrainingProvidersLink = returnToYourTrainingProvidersLink;
    }
}
