using SFA.DAS.Employer.PR.Web.Infrastructure.Configuration;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.Employer.Shared.UI.Configuration;

namespace SFA.DAS.Employer.PR.Web.Services;

public class AccountsLinkService(UrlBuilder _urlBuilder, IConfiguration _configuration) : IAccountsLinkService
{
    public string GetAccountsLink(EmployerAccountRoutes route, string? hashedAccountId = null)
    {
        var path = route.ToString();
        string environmentName = GetEnvironmentName(_configuration);
        string? employerWebUrl = GetLocalAccountsUrl(_configuration);

        if (IsLocalEnvironment(environmentName) && !string.IsNullOrWhiteSpace(employerWebUrl))
        {

            var uri = new Uri(employerWebUrl);

            var uriPath = string.IsNullOrEmpty(hashedAccountId)
                ? MaRoutes.Accounts[path]
                : $"{string.Format(MaRoutes.Accounts[path], hashedAccountId)}";
            UriBuilder uriBuilder = new(employerWebUrl)
            {
                Path = uriPath,
                Port = uri.Port == 443 ? -1 : uri.Port
            };
            return uriBuilder.ToString();
        }

        return string.IsNullOrEmpty(hashedAccountId)
            ? _urlBuilder.AccountsLink(path)
            : _urlBuilder.AccountsLink(path, hashedAccountId);
    }

    public string GetAccountsHomeLink()
    {
        string environmentName = GetEnvironmentName(_configuration);
        string? employerWebUrl = GetLocalAccountsUrl(_configuration);

        if (IsLocalEnvironment(environmentName) && !string.IsNullOrWhiteSpace(employerWebUrl))
        {
            return employerWebUrl;
        }

        return _urlBuilder.AccountsLink();
    }
    private static bool IsLocalEnvironment(string environmentName) => !string.IsNullOrEmpty(environmentName) && environmentName.Equals("LOCAL", StringComparison.InvariantCultureIgnoreCase);

    /// <summary>
    ///  This is expected to be defined in the local app settings for local testing only.
    ///  Absence of this setting is inconsequential
    /// </summary>
    /// <param name="_configuration"></param>
    /// <returns></returns>
    private static string? GetLocalAccountsUrl(IConfiguration _configuration) => _configuration[ConfigurationKeys.EmployerAccountWebLocalUrl];

    private static string GetEnvironmentName(IConfiguration _configuration) => _configuration[ConfigurationKeys.EnvironmentName]!;
}
