using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Common;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Controllers;
using SFA.DAS.Employer.PR.Web.Infrastructure;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Controllers;

public sealed class RequestsControllerTests
{
    private Mock<IOuterApiClient> _outerApiClientMock;
    private RequestsController _controller;

    [SetUp]
    public void SetUp()
    {
        _outerApiClientMock = new Mock<IOuterApiClient>();
        _controller = new RequestsController(_outerApiClientMock.Object);
    }

    [Test]
    public async Task Index_ShouldReturnCannotViewRequestView_WhenRequestIsNotFound()
    {
        var requestId = Guid.NewGuid();
        _outerApiClientMock
            .Setup(api => api.GetRequest(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetPermissionRequestResponse?)null);

        var result = await _controller.Index(requestId, CancellationToken.None);

        var viewResult = result as ViewResult;

        Assert.Multiple(() =>
        {
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(ViewNames.CannotViewRequest, Is.EqualTo(viewResult!.ViewName));
        });
    }

    [Test]
    public async Task Index_ShouldRedirectToAction_WhenRequestIsValid()
    {
        var requestId = Guid.NewGuid();
        var validRequest = new GetPermissionRequestResponse
        {
            RequestId = requestId,
            Status = RequestStatus.New,
            RequestType = RequestType.Permission,
            ProviderName = "Test Provider",
            RequestedBy = "RequestedBy",
            RequestedDate = DateTime.UtcNow,
            AccountLegalEntityId = 2,
            EmployerOrganisationName = "EmployerOrganisationName",
            EmployerContactFirstName = "EmployerContactFirstName",
            EmployerContactLastName = "EmployerContactLastName",
            EmployerContactEmail = "EmployerContactEmail",
            EmployerPAYE = "EmployerPAYE",
            EmployerAORN = "EmployerAORN",
            Ukprn = 10000005,
            UpdatedDate = DateTime.UtcNow
        };

        _outerApiClientMock
            .Setup(api => api.GetRequest(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(validRequest);

        var result = await _controller.Index(requestId, CancellationToken.None);

        var redirectToActionResult = result as RedirectToActionResult;
        Assert.That(redirectToActionResult, Is.Not.Null);
    }

    [Test]
    public async Task Index_ShouldReturnCannotViewRequestView_WhenRequestIsInvalid()
    {
        var requestId = Guid.NewGuid();
        var invalidRequest = new GetPermissionRequestResponse
        {
            RequestId = requestId,
            Status = RequestStatus.Accepted,
            RequestType = RequestType.Permission,
            ProviderName = "Test Provider",
            RequestedBy = "RequestedBy",
            RequestedDate = DateTime.UtcNow
        };

        _outerApiClientMock
            .Setup(api => api.GetRequest(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invalidRequest);

        var result = await _controller.Index(requestId, CancellationToken.None);

        var viewResult = result as ViewResult;

        Assert.Multiple(() =>
        {
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(ViewNames.CannotViewRequest, Is.EqualTo(viewResult!.ViewName));
        });
    }
}
