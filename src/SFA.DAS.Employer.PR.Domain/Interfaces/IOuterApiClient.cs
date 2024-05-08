using RestEase;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;

namespace SFA.DAS.Employer.PR.Domain.Interfaces;
public interface IOuterApiClient
{
    [Get("/accountusers/{userId}/accounts")]
    Task<GetEmployerUserAccountsResponse> GetUserAccounts([Path] string userId, [Query] string email, CancellationToken cancellationToken);

}
