using SFA.DAS.Employer.PR.Domain.Models;

namespace SFA.DAS.Employer.PR.Web.Models.Session;

public class AddTrainingProvidersSessionModel
{
    public string? EmployerAccountId { get; set; } = null!;
    public long? LegalEntityId { get; set; }
    public string? LegalName { get; set; }
    public string? ProviderName { get; set; }
    public long? Ukprn { get; set; }

    public List<AccountLegalEntity> AccountLegalEntities { get; set; } = [];
    public string? AddRecords { get; set; }
    public string? RecruitApprentices { get; set; }

    public bool SuccessfulAddition { get; set; }
}