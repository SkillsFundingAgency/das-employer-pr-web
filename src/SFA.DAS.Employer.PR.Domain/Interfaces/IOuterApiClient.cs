using RestEase;
using SFA.DAS.Employer.PR.Domain.OuterApi.Permissions;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;

namespace SFA.DAS.Employer.PR.Domain.Interfaces;

public interface IOuterApiClient
{
    [Get("/serviceCheck")]
    Task<HttpResponseMessage> ServiceCheck();

    [Get("/accountusers/{userId}/accounts")]
    Task<GetEmployerUserAccountsResponse> GetUserAccounts([Path] string userId, [Query] string email, CancellationToken cancellationToken);

    [Get("/relationships/{accountId}")]
    Task<GetEmployerRelationshipsQueryResponse> GetEmployerRelationships([Path] long accountId, CancellationToken cancellationToken);

    [Get("/providers")]
    Task<GetRegisteredProvidersResponse> GetRegisteredProviders(CancellationToken cancellationToken);

    [Get("/permissions")]
    [AllowAnyStatusCode]
    Task<Response<GetPermissionsResponse>> GetPermissions([Query] long ukprn, [Query] long AccountLegalEntityId, CancellationToken cancellationToken);

    [Post("/permissions")]
    Task PostPermissions([Body] PostPermissionsCommand command, CancellationToken cancellationToken);
}
