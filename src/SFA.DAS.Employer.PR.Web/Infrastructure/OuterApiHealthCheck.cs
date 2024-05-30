using Microsoft.Extensions.Diagnostics.HealthChecks;
using SFA.DAS.Employer.PR.Domain.Interfaces;

namespace SFA.DAS.Employer.PR.Web.Infrastructure;

public class OuterApiHealthCheck(IOuterApiClient _outerApiClient) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var response = await _outerApiClient.Ping();

        return response.IsSuccessStatusCode ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
    }
}
