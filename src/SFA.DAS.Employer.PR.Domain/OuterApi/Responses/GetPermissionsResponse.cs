using SFA.DAS.Employer.PR.Domain.Models;

namespace SFA.DAS.Employer.PR.Domain.OuterApi.Responses;

public class GetPermissionsResponse
{
    public List<Operation> Operations { get; set; } = [];
    public string ProviderName { get; set; } = null!;
    public string AccountLegalEntityName { get; set; } = null!;
}