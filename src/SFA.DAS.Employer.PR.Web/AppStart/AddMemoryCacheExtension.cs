using SFA.DAS.Employer.PR.Web.Infrastructure.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Employer.PR.Web.AppStart;

[ExcludeFromCodeCoverage]
public static class AddMemoryCacheExtension
{
    public static IServiceCollection AddDistributedCache(this IServiceCollection services, IConfiguration configuration)
    {
        if (configuration["EnvironmentName"]!.Equals("LOCAL", StringComparison.CurrentCultureIgnoreCase))
        {
            services.AddDistributedMemoryCache();
        }
        else
        {
            services.AddStackExchangeRedisCache(options =>
            {
                var applicationConfiguration = configuration.GetSection(nameof(ApplicationSettings)).Get<ApplicationSettings>();
                options.Configuration = applicationConfiguration!.RedisConnectionString;
            });
        }

        return services;
    }
}