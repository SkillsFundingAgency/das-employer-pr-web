using SFA.DAS.Employer.PR.Domain.Models;

namespace SFA.DAS.Employer.PR.Web.Models.Cache;

public class RegisteredProvidersCache
{
    public List<RegisteredProvider> RegisteredProviders { get; set; } = new List<RegisteredProvider>();
}