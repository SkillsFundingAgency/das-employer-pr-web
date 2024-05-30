using SFA.DAS.Http.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Employer.PR.Web.Infrastructure.Configuration;

[ExcludeFromCodeCoverage]
public class EmployerPROuterApiConfiguration : IApimClientConfiguration
{
    public string ApiBaseUrl { get; set; } = null!;

    public string SubscriptionKey { get; set; } = null!;

    public string ApiVersion { get; set; } = null!;
}