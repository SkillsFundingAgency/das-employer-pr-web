using System.Web;
using AutoFixture.NUnit3;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Common;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Controllers.Requests;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Infrastructure.DataProtection;
using SFA.DAS.Employer.PR.Web.Infrastructure.Services;
using SFA.DAS.Employer.PR.Web.Models.Requests;
using SFA.DAS.Employer.PR.Web.Models.Session;
using SFA.DAS.Employer.PR.Web.Services;
using SFA.DAS.Employer.PR.Web.UnitTests.TestHelpers;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Controllers.RequestsControllerTests;

public class RequestsControllerGetRequestDetailsTests
{
    public static readonly string ChangeNameLink = Guid.NewGuid().ToString();
    public static readonly string DeclineCreateAccountLink = Guid.NewGuid().ToString();

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
        [Frozen] Mock<IAccountsLinkService> accountsLinkServiceMock,
        [Greedy] RequestsController sut,
        Guid requestId,
        string accountsHomeUrl,
        CancellationToken cancellationToken)
    {
        ValidateCreateAccountRequestResponse response = new()
        {
            IsRequestValid = true,
            Status = RequestStatus.New,
            HasValidPaye = true
        };
        outerApiClientMock.Setup(o => o.ValidateCreateAccountRequest(requestId, cancellationToken)).ReturnsAsync(response);
        accountsLinkServiceMock.Setup(o => o.GetAccountsHomeLink()).Returns(accountsHomeUrl);
        sut.AddDefaultContext();

        var result = await sut.GetRequestDetails(requestId, cancellationToken);

        result.As<ViewResult>().ViewName.Should().Be(RequestsController.InvalidRequestStatusShutterPageViewPath);
        result.As<ViewResult>().Model.As<InvalidCreateAccountRequestShutterPageViewModel>().AccountsUrl.Should().Be(accountsHomeUrl);
    }

    [Test, MoqAutoData]
    public async Task GetRequestDetails_InvalidPayeDetails_ReturnsInvalidRequestStatusShutterPage(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IAccountsLinkService> accountsLinkServiceMock,
        [Greedy] RequestsController sut,
        Guid requestId,
        string accountsHomeUrl,
        CancellationToken cancellationToken)
    {
        ValidateCreateAccountRequestResponse response = new()
        {
            IsRequestValid = true,
            Status = RequestStatus.Sent,
            HasValidPaye = false
        };
        outerApiClientMock.Setup(o => o.ValidateCreateAccountRequest(requestId, cancellationToken)).ReturnsAsync(response);
        accountsLinkServiceMock.Setup(o => o.GetAccountsHomeLink()).Returns(accountsHomeUrl);
        sut.AddDefaultContext();

        var result = await sut.GetRequestDetails(requestId, cancellationToken);

        result.As<ViewResult>().ViewName.Should().Be(RequestsController.InvalidRequestStatusShutterPageViewPath);
        result.As<ViewResult>().Model.As<InvalidCreateAccountRequestShutterPageViewModel>().AccountsUrl.Should().Be(accountsHomeUrl);
    }

    [Test, MoqAutoData]
    public async Task GetRequestDetails_EmployerAccountAlreadyExists_ReturnsAccountAlreadyExistsShutterPage(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IAccountsLinkService> accountsLinkServiceMock,
        [Greedy] RequestsController sut,
        Guid requestId,
        string accountsHomeUrl,
        CancellationToken cancellationToken)
    {
        ValidateCreateAccountRequestResponse response = new()
        {
            IsRequestValid = true,
            Status = RequestStatus.Sent,
            HasValidPaye = true,
            HasEmployerAccount = true
        };
        outerApiClientMock.Setup(o => o.ValidateCreateAccountRequest(requestId, cancellationToken)).ReturnsAsync(response);
        accountsLinkServiceMock.Setup(o => o.GetAccountsHomeLink()).Returns(accountsHomeUrl);

        sut.AddDefaultContext();

        var result = await sut.GetRequestDetails(requestId, cancellationToken);

        result.As<ViewResult>().ViewName.Should().Be(RequestsController.AccountAlreadyExistsShutterPageViewPath);
        result.As<ViewResult>().Model.As<InvalidCreateAccountRequestShutterPageViewModel>().AccountsUrl.Should().Be(accountsHomeUrl);
    }

    [Test, MoqAutoData]
    public async Task GetRequestDetails_UserEmailAndRequestEmailDoNotMatch_RedirectsToCreateAccountCheckDetails(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Greedy] RequestsController sut,
        Guid requestId,
        GetPermissionRequestResponse permissionRequest,
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
        outerApiClientMock.Setup(o => o.GetPermissionRequest(requestId, cancellationToken)).ReturnsAsync(permissionRequest);

        sut.AddDefaultContext();

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
            HasValidPaye = true,
            HasEmployerAccount = false,
        };
        outerApiClientMock.Setup(o => o.ValidateCreateAccountRequest(requestId, cancellationToken)).ReturnsAsync(response);
        outerApiClientMock.Setup(o => o.GetPermissionRequest(requestId, cancellationToken)).ReturnsAsync(permissionRequest);
        permissionRequest.EmployerContactEmail = ControllerExtensions.UserEmail;
        sut
            .AddDefaultContext()
            .AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.CreateAccountChangeName, ChangeNameLink)
            .AddUrlForRoute(RouteNames.DeclineCreateAccount, DeclineCreateAccountLink);

        /// Act
        var result = await sut.GetRequestDetails(requestId, cancellationToken);

        using (new AssertionScope())
        {
            var viewResult = result as ViewResult;
            viewResult.As<ViewResult>().ViewName.Should().Be(RequestsController.RequestsCheckDetailsViewPath);
            var viewModel = result.As<ViewResult>().Model.As<EmployerAccountCreationViewModel>();
            viewModel.ChangeNameLink.Should().Be(ChangeNameLink);
            viewModel.DeclineCreateAccountLink.Should().Be(DeclineCreateAccountLink);
            viewModel.Should().BeEquivalentTo(permissionRequest, options =>
                options.ExcludingMissingMembers()
                    .Excluding(x => x.ProviderName)
                    .Excluding(x => x.EmployerOrganisationName));
        }
    }

    [Test, MoqAutoData]
    public async Task GetRequestDetails_ValidRequests_ReturnsCorrectAgreementLink(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IAccountsLinkService> accountsLinkServiceMock,
        [Frozen] Mock<IDataProtectorServiceFactory> dataProtectionServiceFactoryMock,
        [Frozen] UrlBuilder builder,
        [Greedy] RequestsController sut,
        Mock<IDataProtectorService> dataProtectorServiceMock,
        string encryptedName,
        Guid requestId,
        GetPermissionRequestResponse permissionRequest,
        CancellationToken cancellationToken)
    {
        ValidateCreateAccountRequestResponse response = new()
        {
            IsRequestValid = true,
            Status = RequestStatus.Sent,
            HasValidPaye = true,
            HasEmployerAccount = false,
        };
        outerApiClientMock.Setup(o => o.ValidateCreateAccountRequest(requestId, cancellationToken)).ReturnsAsync(response);
        outerApiClientMock.Setup(o => o.GetPermissionRequest(requestId, cancellationToken)).ReturnsAsync(permissionRequest);
        permissionRequest.EmployerContactEmail = ControllerExtensions.UserEmail;

        var accountName = "Account Name";
        permissionRequest.EmployerOrganisationName = accountName;
        var accountsHomeLink = "https://accounts/agreements/preview";
        accountsLinkServiceMock.Setup(o => o.GetAccountsHomeLink()).Returns(accountsHomeLink);
        var createAccountCheckDetailsLink = "https://relationships/requests";

        dataProtectorServiceMock.Setup(i => i.Protect(permissionRequest.EmployerOrganisationName)).Returns(encryptedName);
        dataProtectionServiceFactoryMock.Setup(o => o.Create(DataProtectionKeys.EmployerName)).Returns(dataProtectorServiceMock.Object);

        sut
            .AddDefaultContext()
            .AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.CreateAccountChangeName, ChangeNameLink)
            .AddUrlForRoute(RouteNames.DeclineCreateAccount, DeclineCreateAccountLink)
            .AddUrlForRoute(RouteNames.CreateAccountCheckDetails, createAccountCheckDetailsLink);

        /// Act
        var result = await sut.GetRequestDetails(requestId, cancellationToken);

        using (new AssertionScope())
        {
            var viewModel = result.As<ViewResult>().Model.As<EmployerAccountCreationViewModel>();
            viewModel.EmployerAgreementLink.Should().StartWith(accountsHomeLink);
            viewModel.EmployerAgreementLink.Should().Contain("agreements/preview");
            viewModel.EmployerAgreementLink.Should().Contain($"returnUrl={HttpUtility.UrlEncode(createAccountCheckDetailsLink)}");
            viewModel.EmployerAgreementLink.Should().Contain($"legalEntityName={HttpUtility.UrlEncode(encryptedName)}");
        }
    }

    [Test, MoqAutoData]
    public async Task GetRequestDetails_ValidRequests_SetsSessionModelIfNull(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] UrlBuilder builder,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] RequestsController sut,
        Guid requestId,
        GetPermissionRequestResponse permissionRequest,
        CancellationToken cancellationToken)
    {
        permissionRequest.EmployerContactEmail = ControllerExtensions.UserEmail;
        ValidateCreateAccountRequestResponse response = new()
        {
            IsRequestValid = true,
            Status = RequestStatus.Sent,
            HasValidPaye = true,
            HasEmployerAccount = false,
        };

        outerApiClientMock.Setup(o => o.ValidateCreateAccountRequest(requestId, cancellationToken)).ReturnsAsync(response);
        outerApiClientMock.Setup(o => o.GetPermissionRequest(requestId, cancellationToken)).ReturnsAsync(permissionRequest);
        sessionServiceMock.Setup(s => s.Get<AccountCreationSessionModel>()).Returns((AccountCreationSessionModel)null!);

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.CreateAccountChangeName, ChangeNameLink);
        sut.AddDefaultContext();

        await sut.GetRequestDetails(requestId, cancellationToken);
        sessionServiceMock.Verify(s =>
            s.Set(It.Is<AccountCreationSessionModel>(m => m.FirstName == permissionRequest.EmployerContactFirstName && m.LastName == permissionRequest.EmployerContactLastName)), Times.Once);
    }


    [Test, MoqAutoData]
    public async Task GetRequestDetails_SessionModelExists_UpdatesNamesFromSessionModel(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] UrlBuilder builder,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] RequestsController sut,
        Guid requestId,
        GetPermissionRequestResponse permissionRequest,
        AccountCreationSessionModel accountCreationSessionModel,
        CancellationToken cancellationToken)
    {
        ValidateCreateAccountRequestResponse response = new()
        {
            IsRequestValid = true,
            Status = RequestStatus.Sent,
            HasValidPaye = true,
            HasEmployerAccount = false,
        };
        permissionRequest.EmployerContactEmail = ControllerExtensions.UserEmail;
        outerApiClientMock.Setup(o => o.ValidateCreateAccountRequest(requestId, cancellationToken)).ReturnsAsync(response);
        outerApiClientMock.Setup(o => o.GetPermissionRequest(requestId, cancellationToken)).ReturnsAsync(permissionRequest);
        sessionServiceMock.Setup(s => s.Get<AccountCreationSessionModel>()).Returns(accountCreationSessionModel);

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.CreateAccountChangeName, ChangeNameLink);
        sut.AddDefaultContext();

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
            HasValidPaye = true,
            HasEmployerAccount = false,
        };
        outerApiClientMock.Setup(o => o.ValidateCreateAccountRequest(requestId, cancellationToken)).ReturnsAsync(response);
        outerApiClientMock.Setup(o => o.GetPermissionRequest(requestId, cancellationToken)).ReturnsAsync(permissionRequest);

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.CreateAccountChangeName, ChangeNameLink);
        sut.AddDefaultContext();

        await sut.GetRequestDetails(requestId, cancellationToken);

        sut.ControllerContext.HttpContext.Items.Should().ContainKey(SessionKeys.AccountTasksKey);
    }
}
