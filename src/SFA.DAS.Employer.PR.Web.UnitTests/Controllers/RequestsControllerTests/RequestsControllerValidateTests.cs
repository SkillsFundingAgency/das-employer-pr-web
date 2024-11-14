using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Common;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Controllers.Requests;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Models.Requests;
using SFA.DAS.Employer.PR.Web.Services;
using SFA.DAS.Employer.PR.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Controllers.RequestsControllerTests;

public class RequestsControllerValidateTests
{
    [Test, MoqAutoData]
    public async Task Validate_SetsAccountTaskInContextToHideMenus(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RequestsController sut,
        Guid requestId,
        CancellationToken cancellationToken)
    {
        ValidateCreateAccountRequestResponse response = new() { IsRequestValid = false };
        outerApiClientMock.Setup(o => o.ValidateCreateAccountRequest(requestId, cancellationToken)).ReturnsAsync(response);
        sut.AddDefaultContext();

        await sut.ValidateCreateAccountRequest(requestId, cancellationToken);

        sut.ControllerContext.HttpContext.Items.Should().ContainKey(SessionKeys.AccountTasksKey);
    }

    [Test, MoqAutoData]
    public async Task Validate_InvalidRequest_ReturnsPageNotFound(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RequestsController sut,
        Guid requestId,
        CancellationToken cancellationToken)
    {
        ValidateCreateAccountRequestResponse response = new() { IsRequestValid = false };
        outerApiClientMock.Setup(o => o.ValidateCreateAccountRequest(requestId, cancellationToken)).ReturnsAsync(response);
        sut.AddDefaultContext();

        var result = await sut.ValidateCreateAccountRequest(requestId, cancellationToken);

        result.As<ViewResult>().ViewName.Should().Be(RequestsController.PageNotFoundViewPath);
    }

    [Test, MoqAutoData]
    public async Task Validate_InvalidRequestStatus_ReturnsInvalidRequestStatusShutterPage(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IAccountsLinkService> accountsLinkServiceMock,
        [Greedy] RequestsController sut,
        Guid requestId,
        string accountsLink,
        CancellationToken cancellationToken)
    {
        ValidateCreateAccountRequestResponse response = new()
        {
            IsRequestValid = true,
            Status = RequestStatus.New,
            HasValidPaye = true
        };
        accountsLinkServiceMock.Setup(s => s.GetAccountsHomeLink()).Returns(accountsLink);
        outerApiClientMock.Setup(o => o.ValidateCreateAccountRequest(requestId, cancellationToken)).ReturnsAsync(response);
        sut.AddDefaultContext();

        var result = await sut.ValidateCreateAccountRequest(requestId, cancellationToken);

        result.As<ViewResult>().ViewName.Should().Be(RequestsController.InvalidRequestStatusShutterPageViewPath);
        result.As<ViewResult>().Model.As<InvalidCreateAccountRequestShutterPageViewModel>().AccountsUrl.Should().Be(accountsLink);
    }

    [Test, MoqAutoData]
    public async Task Validate_InvalidPayeDetails_ReturnsInvalidRequestStatusShutterPage(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IAccountsLinkService> accountsLinkServiceMock,
        [Greedy] RequestsController sut,
        Guid requestId,
        string accountsLink,
        CancellationToken cancellationToken)
    {
        ValidateCreateAccountRequestResponse response = new()
        {
            IsRequestValid = true,
            Status = RequestStatus.Sent,
            HasValidPaye = false
        };
        accountsLinkServiceMock.Setup(s => s.GetAccountsHomeLink()).Returns(accountsLink);
        outerApiClientMock.Setup(o => o.ValidateCreateAccountRequest(requestId, cancellationToken)).ReturnsAsync(response);
        sut.AddDefaultContext();

        var result = await sut.ValidateCreateAccountRequest(requestId, cancellationToken);

        result.As<ViewResult>().ViewName.Should().Be(RequestsController.InvalidRequestStatusShutterPageViewPath);
        result.As<ViewResult>().Model.As<InvalidCreateAccountRequestShutterPageViewModel>().AccountsUrl.Should().Be(accountsLink);
    }

    [Test, MoqAutoData]
    public async Task Validate_EmployerAccountAlreadyExists_ReturnsAccountAlreadyExistsShutterPage(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IAccountsLinkService> accountsLinkServiceMock,
        [Greedy] RequestsController sut,
        Guid requestId,
        string accountsLink,
        CancellationToken cancellationToken)
    {
        ValidateCreateAccountRequestResponse response = new()
        {
            IsRequestValid = true,
            Status = RequestStatus.Sent,
            HasValidPaye = true,
            HasEmployerAccount = true
        };
        accountsLinkServiceMock.Setup(s => s.GetAccountsHomeLink()).Returns(accountsLink);
        outerApiClientMock.Setup(o => o.ValidateCreateAccountRequest(requestId, cancellationToken)).ReturnsAsync(response);
        sut.AddDefaultContext();

        var result = await sut.ValidateCreateAccountRequest(requestId, cancellationToken);

        result.As<ViewResult>().ViewName.Should().Be(RequestsController.AccountAlreadyExistsShutterPageViewPath);
        result.As<ViewResult>().Model.As<InvalidCreateAccountRequestShutterPageViewModel>().AccountsUrl.Should().Be(accountsLink);
    }

    [Test, MoqAutoData]
    public async Task Validate_RequestIsValid_RedirectsToCreateAccountCheckDetails(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RequestsController sut,
        Guid requestId,
        CancellationToken cancellationToken)
    {
        ValidateCreateAccountRequestResponse response = new()
        {
            IsRequestValid = true,
            Status = RequestStatus.Sent,
            HasValidPaye = true,
            HasEmployerAccount = false
        };
        outerApiClientMock.Setup(o => o.ValidateCreateAccountRequest(requestId, cancellationToken)).ReturnsAsync(response);
        sut.AddDefaultContext();

        var result = await sut.ValidateCreateAccountRequest(requestId, cancellationToken);

        result.As<RedirectToRouteResult>().RouteName.Should().Be(RouteNames.CreateAccountCheckDetails);
    }
}
