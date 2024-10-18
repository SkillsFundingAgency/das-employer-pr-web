using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Web.Controllers.Requests;
using SFA.DAS.Employer.PR.Web.Infrastructure.Services;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Employer.PR.Web.Models.Session;
using SFA.DAS.Employer.PR.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Controllers.RequestsControllerTests;

public sealed class DeclineCreateAccountConfirmationControllerGetTests
{
    [Test, MoqAutoData]
    public void Get_WhenValidResponse_ReturnsShutterPage(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] DeclineCreateAccountConfirmationController sut,
        string providerName
        )
    {
        var requestId = Guid.NewGuid();

        var sessionModel = new AccountCreationSessionModel { ProviderName = providerName };

        sessionServiceMock.Setup(x => x.Get<AccountCreationSessionModel>()).Returns(sessionModel);

        sut.AddDefaultContext();

        var result = sut.Index(requestId, CancellationToken.None) as ViewResult;

        Assert.That(result, Is.Not.Null);

        var model = result!.Model as DeclineCreateAccountConfirmationViewModel;

        Assert.Multiple(() =>
        {
            Assert.That(model, Is.Not.Null);
            Assert.That(providerName.ToUpper, Is.EqualTo(model!.ProviderName));
        });
    }
}
