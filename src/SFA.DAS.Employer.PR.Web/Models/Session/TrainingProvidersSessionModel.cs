namespace SFA.DAS.Employer.PR.Web.Models.Session;

public class TrainingProvidersSessionModel
{
    public string? EmployerAccountId { get; set; } = null!;
    public long? LegalEntityId { get; set; }
    public string? LegalName { get; set; }
    public string? ProviderName { get; set; }
    public long? Ukprn { get; set; }

    public List<LegalEntityModel> AccountLegalEntities { get; set; } = [];
    public string? AddRecords { get; set; }
    public string? RecruitApprentices { get; set; }

    public bool SuccessfulAddition { get; set; }
}