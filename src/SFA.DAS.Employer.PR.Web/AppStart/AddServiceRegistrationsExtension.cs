﻿using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using RestEase.HttpClientFactory;
using SFA.DAS.Employer.PR.Application.Services;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Web.Infrastructure.Configuration;
using SFA.DAS.Employer.PR.Web.Infrastructure.Services;
using SFA.DAS.Employer.PR.Web.Services;
using SFA.DAS.Encoding;
using SFA.DAS.Http.Configuration;

namespace SFA.DAS.Employer.PR.Web.AppStart;

[ExcludeFromCodeCoverage]
public static class AddServiceRegistrationsExtension
{
    public const string EncodingConfigKey = "SFA.DAS.Encoding";

    public static IServiceCollection AddServiceRegistrations(this IServiceCollection services, IConfigurationRoot configuration)
    {
        var outerApiConfiguration = configuration.GetSection(nameof(EmployerPROuterApiConfiguration)).Get<EmployerPROuterApiConfiguration>();
        AddOuterApi(services, outerApiConfiguration!);

        var encodingsConfiguration = configuration.GetSection(EncodingConfigKey).Value;

        var encodingConfig = JsonSerializer.Deserialize<EncodingConfig>(encodingsConfiguration!);
        services.AddSingleton(encodingConfig!);

        services.AddTransient<ISessionService, SessionService>();
        services.AddTransient<IEncodingService, EncodingService>();
        services.AddTransient<ICacheStorageService, CacheStorageService>();
        services.AddTransient<IAccountsLinkService, AccountsLinkService>();

        return services;
    }

    private static void AddOuterApi(this IServiceCollection services, EmployerPROuterApiConfiguration configuration)
    {
        services.AddTransient<IApimClientConfiguration>((_) => configuration);

        services.AddScoped<Http.MessageHandlers.DefaultHeadersHandler>();
        services.AddScoped<Http.MessageHandlers.LoggingMessageHandler>();
        services.AddScoped<Http.MessageHandlers.ApimHeadersHandler>();

        services
            .AddRestEaseClient<IOuterApiClient>(configuration.ApiBaseUrl)
            .AddHttpMessageHandler<Http.MessageHandlers.DefaultHeadersHandler>()
            .AddHttpMessageHandler<Http.MessageHandlers.ApimHeadersHandler>()
            .AddHttpMessageHandler<Http.MessageHandlers.LoggingMessageHandler>();
    }
}
