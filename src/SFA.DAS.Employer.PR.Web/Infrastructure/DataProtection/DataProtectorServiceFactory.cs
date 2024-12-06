using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;

namespace SFA.DAS.Employer.PR.Web.Infrastructure.DataProtection;

public class DataProtectorServiceFactory(IDataProtectionProvider _provider) : IDataProtectorServiceFactory
{
    public IDataProtectorService Create(string key)
         => new DataProtectorService(_provider.CreateProtector(key));

    internal class DataProtectorService(IDataProtector _dataProtector) : IDataProtectorService
    {
        public string Protect(string plainText)
        {
            return WebEncoders.Base64UrlEncode(_dataProtector.Protect(
                System.Text.Encoding.UTF8.GetBytes(plainText)));
        }

        public string? Unprotect(string? cipherText)
        {
            if (cipherText == null) return null;

            var decodedBytes = WebEncoders.Base64UrlDecode(cipherText);
            var encodedData = System.Text.Encoding.UTF8.GetString(_dataProtector.Unprotect(decodedBytes));
            return encodedData;
        }
    }
}
