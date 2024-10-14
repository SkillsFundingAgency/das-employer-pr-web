using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Controllers;
using SFA.DAS.Employer.PR.Web.Models.Requests;
using SFA.DAS.Employer.PR.Web.UnitTests.TestHelpers;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Controllers.RequestsControllerTests;

public class RequestsControllerGetRequestDetailsTests
{
    [Test, MoqAutoData]
    public async Task GetRequestDetails_InvalidRequest_ReturnsPageNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RequestsController sut,
        Guid requestId,
        CancellationToken cancellationToken)
    {
        ValidateCreateAccountRequestResponse response = new() { IsRequestValid = false };
        outerApiClientMock.Setup(o => o.ValidateCreateAccountRequest(requestId, cancellationToken)).ReturnsAsync(response);

        sut.AddDefaultContext();

        var result = await sut.GetRequestDetails(requestId, cancellationToken);

        result.As<ViewResult>().ViewName.Should().Be(RequestsController.PageNotFoundViewPath);
    }

    [Test, MoqAutoData]
    public async Task GetRequestDetails_InvalidRequestStatus_ReturnsInvalidRequestStatusShutterPage(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] UrlBuilder builder,
        [Greedy] RequestsController sut,
        Guid requestId,
        CancellationToken cancellationToken)
    {
        ValidateCreateAccountRequestResponse response = new()
        {
            IsRequestValid = true,
            Status = RequestStatus.New,
            HasValidaPaye = true
        };
        outerApiClientMock.Setup(o => o.ValidateCreateAccountRequest(requestId, cancellationToken)).ReturnsAsync(response);

        sut.AddDefaultContext();

        var result = await sut.GetRequestDetails(requestId, cancellationToken);

        result.As<ViewResult>().ViewName.Should().Be(RequestsController.InvalidRequestStatusShutterPageViewPath);
        result.As<ViewResult>().Model.As<InvalidCreateAccountRequestShutterPageViewModel>().AccountsUrl.Should().Be(builder.AccountsLink());
    }

    [Test, MoqAutoData]
    public async Task GetRequestDetails_InvalidPayeDetails_ReturnsInvalidRequestStatusShutterPage(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] UrlBuilder builder,
        [Greedy] RequestsController sut,
        Guid requestId,
        CancellationToken cancellationToken)
    {
        ValidateCreateAccountRequestResponse response = new()
        {
            IsRequestValid = true,
            Status = RequestStatus.Sent,
            HasValidaPaye = false
        };
        outerApiClientMock.Setup(o => o.ValidateCreateAccountRequest(requestId, cancellationToken)).ReturnsAsync(response);

        sut.AddDefaultContext();

        var result = await sut.GetRequestDetails(requestId, cancellationToken);

        result.As<ViewResult>().ViewName.Should().Be(RequestsController.InvalidRequestStatusShutterPageViewPath);
        result.As<ViewResult>().Model.As<InvalidCreateAccountRequestShutterPageViewModel>().AccountsUrl.Should().Be(builder.AccountsLink());
    }

    [Test, MoqAutoData]
    public async Task GetRequestDetails_EmployerAccountAlreadyExists_ReturnsAccountAlreadyExistsShutterPage(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] UrlBuilder builder,
        [Greedy] RequestsController sut,
        Guid requestId,
        CancellationToken cancellationToken)
    {
        ValidateCreateAccountRequestResponse response = new()
        {
            IsRequestValid = true,
            Status = RequestStatus.Sent,
            HasValidaPaye = true,
            HasEmployerAccount = true
        };
        outerApiClientMock.Setup(o => o.ValidateCreateAccountRequest(requestId, cancellationToken)).ReturnsAsync(response);

        sut.AddDefaultContext();

        var result = await sut.GetRequestDetails(requestId, cancellationToken);

        result.As<ViewResult>().ViewName.Should().Be(RequestsController.AccountAlreadyExistsShutterPageViewPath);
        result.As<ViewResult>().Model.As<InvalidCreateAccountRequestShutterPageViewModel>().AccountsUrl.Should().Be(builder.AccountsLink());
    }

    [Test, MoqAutoData]
    public async Task GetRequestDetails_UserEmailAndRequestEmailDoNotMatch_RedirectsToCreateAccountCheckDetails(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] UrlBuilder builder,
        [Greedy] RequestsController sut,
        Guid requestId,
        GetPermissionRequestResponse permissionRequest,
        string email,
        CancellationToken cancellationToken)
    {
        ValidateCreateAccountRequestResponse response = new()
        {
            IsRequestValid = true,
            Status = RequestStatus.Sent,
            HasValidaPaye = true,
            HasEmployerAccount = false
        };
        outerApiClientMock.Setup(o => o.ValidateCreateAccountRequest(requestId, cancellationToken)).ReturnsAsync(response);
        outerApiClientMock.Setup(o => o.GetPermissionRequest(requestId, cancellationToken)).ReturnsAsync(permissionRequest);

        sut.AddDefaultContext(email);

        var result = await sut.GetRequestDetails(requestId, cancellationToken);

        result.As<ViewResult>().ViewName.Should().Be(RequestsController.UserEmailDoesNotMatchRequestShutterPageViewPath);
    }

    [Test, MoqAutoData]
    public async Task GetRequestDetails_ValidRequests_ReturnsCheckDetailsView(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] UrlBuilder builder,
        [Greedy] RequestsController sut,
        Guid requestId,
        GetPermissionRequestResponse permissionRequest,
        CancellationToken cancellationToken)
    {
        ValidateCreateAccountRequestResponse response = new()
        {
            IsRequestValid = true,
            Status = RequestStatus.Sent,
            HasValidaPaye = true,
            HasEmployerAccount = false
        };
        outerApiClientMock.Setup(o => o.ValidateCreateAccountRequest(requestId, cancellationToken)).ReturnsAsync(response);
        outerApiClientMock.Setup(o => o.GetPermissionRequest(requestId, cancellationToken)).ReturnsAsync(permissionRequest);

        sut.AddDefaultContext(permissionRequest.EmployerContactEmail!);

        var result = await sut.GetRequestDetails(requestId, cancellationToken);

        result.As<ViewResult>().ViewName.Should().Be(RequestsController.RequestsCheckDetailsViewPath);
    }

    [Test, MoqAutoData]
    public async Task GetRequestDetails_ValidRequests_SetsAccountTaskInContextToHideMenus(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] UrlBuilder builder,
        [Greedy] RequestsController sut,
        Guid requestId,
        GetPermissionRequestResponse permissionRequest,
        CancellationToken cancellationToken)
    {
        ValidateCreateAccountRequestResponse response = new()
        {
            IsRequestValid = true,
            Status = RequestStatus.Sent,
            HasValidaPaye = true,
            HasEmployerAccount = false,
        };
        outerApiClientMock.Setup(o => o.ValidateCreateAccountRequest(requestId, cancellationToken)).ReturnsAsync(response);
        outerApiClientMock.Setup(o => o.GetPermissionRequest(requestId, cancellationToken)).ReturnsAsync(permissionRequest);

        sut.AddDefaultContext(permissionRequest.EmployerContactEmail!);

        await sut.GetRequestDetails(requestId, cancellationToken);

        sut.ControllerContext.HttpContext.Items.Should().ContainKey(SessionKeys.AccountTasksKey);
    }
}
