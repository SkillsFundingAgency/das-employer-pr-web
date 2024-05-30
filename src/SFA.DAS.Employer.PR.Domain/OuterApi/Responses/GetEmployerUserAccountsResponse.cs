namespace SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
public record GetEmployerUserAccountsResponse(string FirstName, string LastName, string EmployerUserId, bool IsSuspended, IEnumerable<EmployerUserAccountItem> UserAccounts);

public record EmployerUserAccountItem(string EncodedAccountId, string DasAccountName, string Role);

