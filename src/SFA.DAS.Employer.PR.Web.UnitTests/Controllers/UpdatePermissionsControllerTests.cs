using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using SFA.DAS.Employer.PR.Domain.Common;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Domain.OuterApi.Permissions;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Authentication;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Controllers;
using SFA.DAS.Employer.PR.Web.Extensions;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Employer.PR.Web.UnitTests.TestHelpers;
using System.Net;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Controllers;

public class UpdatePermissionsControllerTests
{
    private Mock<IOuterApiClient> _outerApiClientMock;
    private Mock<IValidator<ReviewPermissionsRequestSubmitViewModel>> _validatorMock;
    private UpdatePermissionsController _controller;
    private const string employerAccountId = "V9PRXG";
    private const string YourTrainingProvidersUrl = "your-traing-providers-url";
    private ClaimsPrincipal user;

    [SetUp]
    public void Setup()
    {
        _outerApiClientMock = new Mock<IOuterApiClient>();
        _validatorMock = new Mock<IValidator<ReviewPermissionsRequestSubmitViewModel>>();

        user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        _controller = new UpdatePermissionsController(_outerApiClientMock.Object, _validatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } },
        };

        _controller.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersUrl);

        var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        _controller.TempData = tempData;
    }

    [Test]
    public async Task GetIndex_WhenInvalidResponse_ReturnsPageNotFoundView()
    {
        var requestId = Guid.NewGuid();

        _outerApiClientMock.Setup(x => x.GetRequest(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetPermissionRequestResponse?)null);

        var result = await _controller.Index(requestId, employerAccountId, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());

            var redirectResult = result as RedirectToActionResult;

            Assert.That(redirectResult!.ActionName, Is.EqualTo("HttpStatusCodeHandler"));
            Assert.That(redirectResult.ControllerName, Is.EqualTo(RouteNames.Error));
            Assert.That(redirectResult.RouteValues!["statusCode"], Is.EqualTo((int)HttpStatusCode.NotFound));
        });
    }

    [Test]
    public async Task GetIndex_WhenValidResponse_ReturnsReviewPermissionsView()
    {
        var requestId = Guid.NewGuid();

        var response = new GetPermissionRequestResponse
        {
            ProviderName = "Test Provider",
            RequestType = RequestType.Permission,
            Status = RequestStatus.New,
            Operations = [Operation.CreateCohort],
            RequestedBy = Guid.NewGuid().ToString()
        };

        _outerApiClientMock.Setup(x => x.GetRequest(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _controller.Index(requestId, employerAccountId, CancellationToken.None) as ViewResult;

        Assert.That(result, Is.Not.Null);

        var model = result!.Model as ReviewPermissionsRequestViewModel;

        Assert.Multiple(() =>
        {
            Assert.That(ViewNames.ReviewPermissionsRequest, Is.EqualTo(result.ViewName));
            Assert.That(model, Is.Not.Null);
            Assert.That("Test Provider", Is.EqualTo(model!.ProviderName));
            Assert.That(ReviewRequest.Yes, Is.EqualTo(model!.AddApprenticeRecordsText));
            Assert.That(ReviewRequest.No, Is.EqualTo(model!.RecruitApprenticesText));
        });
    }

    [Test]
    public async Task PostIndex_WhenValidModel_RedirectsToYourTrainingProviders()
    {
        var requestId = Guid.NewGuid();
        var model = new ReviewPermissionsRequestSubmitViewModel { AcceptPermissions = true };
        var response = new GetPermissionRequestResponse
        {
            ProviderName = "Test Provider",
            RequestType = RequestType.Permission,
            Status = RequestStatus.New,
            Operations = [Operation.CreateCohort],
            RequestedBy = Guid.NewGuid().ToString()
        };

        _outerApiClientMock.Setup(x => x.GetRequest(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        _validatorMock.Setup(x => x.Validate(It.IsAny<ReviewPermissionsRequestSubmitViewModel>()))
            .Returns(new ValidationResult());

        var result = await _controller.Index(requestId, employerAccountId, model, CancellationToken.None) as RedirectToRouteResult;

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(RouteNames.YourTrainingProviders, Is.EqualTo(result!.RouteName));
        });
    }

    [Test]
    public async Task PostIndex_WhenInvalidRequest_RedirectsToYourTrainingProviders()
    {
        var requestId = Guid.NewGuid();
        var model = new ReviewPermissionsRequestSubmitViewModel();

        _outerApiClientMock.Setup(x => x.GetRequest(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetPermissionRequestResponse?)null);

        var result = await _controller.Index(requestId, employerAccountId, model, CancellationToken.None) as RedirectToRouteResult;

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(RouteNames.YourTrainingProviders, Is.EqualTo(result!.RouteName));
        });
    }

    [Test]
    public async Task PostIndex_WhenInvalidModel_ReturnsReviewPermissionsView()
    {
        var requestId = Guid.NewGuid();
        var model = new ReviewPermissionsRequestSubmitViewModel();
        var response = new GetPermissionRequestResponse
        {
            ProviderName = "Test Provider",
            RequestType = RequestType.Permission,
            Status = RequestStatus.New,
            Operations = [Operation.CreateCohort],
            RequestedBy = Guid.NewGuid().ToString()
        };

        _outerApiClientMock.Setup(x => x.GetRequest(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure("Field", "Error"));
        _validatorMock.Setup(x => x.Validate(It.IsAny<ReviewPermissionsRequestSubmitViewModel>()))
            .Returns(validationResult);

        var result = await _controller.Index(requestId, employerAccountId, model, CancellationToken.None) as ViewResult;

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(ViewNames.ReviewPermissionsRequest, Is.EqualTo(result!.ViewName));
        });

        var returnedModel = result!.Model as ReviewPermissionsRequestViewModel;
        Assert.Multiple(() =>
        {
            Assert.That(returnedModel, Is.Not.Null);
            Assert.That("Test Provider", Is.EqualTo(returnedModel!.ProviderName));
        });
    }

    [Test]
    public async Task PostIndex_OnAcceptPermissions_SetsTempData()
    {
        var requestId = Guid.NewGuid();
        var model = new ReviewPermissionsRequestSubmitViewModel { AcceptPermissions = true };
        var response = new GetPermissionRequestResponse
        {
            ProviderName = "Test Provider",
            RequestType = RequestType.Permission,
            Status = RequestStatus.New,
            Operations = [Operation.CreateCohort],
            RequestedBy = Guid.NewGuid().ToString()
        };

        _outerApiClientMock.Setup(x => x.GetRequest(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        _validatorMock.Setup(x => x.Validate(It.IsAny<ReviewPermissionsRequestSubmitViewModel>()))
            .Returns(new ValidationResult());

        var result = await _controller.Index(requestId, employerAccountId, model, CancellationToken.None) as RedirectToRouteResult;

        _outerApiClientMock.Verify(
            x => x.AcceptPermissionsRequest(
                requestId,
                It.Is<AcceptPermissionsRequestModel>(model => model.ActionedBy == user.GetUserId().ToString()),
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(RouteNames.YourTrainingProviders, Is.EqualTo(result!.RouteName));

            Assert.That("Test Provider", Is.EqualTo(_controller.TempData[TempDataKeys.NameOfProviderUpdated]));
            Assert.That(RequestType.Permission.ToString(), Is.EqualTo(_controller.TempData[TempDataKeys.RequestTypeActioned]));
            Assert.That(RequestAction.Accepted.ToString(), Is.EqualTo(_controller.TempData[TempDataKeys.RequestAction]));
        });
    }

    [Test]
    public async Task PostIndex_OnDeclinePermissions_SetsTempData()
    {
        var requestId = Guid.NewGuid();
        var accountId = "test-account-id";
        var model = new ReviewPermissionsRequestSubmitViewModel { AcceptPermissions = false };
        var response = new GetPermissionRequestResponse
        {
            ProviderName = "Test Provider",
            RequestType = RequestType.Permission,
            Status = RequestStatus.New,
            Operations = [Operation.CreateCohort],
            RequestedBy = Guid.NewGuid().ToString()
        };

        _outerApiClientMock.Setup(x => x.GetRequest(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        _validatorMock.Setup(x => x.Validate(It.IsAny<ReviewPermissionsRequestSubmitViewModel>()))
            .Returns(new ValidationResult());

        var result = await _controller.Index(requestId, accountId, model, CancellationToken.None) as RedirectToRouteResult;

        _outerApiClientMock.Verify(
            x => x.DeclineRequest(
                requestId,
                It.Is<DeclineRequestModel>(model => model.ActionedBy == user.GetUserId().ToString()),
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(RouteNames.YourTrainingProviders, Is.EqualTo(result!.RouteName));

            Assert.That("Test Provider", Is.EqualTo(_controller.TempData[TempDataKeys.NameOfProviderUpdated]));
            Assert.That(RequestType.Permission.ToString(), Is.EqualTo(_controller.TempData[TempDataKeys.RequestTypeActioned]));
            Assert.That(RequestAction.Declined.ToString(), Is.EqualTo(_controller.TempData[TempDataKeys.RequestAction]));
        });
    }
}
