namespace SFA.DAS.Employer.PR.Domain.Models;

public class ProviderPermission
{
    public long Ukprn { get; set; }
    public string ProviderName { get; set; } = null!;
    public List<Operation> Operations { get; set; } = null!;
}