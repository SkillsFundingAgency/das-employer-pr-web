using SFA.DAS.Configuration.AzureTableStorage;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Employer.PR.Web.AppStart;

[ExcludeFromCodeCoverage]
public static class LoadConfigurationExtension
{
    private const string EncodingConfigKey = "SFA.DAS.Encoding";


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
            options.ConfigurationKeysRawJsonResult = new[] { EncodingConfigKey };
        });

        var configuration = configBuilder.Build();

        return configuration;
    }
}