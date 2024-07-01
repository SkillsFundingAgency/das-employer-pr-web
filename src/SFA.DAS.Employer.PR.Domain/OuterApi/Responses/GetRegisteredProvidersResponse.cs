using SFA.DAS.Employer.PR.Domain.Models;

namespace SFA.DAS.Employer.PR.Domain.OuterApi.Responses;

public class GetRegisteredProvidersResponse
{
    public List<RegisteredProvider> Providers { get; set; } = [];
}