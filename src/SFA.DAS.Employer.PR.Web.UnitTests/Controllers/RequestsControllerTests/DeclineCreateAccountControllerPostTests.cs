using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.OuterApi.Requests;
using SFA.DAS.Employer.PR.Web.Controllers.Requests;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Infrastructure.Services;
using SFA.DAS.Employer.PR.Web.Models.Session;
using SFA.DAS.Employer.PR.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Controllers.RequestsControllerTests;

public sealed class DeclineCreateAccountControllerPostTests
{
    [Test, MoqAutoData]
    public async Task PostDeclineRequest_ShouldRedirectToDeclineCreateAccountConfirmationRoute(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] DeclineCreateAccountController sut,
        string providerName
        )
    {
        sessionServiceMock.Setup(x => x.Get<AccountCreationSessionModel>())
            .Returns(new AccountCreationSessionModel { ProviderName = providerName });

        sut.AddDefaultContext();
        var requestId = Guid.NewGuid();

        var result = await sut.Post(requestId, CancellationToken.None);

        var redirectResult = result as RedirectToRouteResult;

        outerApiClientMock.Verify(x => x.DeclineCreateAccountRequest(
                 requestId,
                 It.IsAny<DeclinePermissionsRequest>(),
                 CancellationToken.None
             ),
             Times.Once
         );

        sessionServiceMock.Verify(s => s.Get<AccountCreationSessionModel>(), Times.Once);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(RouteNames.DeclineCreateAccountConfirmation, Is.EqualTo(redirectResult!.RouteName));
            Assert.That(requestId, Is.EqualTo(redirectResult.RouteValues?["requestId"]));
        });
    }


    [Test, MoqAutoData]
    public async Task PostDeclineRequest_SessionExpired_ShouldRedirectToCreateAccountCheckDetails(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] DeclineCreateAccountController sut,
        string providerName
    )
    {
        sessionServiceMock.Setup(x => x.Get<AccountCreationSessionModel>())
            .Returns((AccountCreationSessionModel)null!);

        sut.AddDefaultContext();
        var requestId = Guid.NewGuid();

        var result = await sut.Post(requestId, CancellationToken.None);

        var redirectResult = result as RedirectToRouteResult;

        outerApiClientMock.Verify(x => x.DeclineRequest(
                requestId,
                It.IsAny<DeclinePermissionsRequest>(),
                CancellationToken.None
            ),
            Times.Never
        );

        sessionServiceMock.Verify(s => s.Get<AccountCreationSessionModel>(), Times.Once);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(RouteNames.CreateAccountCheckDetails, Is.EqualTo(redirectResult!.RouteName));
            Assert.That(requestId, Is.EqualTo(redirectResult.RouteValues?["requestId"]));
        });
    }
}
