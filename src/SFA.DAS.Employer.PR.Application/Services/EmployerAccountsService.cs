using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;

namespace SFA.DAS.Employer.PR.Application.Services;
public class EmployerAccountsService : IEmployerAccountsService
{
    private readonly IOuterApiClient _outerApiClient;

    public EmployerAccountsService(IOuterApiClient outerApiClient)
    {
        _outerApiClient = outerApiClient;
    }

    public async Task<EmployerUserAccounts> GetEmployerUserAccounts(string userId, string email)
    {
        var result = await _outerApiClient.GetUserAccounts(userId, email, CancellationToken.None);
        return Transform(result);
    }

    private static EmployerUserAccounts Transform(GetEmployerUserAccountsResponse response) =>
        new(response.IsSuspended, response.FirstName, response.LastName, response.EmployerUserId, response.UserAccounts.Select(u => new EmployerIdentifier { AccountId = u.EncodedAccountId, EmployerName = u.DasAccountName, Role = u.Role! }).ToList());
}
