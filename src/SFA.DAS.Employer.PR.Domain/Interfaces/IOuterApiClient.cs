using RestEase;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;

namespace SFA.DAS.Employer.PR.Domain.Interfaces;

public interface IOuterApiClient
{
    [Get("/serviceCheck")]
    Task<HttpResponseMessage> ServiceCheck();

    [Get("/accountusers/{userId}/accounts")]
    Task<GetEmployerUserAccountsResponse> GetUserAccounts([Path] string userId, [Query] string email, CancellationToken cancellationToken);

    [Get("/relationships/employeraccount/{accountHashedId}")]
    Task<GetEmployerRelationshipsQueryResponse> GetAccountLegalEntities([Path] string accountHashedId, CancellationToken cancellationToken);

}
