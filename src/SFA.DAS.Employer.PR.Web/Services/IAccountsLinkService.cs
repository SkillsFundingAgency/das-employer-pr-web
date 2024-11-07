namespace SFA.DAS.Employer.PR.Web.Services;

public interface IAccountsLinkService
{
    string GetAccountsLink(EmployerAccountRoutes route, string hashedAccountId);
    string GetAccountsHomeLink();
}
