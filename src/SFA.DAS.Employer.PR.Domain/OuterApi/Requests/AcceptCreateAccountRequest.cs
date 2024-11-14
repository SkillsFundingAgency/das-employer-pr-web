namespace SFA.DAS.Employer.PR.Domain.OuterApi.Requests;

public record AcceptCreateAccountRequest(string FirstName, string LastName, string Email, Guid UserRef);
