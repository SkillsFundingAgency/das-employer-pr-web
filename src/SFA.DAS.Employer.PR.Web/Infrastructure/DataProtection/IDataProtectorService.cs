namespace SFA.DAS.Employer.PR.Web.Infrastructure.DataProtection;

public interface IDataProtectorService
{
    string Protect(string plainText);
    string? Unprotect(string? cipherText);
}
