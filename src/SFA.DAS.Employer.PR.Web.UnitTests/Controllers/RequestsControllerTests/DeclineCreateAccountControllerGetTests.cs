using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Common;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Controllers.Requests;
using SFA.DAS.Employer.PR.Web.Infrastructure;
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
        [Greedy] DeclineCreateAccountController sut,
        string backlink,
        string providerName
        )
    {
        sut.AddDefaultContext();
        var requestId = Guid.NewGuid();

        var response = new GetPermissionRequestResponse
        {
            ProviderName = providerName,
            RequestType = RequestType.AddAccount,
            Status = RequestStatus.New,
            Operations = [Operation.CreateCohort],
            RequestedBy = Guid.NewGuid().ToString()
        };

        outerApiClientMock.Setup(x => x.GetPermissionRequest(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.CreateAccountCheckDetails, backlink);

        var result = await sut.Index(requestId, CancellationToken.None) as ViewResult;

        Assert.That(result, Is.Not.Null);

        var model = result!.Model as DeclineCreateAccountViewModel;

        Assert.Multiple(() =>
        {
            Assert.That(model, Is.Not.Null);
            Assert.That(providerName.ToUpper, Is.EqualTo(model!.ProviderName));
            Assert.That(backlink, Is.EqualTo(model.BackLink));
        });
    }
}
