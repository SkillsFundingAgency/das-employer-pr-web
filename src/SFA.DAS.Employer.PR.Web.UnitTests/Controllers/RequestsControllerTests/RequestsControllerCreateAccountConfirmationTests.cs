using AutoFixture;
using AutoFixture.NUnit3;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Controllers.Requests;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Infrastructure.Services;
using SFA.DAS.Employer.PR.Web.Models.Requests;
using SFA.DAS.Employer.PR.Web.Models.Session;
using SFA.DAS.Employer.PR.Web.Services;
using SFA.DAS.Employer.PR.Web.UnitTests.TestHelpers;
using SFA.DAS.Encoding;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Controllers.RequestsControllerTests;
public class RequestsControllerCreateAccountConfirmationTests
{
    private RequestsController _sut = null!;
    private string _agreementsLink = null!;
    private string _accountsHomeLink = null!;
    private string _providerName = null!;
    private string _manageProvidersLink = null!;
    private Mock<ISessionService> _sessionServiceMock = null!;

    [SetUp]
    public void SetUp()
    {
        Fixture fixture = new();

        var sessionModel = fixture.Create<AccountCreationSessionModel>();
        _providerName = fixture.Create<string>();
        sessionModel.ProviderName = _providerName;
        _sessionServiceMock = new();
        _sessionServiceMock.Setup(s => s.Get<AccountCreationSessionModel>()).Returns(sessionModel);

        var hashedEmployerAccountId = fixture.Create<string>();
        Mock<IEncodingService> encodingServiceMock = new();
        encodingServiceMock.Setup(e => e.Encode(sessionModel.AccountId, EncodingType.AccountId)).Returns(hashedEmployerAccountId);

        _agreementsLink = fixture.Create<string>();
        Mock<IAccountsLinkService> accountsLinkServiceMock = new();
        accountsLinkServiceMock.Setup(a => a.GetAccountsLink(EmployerAccountRoutes.AccountsAgreements, hashedEmployerAccountId)).Returns(_agreementsLink);
        _accountsHomeLink = fixture.Create<string>();
        accountsLinkServiceMock.Setup(a => a.GetAccountsHomeLink()).Returns(_accountsHomeLink);

        _sut = new(Mock.Of<IOuterApiClient>(), _sessionServiceMock.Object, Mock.Of<IValidator<EmployerAccountCreationSubmitModel>>(), accountsLinkServiceMock.Object, encodingServiceMock.Object);

        _manageProvidersLink = fixture.Create<string>();
        _sut
            .AddDefaultContext()
            .AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.YourTrainingProviders, _manageProvidersLink);
    }

    [Test]
    public void ValidSession_DeletesAccountTaskKeyFromContext()
    {
        _sut.HttpContext.Items.Add(SessionKeys.AccountTasksKey, true);

        /// Act
        _sut.CreateAccountConfirmation();

        _sut.HttpContext.Items.Should().NotContainKey(SessionKeys.AccountTasksKey);
    }

    [Test]
    public void ValidSession_ReturnsView()
    {
        var result = _sut.CreateAccountConfirmation();

        result.As<ViewResult>().ViewName.Should().Be(RequestsController.AccountCreatedConfirmationViewPath);
    }

    [Test]
    public void ValidSession_SetsHomeLinkInViewModel()
    {
        var result = _sut.CreateAccountConfirmation();

        result.As<ViewResult>().Model.As<AccountCreatedConfirmationViewModel>().AccountsHomeUrl.Should().Be(_accountsHomeLink);
    }

    [Test]
    public void ValidSession_SetsAgreementsLinkInViewModel()
    {
        var result = _sut.CreateAccountConfirmation();

        result.As<ViewResult>().Model.As<AccountCreatedConfirmationViewModel>().AccountsAgreementUrl.Should().Be(_agreementsLink);
    }

    [Test]
    public void ValidSession_SetsProviderNameInViewModel()
    {
        var result = _sut.CreateAccountConfirmation();

        result.As<ViewResult>().Model.As<AccountCreatedConfirmationViewModel>().ProviderName.Should().Be(_providerName);
    }

    [Test]
    public void ValidSession_SetsYourTrainingProvidersLinkInViewModel()
    {
        var result = _sut.CreateAccountConfirmation();

        result.As<ViewResult>().Model.As<AccountCreatedConfirmationViewModel>().ManageProvidersUrl.Should().Be(_manageProvidersLink);
    }

    [Test]
    public void ValidSession_ClearsSessionModel()
    {
        _sut.CreateAccountConfirmation();

        _sessionServiceMock.Verify(s => s.Delete<AccountCreationSessionModel>());
    }

    [Test, MoqAutoData]
    public void InvalidSession_ReturnsPageNotFound(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] RequestsController sut)
    {
        sessionServiceMock.Setup(s => s.Get<AccountCreationSessionModel>()).Returns(() => null);

        var result = sut.CreateAccountConfirmation();

        result.As<ViewResult>().ViewName.Should().Be(RequestsController.PageNotFoundViewPath);
    }
}
