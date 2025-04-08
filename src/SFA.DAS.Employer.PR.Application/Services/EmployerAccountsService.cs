using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.GovUK.Auth.Employer;
using EmployerUserAccountItem = SFA.DAS.GovUK.Auth.Employer.EmployerUserAccountItem;

namespace SFA.DAS.Employer.PR.Application.Services;
public class EmployerAccountsService : IGovAuthEmployerAccountService
{
    private readonly IOuterApiClient _outerApiClient;

    public EmployerAccountsService(IOuterApiClient outerApiClient)
    {
        _outerApiClient = outerApiClient;
    }

    public async Task<EmployerUserAccounts> GetUserAccounts(string userId, string email)
    {
        var result = await _outerApiClient.GetUserAccounts(userId, email, CancellationToken.None);
        return Transform(result);
    }

    private static EmployerUserAccounts Transform(GetEmployerUserAccountsResponse response)
    {
        return new EmployerUserAccounts
        {
            EmployerAccounts = response.UserAccounts != null? response.UserAccounts.Select(c => new EmployerUserAccountItem
            {
                Role = c.Role,
                AccountId = c.EncodedAccountId,
                ApprenticeshipEmployerType = Enum.Parse<ApprenticeshipEmployerType>(c.ApprenticeshipEmployerType.ToString()),
                EmployerName = c.DasAccountName,
            }).ToList() : [],
            FirstName = response.FirstName,
            IsSuspended = response.IsSuspended,
            LastName = response.LastName,
            EmployerUserId = response.EmployerUserId,
        };
    }
}
