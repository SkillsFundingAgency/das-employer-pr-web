using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Web.Controllers.Requests;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Infrastructure.Services;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Employer.PR.Web.Models.Session;
using SFA.DAS.Employer.PR.Web.Services;
using SFA.DAS.Employer.PR.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Controllers.RequestsControllerTests;

public sealed class DeclineCreateAccountConfirmationControllerGetTests
{
    [Test, MoqAutoData]
    public void Get_WhenValidResponse_ReturnsConfirmationPage(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IAccountsLinkService> accountsLinkServiceMock,
        [Greedy] DeclineCreateAccountConfirmationController sut,
        string helpLink,
        string providerName
        )
    {
        var requestId = Guid.NewGuid();

        var sessionModel = new AccountCreationSessionModel { ProviderName = providerName };

        sessionServiceMock.Setup(x => x.Get<AccountCreationSessionModel>()).Returns(sessionModel);
        accountsLinkServiceMock.Setup(x => x.GetAccountsLink(EmployerAccountRoutes.Help, null)).Returns(helpLink);

        sut.AddDefaultContext();

        var result = sut.Index(requestId, CancellationToken.None) as ViewResult;

        Assert.That(result, Is.Not.Null);

        var model = result!.Model as DeclineCreateAccountConfirmationViewModel;

        Assert.Multiple(() =>
        {
            Assert.That(model, Is.Not.Null);
            Assert.That(providerName.ToUpper, Is.EqualTo(model!.ProviderName));
            Assert.That(model.HelpLink, Is.EqualTo(helpLink));
        });
    }

    [Test, MoqAutoData]
    public void GetDeclineRequest_SessionExpired_ShouldRedirectToCreateAccountCheckDetails(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] DeclineCreateAccountConfirmationController sut,
        string providerName
    )
    {
        sessionServiceMock.Setup(x => x.Get<AccountCreationSessionModel>())
            .Returns((AccountCreationSessionModel)null!);

        sut.AddDefaultContext();
        var requestId = Guid.NewGuid();

        var result = sut.Index(requestId, CancellationToken.None);

        var redirectResult = result as RedirectToRouteResult;

        sessionServiceMock.Verify(s => s.Get<AccountCreationSessionModel>(), Times.Once);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(RouteNames.CreateAccountCheckDetails, Is.EqualTo(redirectResult!.RouteName));
            Assert.That(requestId, Is.EqualTo(redirectResult.RouteValues?["requestId"]));
        });
    }
}
