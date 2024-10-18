using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Web.Controllers.Requests;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Infrastructure.Services;
using SFA.DAS.Employer.PR.Web.Models.Requests;
using SFA.DAS.Employer.PR.Web.Models.Session;
using SFA.DAS.Employer.PR.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Controllers.RequestsControllerTests;

public sealed class DeclineCreateAccountControllerGetTests
{
    [Test, MoqAutoData]
    public void Get_WhenValidResponse_ReturnsDeclineCreateAccountView(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] DeclineCreateAccountController sut,
        string backlink,
        string providerName
        )
    {
        sut.AddDefaultContext();
        var requestId = Guid.NewGuid();

        sessionServiceMock.Setup(s => s.Get<AccountCreationSessionModel>()).Returns(new AccountCreationSessionModel { ProviderName = providerName });

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.CreateAccountCheckDetails, backlink);

        var result = sut.Index(requestId, CancellationToken.None) as ViewResult;

        Assert.That(result, Is.Not.Null);

        var model = result!.Model as DeclineCreateAccountViewModel;

        Assert.Multiple(() =>
        {
            Assert.That(model, Is.Not.Null);
            Assert.That(providerName.ToUpper, Is.EqualTo(model!.ProviderName));
            Assert.That(backlink, Is.EqualTo(model.BackLink));
        });
    }

    [Test, MoqAutoData]
    public void GetDeclineRequest_SessionExpired_ShouldRedirectToCreateAccountCheckDetails(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] DeclineCreateAccountController sut,
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
