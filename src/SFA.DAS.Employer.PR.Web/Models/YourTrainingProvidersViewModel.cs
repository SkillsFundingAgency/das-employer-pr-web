using SFA.DAS.Employer.PR.Domain.Models;

namespace SFA.DAS.Employer.PR.Web.Models;

public class YourTrainingProvidersViewModel
{
    public YourTrainingProvidersViewModel() { }

    public YourTrainingProvidersViewModel(List<LegalEntityModel> legalEntities)
    {
        LegalEntities = legalEntities;
    }

    public bool IsOwner { get; set; }
    public string? PermissionsUpdatedForProvider { get; set; }
    public string? PermissionsUpdatedForProviderText { get; set; }
    public bool ShowPermissionsUpdatedBanner() => !string.IsNullOrWhiteSpace(PermissionsUpdatedForProvider);
    public List<LegalEntityModel> LegalEntities { get; set; } = [];
    public string AddTrainingProviderUrl { get; set; } = null!;
}