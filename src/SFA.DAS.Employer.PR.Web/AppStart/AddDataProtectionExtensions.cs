using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.DataProtection;
using SFA.DAS.Employer.PR.Web.Infrastructure.Configuration;
using SFA.DAS.Employer.PR.Web.Infrastructure.DataProtection;
using StackExchange.Redis;

namespace SFA.DAS.Employer.PR.Web.AppStart;

[ExcludeFromCodeCoverage]
public static class AddDataProtectionExtensions
{
    private const string ApplicationName = "das-employer";

    public static IServiceCollection AddDataProtection(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        var applicationSettings = configuration.GetSection(nameof(ApplicationSettings)).Get<ApplicationSettings>()!;

        if (isDevelopment)
        {
            services
                .AddDataProtection()
                .SetApplicationName(ApplicationName)
                .PersistKeysToFileSystem(new DirectoryInfo(@"C:\Esfa\SharedKeys"));
        }
        else
        {
            var redisConnectionString = applicationSettings.RedisConnectionString;
            var dataProtectionKeysDatabase = applicationSettings.DataProtectionKeysDatabase;

            var redis = ConnectionMultiplexer.Connect($"{redisConnectionString},{dataProtectionKeysDatabase}");

            services.AddDataProtection()
                .SetApplicationName(ApplicationName)
                .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys");
        }

        services.AddTransient<IDataProtectorServiceFactory, DataProtectorServiceFactory>();

        return services;
    }
}
