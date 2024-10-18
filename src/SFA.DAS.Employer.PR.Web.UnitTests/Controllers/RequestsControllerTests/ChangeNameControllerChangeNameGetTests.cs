using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Common;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Controllers.Requests;
using SFA.DAS.Employer.PR.Web.Infrastructure.Services;
using SFA.DAS.Employer.PR.Web.Models.Requests;
using SFA.DAS.Employer.PR.Web.Models.Session;
using SFA.DAS.Employer.PR.Web.UnitTests.TestHelpers;
using SFA.DAS.Employer.Shared.UI;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Controllers.RequestsControllerTests;
public class ChangeNameControllerChangeNameGetTests
{
    public static readonly string Email = "test@account.com";

    [Test, MoqAutoData]
    public async Task GetChangeName_ReturnsExpectedDetails(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        GetPermissionRequestResponse permissionRequestResponse,
        [Greedy] ChangeNameController sut,
        Guid requestId,
        AccountCreationSessionModel accountCreationSessionModel,
        CancellationToken cancellationToken)
    {
        permissionRequestResponse.EmployerContactEmail = Email;
        outerApiClientMock.Setup(x => x.GetPermissionRequest(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissionRequestResponse);
        sessionServiceMock.Setup(x => x.Get<AccountCreationSessionModel>()).Returns(accountCreationSessionModel);

        sut.AddDefaultContext(Email);

        var result = await sut.Index(requestId, cancellationToken);

        var viewResult = result as ViewResult;

        viewResult!.ViewName.Should().Be(RequestsController.RequestsChangeNameViewPath);
        var viewModel = viewResult.Model as EmployerUserNamesViewModel;
        viewModel.EmployerContactFirstName.Should().Be(accountCreationSessionModel.FirstName);
        viewModel.EmployerContactLastName.Should().Be(accountCreationSessionModel.LastName);
    }

    [Test, MoqAutoData]
    public async Task GetChangeName_UserEmailAndRequestEmailDoNotMatch_RedirectsToCreateAccountCheckDetails(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] UrlBuilder builder,
        [Greedy] ChangeNameController sut,
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

        var result = await sut.Index(requestId, cancellationToken);

        result.As<ViewResult>().ViewName.Should().Be(RequestsController.UserEmailDoesNotMatchRequestShutterPageViewPath);
    }
}
