using System.Diagnostics.CodeAnalysis;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Infrastructure.Configuration;

namespace SFA.DAS.Employer.PR.Web.AppStart;

[ExcludeFromCodeCoverage]
public static class AddHealthChecksExtension
{
    public static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        ApplicationSettings config = configuration.GetSection(nameof(ApplicationSettings)).Get<ApplicationSettings>()!;
        services
            .AddHealthChecks()
            .AddCheck<OuterApiHealthCheck>("Outer api health check")
            .AddRedis(config.RedisConnectionString, "Redis health check");
        return services;
    }
}
