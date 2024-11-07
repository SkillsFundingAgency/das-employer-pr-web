namespace SFA.DAS.Employer.PR.Web.Models.Requests;

public record AccountCreatedConfirmationViewModel(string AccountsHomeUrl, string ProviderName, string ViewAgreementUrl, string ManageProvidersUrl)
{
    public string ProviderNameInUpperCase => ProviderName.ToUpper();
}

