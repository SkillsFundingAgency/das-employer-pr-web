using SFA.DAS.Employer.PR.Domain.Models;

namespace SFA.DAS.Employer.PR.Domain.Interfaces;
public interface IEmployerAccountsService
{
    Task<EmployerUserAccounts> GetEmployerUserAccounts(string userId, string email);
}