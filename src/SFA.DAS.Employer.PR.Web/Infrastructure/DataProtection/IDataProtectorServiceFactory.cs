namespace SFA.DAS.Employer.PR.Web.Infrastructure.DataProtection;

public interface IDataProtectorServiceFactory
{
    IDataProtectorService Create(string key);
}
