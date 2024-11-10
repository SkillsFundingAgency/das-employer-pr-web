using AutoFixture.NUnit3;
using FluentAssertions.Execution;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Common;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.OuterApi.Requests;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Controllers.Requests;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Infrastructure.Services;
using SFA.DAS.Employer.PR.Web.Models.Requests;
using SFA.DAS.Employer.PR.Web.Models.Session;
using SFA.DAS.Employer.PR.Web.Services;
using SFA.DAS.Employer.PR.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Controllers.RequestsControllerTests;
public class RequestsControllerPostRequestDetailsTests
{
    [Test, MoqAutoData]
    public async Task PostRequestDetails_RequestIsInvalid_RedirectsToRespectiveShutterPage(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IAccountsLinkService> accountsLinkServiceMock,
        [Greedy] RequestsController sut,
        ValidateCreateAccountRequestResponse response,
        Guid requestId,
        string accountsHomeUrl,
        CancellationToken cancellationToken)
    {
        sut.AddDefaultContext();
        response.IsRequestValid = false;
        outerApiClientMock.Setup(o => o.ValidateCreateAccountRequest(requestId, cancellationToken)).ReturnsAsync(response);
        accountsLinkServiceMock.Setup(o => o.GetAccountsHomeLink()).Returns(accountsHomeUrl);

        var result = await sut.ValidateCreateAccountRequest(requestId, cancellationToken);

        result.As<ViewResult>().ViewName.Should().Be(RequestsController.PageNotFoundViewPath);
    }

    [Test, MoqAutoData]
    public async Task PostRequestDetails_RequestIsNotInSentStatus_RedirectsToRespectiveShutterPage(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IAccountsLinkService> accountsLinkServiceMock,
        [Greedy] RequestsController sut,
        ValidateCreateAccountRequestResponse response,
        Guid requestId,
        string accountsHomeUrl,
        CancellationToken cancellationToken)
    {
        sut.AddDefaultContext();
        response.IsRequestValid = true;
        response.Status = RequestStatus.New;
        outerApiClientMock.Setup(o => o.ValidateCreateAccountRequest(requestId, cancellationToken)).ReturnsAsync(response);
        accountsLinkServiceMock.Setup(o => o.GetAccountsHomeLink()).Returns(accountsHomeUrl);

        var result = await sut.ValidateCreateAccountRequest(requestId, cancellationToken);

        result.As<ViewResult>().ViewName.Should().Be(RequestsController.InvalidRequestStatusShutterPageViewPath);
    }

    [Test, MoqAutoData]
    public async Task PostRequestDetails_PayeIsInvalid_RedirectsToRespectiveShutterPage(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IAccountsLinkService> accountsLinkServiceMock,
        [Greedy] RequestsController sut,
        ValidateCreateAccountRequestResponse response,
        Guid requestId,
        string accountsHomeUrl,
        CancellationToken cancellationToken)
    {
        sut.AddDefaultContext();
        response.IsRequestValid = true;
        response.Status = RequestStatus.Sent;
        response.HasValidPaye = false;
        outerApiClientMock.Setup(o => o.ValidateCreateAccountRequest(requestId, cancellationToken)).ReturnsAsync(response);
        accountsLinkServiceMock.Setup(o => o.GetAccountsHomeLink()).Returns(accountsHomeUrl);

        var result = await sut.ValidateCreateAccountRequest(requestId, cancellationToken);

        result.As<ViewResult>().ViewName.Should().Be(RequestsController.InvalidRequestStatusShutterPageViewPath);
    }

    [Test, MoqAutoData]
    public async Task PostRequestDetails_PayeIsInUseByExistingAccount_RedirectsToRespectiveShutterPage(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IAccountsLinkService> accountsLinkServiceMock,
        [Greedy] RequestsController sut,
        ValidateCreateAccountRequestResponse response,
        Guid requestId,
        string accountsHomeUrl,
        CancellationToken cancellationToken)
    {
        sut.AddDefaultContext();
        response.IsRequestValid = true;
        response.Status = RequestStatus.Sent;
        response.HasValidPaye = true;
        response.HasEmployerAccount = true;
        outerApiClientMock.Setup(o => o.ValidateCreateAccountRequest(requestId, cancellationToken)).ReturnsAsync(response);
        accountsLinkServiceMock.Setup(o => o.GetAccountsHomeLink()).Returns(accountsHomeUrl);

        var result = await sut.ValidateCreateAccountRequest(requestId, cancellationToken);

        result.As<ViewResult>().ViewName.Should().Be(RequestsController.AccountAlreadyExistsShutterPageViewPath);
    }

