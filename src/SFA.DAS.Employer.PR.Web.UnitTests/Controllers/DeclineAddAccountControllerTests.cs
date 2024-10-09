using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using SFA.DAS.Employer.PR.Domain.Common;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Authentication;
using SFA.DAS.Employer.PR.Web.Controllers;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Employer.PR.Web.UnitTests.TestHelpers;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Controllers;

public sealed class DeclineAddAccountControllerTests
{
    private Mock<IOuterApiClient> _outerApiClientMock;
    private Mock<IValidator<ReviewAddAccountRequestSubmitViewModel>> _validatorMock;
    private DeclineAddAccountController _controller;
    private const string employerAccountId = "V9PRXG";
    private const string RequestsUrl = "requests-url";
    private const string AddAccountsUrl = "add-accounts-url";
    private ClaimsPrincipal user;

    [SetUp]
    public void Setup()
    {
        _outerApiClientMock = new Mock<IOuterApiClient>();
        _validatorMock = new Mock<IValidator<ReviewAddAccountRequestSubmitViewModel>>();

        user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        _controller = new DeclineAddAccountController(_outerApiClientMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } },
        };

        _controller.AddUrlHelperMock().AddUrlForRoute(RouteNames.Requests, RequestsUrl);
        _controller.AddUrlHelperMock().AddUrlForRoute(RouteNames.AddAccounts, AddAccountsUrl);

        var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        _controller.TempData = tempData;
    }

    [Test]
    public async Task GetIndex_WhenInvalidResponse_ReturnsCannotViewRequest()
    {
        var requestId = Guid.NewGuid();

        _outerApiClientMock.Setup(x => x.GetRequest(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetPermissionRequestResponse?)null);

        var result = await _controller.Index(requestId, employerAccountId, null, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());

            var redirectResult = result as RedirectToActionResult;

            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(RouteNames.Requests, Is.EqualTo(redirectResult?.ActionName)!);
            Assert.That(requestId, Is.EqualTo(redirectResult?.RouteValues?["requestId"]!));
        });
    }

    [Test]
    public async Task GetIndex_WhenValidResponse_ReturnsDeclineAddAccountView()
    {
        var requestid = Guid.NewGuid();

        var response = new GetPermissionRequestResponse
        {
            ProviderName = "Test Provider",
            RequestType = RequestType.AddAccount,
            Status = RequestStatus.New,
            Operations = [Operation.CreateCohort],
            RequestedBy = Guid.NewGuid().ToString()
        };

        _outerApiClientMock.Setup(x => x.GetRequest(requestid, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _controller.Index(requestid, employerAccountId, acceptAddAccountRequest: null, CancellationToken.None) as ViewResult;

        Assert.That(result, Is.Not.Null);

        var model = result!.Model as DeclineAddAccountRequestViewModel;

        Assert.Multiple(() =>
        {
            Assert.That(model, Is.Not.Null);
            Assert.That("Test Provider", Is.EqualTo(model!.ProviderName));
            Assert.That(AddAccountsUrl, Is.EqualTo(model!.BackLink));
        });
    }

    [Test]
    public void PostIndex_ShouldRedirectToDeclineAddAccountConfirmationRoute_WithCorrectParameters()
    {
        var requestId = Guid.NewGuid();

        var result = _controller.Index(requestId, employerAccountId, CancellationToken.None) as RedirectToRouteResult;

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(RouteNames.DeclineAddAccountConfirmation, Is.EqualTo(result!.RouteName));
            Assert.That(requestId, Is.EqualTo(result.RouteValues?["requestId"]));
            Assert.That(employerAccountId, Is.EqualTo(result.RouteValues?["employerAccountId"]));
        });
    }
}
