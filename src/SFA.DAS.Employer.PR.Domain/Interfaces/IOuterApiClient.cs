﻿using RestEase;
using SFA.DAS.Employer.PR.Domain.OuterApi.Requests;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;

namespace SFA.DAS.Employer.PR.Domain.Interfaces;

public interface IOuterApiClient
{
    [Get("/serviceCheck")]
    Task<HttpResponseMessage> ServiceCheck();

    [Get("/accountusers/{userId}/accounts")]
    Task<GetEmployerUserAccountsResponse> GetUserAccounts([Path] string userId, [Query] string email, CancellationToken cancellationToken);

    [Get("/employeraccounts/{accountId}/legalEntities")]
    Task<GetAccountLegalEntitiesResponse> GetAccountLegalEntities([Path] long accountId, CancellationToken cancellationToken);

    [Get("/relationships/{accountId}")]
    Task<GetEmployerRelationshipsQueryResponse> GetEmployerRelationships([Path] long accountId, CancellationToken cancellationToken);

    [Get("/providers")]
    Task<GetRegisteredProvidersResponse> GetRegisteredProviders(CancellationToken cancellationToken);

    [Get("/permissions")]
    [AllowAnyStatusCode]
    Task<Response<GetPermissionsResponse>> GetPermissions([Query] long ukprn, [Query] long AccountLegalEntityId, CancellationToken cancellationToken);

    [Post("/permissions")]
    Task PostPermissions([Body] PostPermissionsRequest command, CancellationToken cancellationToken);

    [Get("/requests/{requestId}/createaccount/validate")]
    Task<ValidateCreateAccountRequestResponse> ValidateCreateAccountRequest([Path] Guid requestId, CancellationToken cancellationToken);

    [Post("/requests/{requestId}/createaccount/accepted")]
    Task<AcceptCreateAccountResponse> AcceptCreateAccountRequest([Path] Guid requestId, [Body] AcceptCreateAccountRequest body, CancellationToken cancellationToken);

    [Get("/requests/{requestId}")]
    Task<GetPermissionRequestResponse?> GetRequest([Path] Guid requestId, CancellationToken cancellationToken);

    [Post("/requests/{requestId}/permission/accepted")]
    Task AcceptPermissionsRequest([Path] Guid requestId, [Body] AcceptPermissionsRequest request, CancellationToken cancellationToken);

    [Post("/requests/{requestId}/addaccount/accepted")]
    Task AcceptAddAccountRequest([Path] Guid requestId, [Body] AcceptAddAccountRequest request, CancellationToken cancellationToken);

    [Post("/requests/{requestId}/addaccount/declined")]
    Task DeclineAddAccountRequest([Path] Guid requestId, [Body] DeclinePermissionsRequest model, CancellationToken cancellationToken);

    [Post("/requests/{requestId}/permission/declined")]
    Task DeclineRequest([Path] Guid requestId, [Body] DeclinePermissionsRequest request, CancellationToken cancellationToken);

    [Post("/requests/{requestId}/createaccount/declined")]
    Task DeclineCreateAccountRequest([Path] Guid requestId, [Body] DeclinePermissionsRequest request, CancellationToken cancellationToken);

    [Get("/requests/{requestId}")]
    Task<GetPermissionRequestResponse> GetPermissionRequest([Path] Guid requestId, CancellationToken cancellationToken);
}