    [Test, MoqAutoData]
    public async Task Post_ValidationFailed_ReturnsExpectedModel(
        [Frozen] Mock<IValidator<EmployerAccountCreationSubmitModel>> validatorMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] RequestsController sut,
        GetPermissionRequestResponse permissionRequest,
        AccountCreationSessionModel sessionModel,
        string createAccountChangeNameLink,
        string declineCreateAccountLink,
        CancellationToken cancellationToken)
    {
        var requestId = permissionRequest.RequestId;
        sessionServiceMock.Setup(s => s.Get<AccountCreationSessionModel>()).Returns(sessionModel);
        outerApiClientMock.Setup(o => o.ValidateCreateAccountRequest(requestId, cancellationToken)).ReturnsAsync(GetValidRequestResponse());
        validatorMock.Setup(v => v.Validate(It.IsAny<EmployerAccountCreationSubmitModel>())).Returns(new ValidationResult(new List<ValidationFailure>()
             {
                 new("TestField","Test Message") { ErrorCode = "1001"}
             }));

        outerApiClientMock.Setup(o => o.GetPermissionRequest(requestId, cancellationToken))
            .ReturnsAsync(permissionRequest);
        sut
            .AddDefaultContext()
            .AddUrlHelperMock()
            .AddUrlForRoute(RouteNames.CreateAccountChangeName, createAccountChangeNameLink)
            .AddUrlForRoute(RouteNames.DeclineCreateAccount, declineCreateAccountLink);
        EmployerAccountCreationSubmitModel submitModel = new() { HasAcceptedTerms = false };

        /// Act
        var result = await sut.PostRequestDetails(requestId, submitModel, cancellationToken);

        ViewResult? viewResult = result.As<ViewResult>();
        EmployerAccountCreationViewModel? viewModel = viewResult.Model as EmployerAccountCreationViewModel;

        using (new AssertionScope())
        {
            outerApiClientMock.Verify(x => x.GetPermissionRequest(requestId, cancellationToken), Times.Once);
            viewModel.Should().BeEquivalentTo(permissionRequest, options =>
                options
                    .ExcludingMissingMembers()
                    .Excluding(x => x.ProviderName)
                    .Excluding(x => x.EmployerOrganisationName));
            viewModel.ProviderName.Should().Be(permissionRequest.ProviderName.ToUpper());
            viewModel.EmployerOrganisationName.Should().Be(permissionRequest.EmployerOrganisationName!.ToUpper());
            viewModel.EmployerContactFirstName.Should().Be(sessionModel.FirstName);
            viewModel.EmployerContactLastName.Should().Be(sessionModel.LastName);
            viewModel.ChangeNameLink.Should().Be(createAccountChangeNameLink);
            viewModel.DeclineCreateAccountLink.Should().Be(declineCreateAccountLink);
        }
    }

    [Test, MoqAutoData]
    public async Task PostRequestDetails_NoValidationFailures_SubmitsCreateAccountRequestAndRedirectsToConfirmationPage(
        [Frozen] Mock<IValidator<EmployerAccountCreationSubmitModel>> validatorMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IAccountsLinkService> accountsLinkServiceMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] RequestsController sut,
        AcceptCreateAccountResponse acceptCreateAccountResponse,
        Guid requestId,
        AccountCreationSessionModel sessionModel,
        CancellationToken cancellationToken)
    {
        outerApiClientMock.Setup(o => o.ValidateCreateAccountRequest(requestId, cancellationToken)).ReturnsAsync(GetValidRequestResponse());
        sessionServiceMock.Setup(s => s.Get<AccountCreationSessionModel>()).Returns(sessionModel);
        outerApiClientMock.Setup(o => o.AcceptCreateAccountRequest(requestId, It.Is<AcceptCreateAccountRequest>(b => b.FirstName == sessionModel.FirstName && b.LastName == sessionModel.LastName && b.Email == ControllerExtensions.UserEmail), cancellationToken)).ReturnsAsync(acceptCreateAccountResponse);
        validatorMock.Setup(v => v.Validate(It.IsAny<EmployerAccountCreationSubmitModel>())).Returns(new ValidationResult());

        sut.AddDefaultContext().AddUrlHelperMock();

        EmployerAccountCreationSubmitModel submitModel = new() { HasAcceptedTerms = true };

        /// Act
        var result = await sut.PostRequestDetails(requestId, submitModel, cancellationToken);

        using (new AssertionScope())
        {
            RedirectToRouteResult redirectToRouteResult = result.As<RedirectToRouteResult>();
            redirectToRouteResult.RouteName.Should().Be(RouteNames.CreateAccountConfirmation);
            outerApiClientMock.Verify(o => o.AcceptCreateAccountRequest(requestId, It.Is<AcceptCreateAccountRequest>(b => b.FirstName == sessionModel.FirstName && b.LastName == sessionModel.LastName && b.Email == ControllerExtensions.UserEmail), cancellationToken), Times.Once);
            outerApiClientMock.Verify(x => x.GetPermissionRequest(requestId, cancellationToken), Times.Never);
            sessionServiceMock.Verify(s => s.Set(It.Is<AccountCreationSessionModel>(s => s.AccountId == acceptCreateAccountResponse.AccountId)), Times.Once);
        }
    }

    [Test, MoqAutoData]
    public async Task PostRequestDetails_SessionTimedOut_RedirectToGetRequest(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] RequestsController sut,
        Guid requestId,
        EmployerAccountCreationSubmitModel model,
        CancellationToken cancellationToken)
    {
        sut.AddDefaultContext();
        sessionServiceMock.Setup(s => s.Get<AccountCreationSessionModel>()).Returns(() => null);

        var result = await sut.PostRequestDetails(requestId, model, cancellationToken);

        result.As<RedirectToRouteResult>().RouteName.Should().Be(RouteNames.CreateAccountCheckDetails);
    }

    private static ValidateCreateAccountRequestResponse GetValidRequestResponse() => new() { IsRequestValid = true, HasValidPaye = true, Status = RequestStatus.Sent, HasEmployerAccount = false };

}
