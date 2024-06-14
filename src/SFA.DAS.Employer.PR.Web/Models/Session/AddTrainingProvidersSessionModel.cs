using SFA.DAS.Employer.PR.Domain.Models;

namespace SFA.DAS.Employer.PR.Web.Models.Session;

public class AddTrainingProvidersSessionModel
{
    public string EmployerAccountId { get; set; } = null!;
    public long? SelectedLegalEntityId { get; set; }
    public string? SelectedLegalName { get; set; }
    public string? ProviderName { get; set; }
    public long? Ukprn { get; set; }

    public List<AccountLegalEntity> AccountLegalEntities { get; set; } = [];
    public string? PermissionToAddCohorts { get; set; }
    public string? PermissionToRecruit { get; set; }

    public bool SuccessfulAddition { get; set; }
}