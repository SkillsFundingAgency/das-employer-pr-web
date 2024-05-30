namespace SFA.DAS.Employer.PR.Domain.Models;
public record EmployerUserAccounts(bool IsSuspended, string FirstName, string LastName, string EmployerUserId, List<EmployerIdentifier> UserAccounts);
