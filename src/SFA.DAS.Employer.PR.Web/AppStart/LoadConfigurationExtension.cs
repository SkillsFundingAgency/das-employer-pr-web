using System.Diagnostics.CodeAnalysis;
using SFA.DAS.Configuration.AzureTableStorage;

namespace SFA.DAS.Employer.PR.Web.AppStart;

[ExcludeFromCodeCoverage]
public static class LoadConfigurationExtension
{
    public static IConfigurationRoot LoadConfiguration(this IConfiguration config, IServiceCollection services)
    {
        var configBuilder = new ConfigurationBuilder()
            .AddConfiguration(config)
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddEnvironmentVariables();

        configBuilder.AddAzureTableStorage(options =>
        {
            options.ConfigurationKeys = config["ConfigNames"]!.Split(",");
            options.StorageConnectionString = config["ConfigurationStorageConnectionString"];
            options.EnvironmentName = config["EnvironmentName"];
            options.PreFixConfigurationKeys = false;
        });

        var configuration = configBuilder.Build();

        return configuration;
    }
}