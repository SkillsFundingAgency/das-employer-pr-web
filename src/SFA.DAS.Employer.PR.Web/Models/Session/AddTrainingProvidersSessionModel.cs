using SFA.DAS.Employer.PR.Domain.Models;

namespace SFA.DAS.Employer.PR.Web.Models.Session;

public class AddTrainingProvidersSessionModel
{
    public long? LegalEntityId { get; set; }
    public string? LegalName { get; set; }
    public string? ProviderName { get; set; }
    public long? Ukprn { get; set; }

    public List<AccountLegalEntity> AccountLegalEntities { get; set; } = [];
}