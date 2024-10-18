using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Common;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Controllers.Requests;
using SFA.DAS.Employer.PR.Web.Infrastructure.Services;
using SFA.DAS.Employer.PR.Web.Models.Requests;
using SFA.DAS.Employer.PR.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Controllers.RequestsControllerTests;

public sealed class DeclineCreateAccountControllerGetTests
{
    [Test, MoqAutoData]
    public async Task Get_WhenValidResponse_ReturnsDeclineCreateAccountView(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] DeclineCreateAccountController sut
        )
    {
        sut.AddDefaultContext();
        var requestId = Guid.NewGuid();

        var response = new GetPermissionRequestResponse
        {
            ProviderName = "Test Provider",
            RequestType = RequestType.AddAccount,
            Status = RequestStatus.New,
            Operations = [Operation.CreateCohort],
            RequestedBy = Guid.NewGuid().ToString()
        };

        outerApiClientMock.Setup(x => x.GetPermissionRequest(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await sut.Index(requestId, CancellationToken.None) as ViewResult;

        Assert.That(result, Is.Not.Null);

        var model = result!.Model as DeclineCreateAccountViewModel;

        Assert.Multiple(() =>
        {
            Assert.That(model, Is.Not.Null);
            Assert.That("Test Provider".ToUpper, Is.EqualTo(model!.ProviderName));
            Assert.That(requestId, Is.EqualTo(model.RequestId));
        });
    }
}
