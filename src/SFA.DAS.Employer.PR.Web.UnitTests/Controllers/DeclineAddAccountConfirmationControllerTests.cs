using FluentValidation;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Web.Authentication;
using SFA.DAS.Employer.PR.Web.Controllers;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Employer.PR.Web.UnitTests.TestHelpers;
using Moq;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Domain.Common;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Domain.OuterApi.Permissions;
using System.Threading;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Controllers;

public sealed class DeclineAddAccountConfirmationControllerTests
{
    private Mock<IOuterApiClient> _outerApiClientMock;
    private DeclineAddAccountConfirmationController _controller;
    private const string employerAccountId = "V9PRXG";
    private const string YourTrainingProvidersUrl = "your-training-providers-url";
    private ClaimsPrincipal user;

    [SetUp]
    public void Setup()
    {
        _outerApiClientMock = new Mock<IOuterApiClient>();

        user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        _controller = new DeclineAddAccountConfirmationController(_outerApiClientMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } },
        };

        _controller.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersUrl);

        var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        _controller.TempData = tempData;
    }

    [Test]
    public async Task GetIndex_WhenInvalidResponse_ReturnsYourTrainingProvidersView()
    {
        var requestId = Guid.NewGuid();

        _outerApiClientMock.Setup(x => x.GetRequest(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetPermissionRequestResponse?)null);

        var result = await _controller.Index(requestId, employerAccountId, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<RedirectToRouteResult>());
            var redirectResult = result as RedirectToRouteResult;
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(RouteNames.YourTrainingProviders, Is.EqualTo(redirectResult?.RouteName)!);
        });
    }

    [Test]
    public async Task GetIndex_WhenValidResponse_DeclinesRequestAndReturnsView()
    {
        var requestId = Guid.NewGuid();

        var response = new GetPermissionRequestResponse
        {
            ProviderName = "Test Provider",
            RequestType = RequestType.AddAccount,
            Status = RequestStatus.New,
            Operations = [Operation.CreateCohort],
            RequestedBy = Guid.NewGuid().ToString()
        };

        _outerApiClientMock.Setup(x => 
            x.GetRequest(
                requestId, 
                It.IsAny<CancellationToken>()
            )
        ).ReturnsAsync(response);

        var result = await _controller.Index(requestId, employerAccountId, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            
        });

        var viewResult = result as ViewResult;
        var model = viewResult!.Model as DeclineAddAccountConfirmationViewModel;

        Assert.Multiple(() =>
        {
            Assert.That("Test Provider", Is.EqualTo(model!.ProviderName));
            Assert.That(YourTrainingProvidersUrl, Is.EqualTo(model.ManageTrainingProvidersUrl));
        });
        
        _outerApiClientMock.Verify(x => x.DeclineRequest(
                requestId,
                It.IsAny<DeclineRequestModel>(),
                CancellationToken.None
            ), 
            Times.Once
        );
    }
}
