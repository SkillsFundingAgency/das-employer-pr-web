using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Common;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Controllers.Requests;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Infrastructure.Services;
using SFA.DAS.Employer.PR.Web.Models.Requests;
using SFA.DAS.Employer.PR.Web.Models.Session;
using SFA.DAS.Employer.PR.Web.UnitTests.TestHelpers;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Controllers.RequestsControllerTests;

public class RequestsControllerGetRequestDetailsTests
{
    public static readonly string ChangeNameLink = Guid.NewGuid().ToString();

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
    public async Task GetRequestDetails_ValidRequests_ReturnsExpectedDetails(
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

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.CreateAccountChangeName, ChangeNameLink);
        sut.AddDefaultContext(permissionRequest.EmployerContactEmail!);

        var result = await sut.GetRequestDetails(requestId, cancellationToken);
        var viewResult = result as ViewResult;
        viewResult!.ViewName.Should().Be(RequestsController.RequestsCheckDetailsViewPath);
        var viewModel = viewResult.Model as EmployerAccountCreationViewModel;
        viewModel!.ChangeNameLink.Should().Be(ChangeNameLink);
        viewModel.Should().BeEquivalentTo(permissionRequest, options =>
            options.ExcludingMissingMembers()
                .Excluding(x => x.ProviderName)
                .Excluding(x => x.EmployerOrganisationName));
    }

    [Test, MoqAutoData]
    public async Task GetRequestDetails_ValidRequests_SetsSessionModelIfNull(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] UrlBuilder builder,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] RequestsController sut,
        Guid requestId,
        GetPermissionRequestResponse permissionRequest,
        string firstName,
        string lastName,
        CancellationToken cancellationToken)
    {
        ValidateCreateAccountRequestResponse response = new()
        {
            IsRequestValid = true,
            Status = RequestStatus.Sent,
            HasValidaPaye = true,
            HasEmployerAccount = false,
        };

        permissionRequest.EmployerContactFirstName = firstName;
        permissionRequest.EmployerContactLastName = lastName;

        outerApiClientMock.Setup(o => o.ValidateCreateAccountRequest(requestId, cancellationToken)).ReturnsAsync(response);
        outerApiClientMock.Setup(o => o.GetPermissionRequest(requestId, cancellationToken)).ReturnsAsync(permissionRequest);
        sessionServiceMock.Setup(s => s.Get<AccountCreationSessionModel>()).Returns((AccountCreationSessionModel)null!);

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.CreateAccountChangeName, ChangeNameLink);
        sut.AddDefaultContext(permissionRequest.EmployerContactEmail!);

        await sut.GetRequestDetails(requestId, cancellationToken);
        sessionServiceMock.Verify(s =>
            s.Set(It.Is<AccountCreationSessionModel>(m => m.FirstName == firstName && m.LastName == lastName)), Times.Once);
    }


    [Test, MoqAutoData]
    public async Task GetRequestDetails_ValidRequests_UpdatesNamesFromSessionModel(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] UrlBuilder builder,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] RequestsController sut,
        Guid requestId,
        GetPermissionRequestResponse permissionRequest,
        string firstName,
        string lastName,
        AccountCreationSessionModel accountCreationSessionModel,
        CancellationToken cancellationToken)
    {
        ValidateCreateAccountRequestResponse response = new()
        {
            IsRequestValid = true,
            Status = RequestStatus.Sent,
            HasValidaPaye = true,
            HasEmployerAccount = false,
        };

        permissionRequest.EmployerContactFirstName = firstName;
        permissionRequest.EmployerContactLastName = lastName;

        outerApiClientMock.Setup(o => o.ValidateCreateAccountRequest(requestId, cancellationToken)).ReturnsAsync(response);
        outerApiClientMock.Setup(o => o.GetPermissionRequest(requestId, cancellationToken)).ReturnsAsync(permissionRequest);
        sessionServiceMock.Setup(s => s.Get<AccountCreationSessionModel>()).Returns(accountCreationSessionModel);

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.CreateAccountChangeName, ChangeNameLink);
        sut.AddDefaultContext(permissionRequest.EmployerContactEmail!);

        var result = await sut.GetRequestDetails(requestId, cancellationToken);
        var viewResult = result as ViewResult;
        var viewModel = viewResult!.Model as EmployerAccountCreationViewModel;
        viewModel!.EmployerContactFirstName.Should().Be(accountCreationSessionModel.FirstName);
        viewModel.EmployerContactLastName.Should().Be(accountCreationSessionModel.LastName);
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

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.CreateAccountChangeName, ChangeNameLink);
        sut.AddDefaultContext(permissionRequest.EmployerContactEmail!);

        await sut.GetRequestDetails(requestId, cancellationToken);

        sut.ControllerContext.HttpContext.Items.Should().ContainKey(SessionKeys.AccountTasksKey);
    }
}
