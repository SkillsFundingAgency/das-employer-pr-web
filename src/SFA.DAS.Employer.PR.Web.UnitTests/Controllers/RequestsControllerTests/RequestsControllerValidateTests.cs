using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Controllers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Controllers.RequestsControllerTests;

public class RequestsControllerValidateTests
{
    [Test, MoqAutoData]
    public async Task Validate_InvalidRequest_ReturnsPageNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RequestsController sut,
        Guid requestId,
        CancellationToken cancellationToken)
    {
        outerApiClientMock.Setup(o => o.ValidateCreateAccountRequest(requestId, cancellationToken)).ReturnsAsync(new ValidateCreateAccountRequestResponse { IsRequestValid = false });

        var result = await sut.ValidateCreateAccountRequest(requestId, cancellationToken);

        result.As<ViewResult>().ViewName.Should().Be(RequestsController.PageNotFoundViewPath);
    }
}
