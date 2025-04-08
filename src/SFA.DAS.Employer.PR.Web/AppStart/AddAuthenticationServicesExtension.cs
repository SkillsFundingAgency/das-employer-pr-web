using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Authorization;
using SFA.DAS.Employer.PR.Application.Services;
using SFA.DAS.Employer.PR.Web.Authentication;
using SFA.DAS.GovUK.Auth.AppStart;
using SFA.DAS.GovUK.Auth.Authentication;
using SFA.DAS.GovUK.Auth.Employer;
using SFA.DAS.GovUK.Auth.Models;
using SFA.DAS.GovUK.Auth.Services;
using EmployerClaims = SFA.DAS.Employer.PR.Web.Authentication.EmployerClaims;

namespace SFA.DAS.Employer.PR.Web.AppStart;

[ExcludeFromCodeCoverage]
public static class AddAuthenticationServicesExtension
{
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IEmployerAccountAuthorisationHandler, EmployerAccountAuthorisationHandler>();

        services.AddSingleton<IAuthorizationHandler, EmployerAccountAllRolesAuthorizationHandler>();
        services.AddSingleton<IAuthorizationHandler, EmployerAccountOwnerAuthorizationHandler>();

        services.AddAuthorizationBuilder()
            .AddPolicy(PolicyNames.IsAuthenticated, policy =>
                {
                    policy.RequireAuthenticatedUser();
                })
            .AddPolicy(PolicyNames.HasEmployerAccount, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(EmployerClaims.AccountsClaimsTypeIdentifier);
                    policy.Requirements.Add(new EmployerAccountAllRolesRequirement());
                    policy.Requirements.Add(new AccountActiveRequirement());
                })
            .AddPolicy(
                PolicyNames.HasEmployerOwnerAccount, policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.RequireClaim(EmployerClaims.AccountsClaimsTypeIdentifier);
                    policy.Requirements.Add(new EmployerAccountOwnerRequirement());
                    policy.Requirements.Add(new AccountActiveRequirement());
                });

        services.AddAndConfigureGovUkAuthentication(
            configuration,
            new AuthRedirects
            {
                LocalStubLoginPath = "/service/account-details",
                SignedOutRedirectUrl = ""
            },null,
            typeof(EmployerAccountsService));

        return services;
    }
}
