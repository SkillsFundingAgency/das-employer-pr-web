﻿using Microsoft.AspNetCore.Authorization;
using SFA.DAS.Employer.PR.Web.Authentication;
using SFA.DAS.GovUK.Auth.AppStart;
using SFA.DAS.GovUK.Auth.Authentication;
using SFA.DAS.GovUK.Auth.Services;
using System.Diagnostics.CodeAnalysis;

namespace SFA.DAS.Employer.PR.Web.AppStart;

[ExcludeFromCodeCoverage]
public static class AddAuthenticationServicesExtension
{
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddTransient<ICustomClaims, EmployerAccountPostAuthenticationClaimsHandler>();

        services.AddSingleton<IAuthorizationHandler, EmployerAccountAuthorizationHandler>();
        services.AddSingleton<IAuthorizationHandler, EmployerOwnerAuthorizationHandler>();

        services.Configure<IISServerOptions>(options => options.AutomaticAuthentication = false);

        services.AddAuthorizationBuilder()
            .AddPolicy(PolicyNames.IsAuthenticated, policy =>
                {
                    policy.RequireAuthenticatedUser();
                })
            .AddPolicy(PolicyNames.HasEmployerAccount, policy =>
                {
                    policy.RequireClaim(EmployerClaims.AccountsClaimsTypeIdentifier);
                    policy.Requirements.Add(new EmployerAccountRequirement());
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new AccountActiveRequirement());
                })
            .AddPolicy(
                PolicyNames.HasEmployerOwnerAccount, policy =>
                {
                    policy.RequireClaim(EmployerClaims.AccountsClaimsTypeIdentifier);
                    policy.Requirements.Add(new EmployerOwnerRoleRequirement());
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new AccountActiveRequirement());
                });

        services.AddAndConfigureGovUkAuthentication(
            configuration,
            typeof(EmployerAccountPostAuthenticationClaimsHandler),
            string.Empty,
            "/service/account-details");
        return services;
    }
}