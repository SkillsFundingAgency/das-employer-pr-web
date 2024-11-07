using AutoFixture.NUnit3;
using Microsoft.Extensions.Configuration;
using SFA.DAS.Employer.PR.Web.Infrastructure.Configuration;
using SFA.DAS.Employer.PR.Web.Services;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.Employer.Shared.UI.Configuration;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Services;

public class AccountsLinkServiceTests
{
    [Test, AutoData]
    public void GetAccountsLink_LOCALEnv_ReturnsCombinedLinkFromConfiguration(string url, string accountId)
    {
        const string environment = "LOCAL";
        Mock<IConfiguration> configurationMock = new();
        configurationMock.Setup(c => c[ConfigurationKeys.EnvironmentName]).Returns(environment);
        configurationMock.Setup(c => c[ConfigurationKeys.EmployerAccountWebLocalUrl]).Returns(url);
        AccountsLinkService sut = new(new UrlBuilder(environment), configurationMock.Object);

        var actual = sut.GetAccountsLink(EmployerAccountRoutes.CreateAccountAddProviderPermissionSuccess, accountId);

        actual.Should().StartWith(url);
        actual.Should().Contain(accountId);
        actual.Should().Contain(string.Format(MaRoutes.Accounts[EmployerAccountRoutes.CreateAccountAddProviderPermissionSuccess.ToString()], accountId));
    }

    [Test, AutoData]
    public void GetAccountsLink_EnvironmentNameIsNotLocal_ReturnsLinkGeneratedByUrlBuilder(string accountId)
    {
        const string environment = "AT";
        UrlBuilder urlBuilder = new(environment);

        AccountsLinkService sut = new(urlBuilder, Mock.Of<IConfiguration>());

        var actual = sut.GetAccountsLink(EmployerAccountRoutes.CreateAccountAddProviderPermissionSuccess, accountId);

        actual.Should().Be(urlBuilder.AccountsLink(nameof(EmployerAccountRoutes.CreateAccountAddProviderPermissionSuccess), accountId));
    }

    [Test, AutoData]
    public void GetAccountsLink_LOCALEnvMissingLocalUrlConfig_ReturnsLinkGeneratedByUrlBuilder(string accountId)
    {
        const string environment = "LOCAL";
        UrlBuilder urlBuilder = new(environment);
        Mock<IConfiguration> configurationMock = new();
        configurationMock.Setup(c => c[ConfigurationKeys.EnvironmentName]).Returns(environment);
        configurationMock.Setup(c => c[ConfigurationKeys.EmployerAccountWebLocalUrl]).Returns(() => null);
        AccountsLinkService sut = new(urlBuilder, configurationMock.Object);

        var actual = sut.GetAccountsLink(EmployerAccountRoutes.CreateAccountAddProviderPermissionSuccess, accountId);

        actual.Should().Be(urlBuilder.AccountsLink(nameof(EmployerAccountRoutes.CreateAccountAddProviderPermissionSuccess), accountId));
    }

    [Test, AutoData]
    public void GetAccountsHomeLink_LOCALEnv_ReturnsLinkFromConfiguration(string expected)
    {
        const string environment = "LOCAL";
        Mock<IConfiguration> configurationMock = new();
        configurationMock.Setup(c => c[ConfigurationKeys.EnvironmentName]).Returns(environment);
        configurationMock.Setup(c => c[ConfigurationKeys.EmployerAccountWebLocalUrl]).Returns(expected);
        AccountsLinkService sut = new(new UrlBuilder(environment), configurationMock.Object);

        var actual = sut.GetAccountsHomeLink();

        actual.Should().Be(expected);
    }

    [Test]
    public void GetAccountsHomeLink_EnvironmentNameIsNotLocal_ReturnsLinkGeneratedByUrlBuilder()
    {
        const string environment = "AT";
        UrlBuilder urlBuilder = new(environment);

        AccountsLinkService sut = new(urlBuilder, Mock.Of<IConfiguration>());

        var actual = sut.GetAccountsHomeLink();

        actual.Should().Be(urlBuilder.AccountsLink());
    }

    [Test]
    public void GetAccountsHomeLink_LOCALEnvMissingLocalUrlConfig_ReturnsLinkGeneratedByUrlBuilder()
    {
        const string environment = "LOCAL";
        UrlBuilder urlBuilder = new(environment);
        Mock<IConfiguration> configurationMock = new();
        configurationMock.Setup(c => c[ConfigurationKeys.EnvironmentName]).Returns(environment);
        configurationMock.Setup(c => c[ConfigurationKeys.EmployerAccountWebLocalUrl]).Returns(() => null);
        AccountsLinkService sut = new(urlBuilder, configurationMock.Object);

        var actual = sut.GetAccountsHomeLink();

        actual.Should().Be(urlBuilder.AccountsLink());
    }
}
