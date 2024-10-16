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

public class AddAccountsControllerTests
{
    private Mock<IOuterApiClient> _outerApiClientMock;
    private Mock<IValidator<ReviewAddAccountRequestSubmitViewModel>> _validatorMock;
    private AddAccountsController _controller;
    private const string employerAccountId = "V9PRXG";
    private const string YourTrainingProvidersUrl = "your-training-providers-url";
    private ClaimsPrincipal user;

    [SetUp]
    public void Setup()
    {
        _outerApiClientMock = new Mock<IOuterApiClient>();
        _validatorMock = new Mock<IValidator<ReviewAddAccountRequestSubmitViewModel>>();

        user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        _controller = new AddAccountsController(_outerApiClientMock.Object, _validatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } },
        };

        _controller.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersUrl);

        var tempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        _controller.TempData = tempData;
    }

    [Test]
    public async Task GetIndex_WhenInvalidResponse_ReturnsCannotViewRequestView()
    {
        var requestId = Guid.NewGuid();

        _outerApiClientMock.Setup(x => x.GetRequest(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetPermissionRequestResponse?)null);

        var result = await _controller.Index(requestId, employerAccountId, acceptAddAccountRequest: null, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult!.ViewName, Is.EqualTo(AddAccountsController.CannotViewRequestShutterPageViewPath));
        });
    }

    [Test]
    public async Task GetIndex_WhenValidResponse_ReturnsReviewAddAccountsRequestView()
    {
        var requestId = Guid.NewGuid();

        var response = new GetPermissionRequestResponse
        {
            ProviderName = "Test Provider",
            RequestType = RequestType.AddAccount,
            Status = RequestStatus.New,
            Operations = [Operation.CreateCohort],
            RequestedBy = Guid.NewGuid().ToString(),
            RequestId = Guid.NewGuid(),
            Ukprn = 1,
            RequestedDate = DateTime.UtcNow,
            AccountLegalEntityId = 1,
            EmployerOrganisationName = "EmployerOrganisationName",
            EmployerContactFirstName = "EmployerContactFirstName",
            EmployerContactLastName = "EmployerContactLastName",
            EmployerContactEmail = "EmployerContactEmail",
            EmployerPAYE = "EmployerPAYE",
            EmployerAORN = "EmployerAORN",
            UpdatedDate = DateTime.UtcNow
        };

        _outerApiClientMock.Setup(x => x.GetRequest(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var result = await _controller.Index(requestId, employerAccountId, acceptAddAccountRequest: null, CancellationToken.None) as ViewResult;

        Assert.That(result, Is.Not.Null);

        var model = result!.Model as ReviewAddAccountRequestViewModel;

        Assert.Multiple(() =>
        {
            Assert.That(ViewNames.ReviewAddAccountsRequest, Is.EqualTo(result.ViewName));
            Assert.That(model, Is.Not.Null);
            Assert.That("Test Provider", Is.EqualTo(model!.ProviderName));
            Assert.That(ManageRequests.YesWithEmployerRecordReview, Is.EqualTo(model!.AddApprenticeRecordsText));
            Assert.That(ManageRequests.No, Is.EqualTo(model!.RecruitApprenticesText));
        });
    }

    [Test]
    public async Task PostIndex_WhenValidModel_RedirectsToYourTrainingProviders()
    {
        var requestId = Guid.NewGuid();
        var model = new ReviewAddAccountRequestSubmitViewModel { AcceptAddAccountRequest = true };
        var response = new GetPermissionRequestResponse
        {
            ProviderName = "Test Provider",
            RequestType = RequestType.AddAccount,
            Status = RequestStatus.New,
            Operations = [Operation.CreateCohort],
            RequestedBy = Guid.NewGuid().ToString()
        };

        _outerApiClientMock.Setup(x => x.GetRequest(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        _validatorMock.Setup(x => x.Validate(It.IsAny<ReviewAddAccountRequestSubmitViewModel>()))
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
        var model = new ReviewAddAccountRequestSubmitViewModel();

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
    public async Task PostIndex_WhenInvalidModel_ReturnsReviewAddAccountsRequestView()
    {
        var requestId = Guid.NewGuid();
        var model = new ReviewAddAccountRequestSubmitViewModel();
        var response = new GetPermissionRequestResponse
        {
            ProviderName = "Test Provider",
            RequestType = RequestType.AddAccount,
            Status = RequestStatus.New,
            Operations = [Operation.CreateCohort],
            RequestedBy = Guid.NewGuid().ToString()
        };

        _outerApiClientMock.Setup(x => x.GetRequest(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var validationResult = new ValidationResult();
        validationResult.Errors.Add(new ValidationFailure("Field", "Error"));
        _validatorMock.Setup(x => x.Validate(It.IsAny<ReviewAddAccountRequestSubmitViewModel>()))
            .Returns(validationResult);

        var result = await _controller.Index(requestId, employerAccountId, model, CancellationToken.None) as ViewResult;

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(ViewNames.ReviewAddAccountsRequest, Is.EqualTo(result!.ViewName));
        });

        var returnedModel = result!.Model as ReviewAddAccountRequestViewModel;
        Assert.Multiple(() =>
        {
            Assert.That(returnedModel, Is.Not.Null);
            Assert.That("Test Provider", Is.EqualTo(returnedModel!.ProviderName));
        });
    }

    [Test]
    public async Task PostIndex_WhenDeclinedRequest_ReturnsConfirmDeclineAddAccountShutterView()
    {
        var requestId = Guid.NewGuid();
        var model = new ReviewAddAccountRequestSubmitViewModel()
        {
            AcceptAddAccountRequest = false
        };

        var response = new GetPermissionRequestResponse
        {
            ProviderName = "Test Provider",
            RequestType = RequestType.AddAccount,
            Status = RequestStatus.New,
            Operations = [Operation.CreateCohort],
            RequestedBy = Guid.NewGuid().ToString()
        };

        _outerApiClientMock.Setup(x => x.GetRequest(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        var validationResult = new ValidationResult();
        _validatorMock.Setup(x => x.Validate(It.IsAny<ReviewAddAccountRequestSubmitViewModel>()))
            .Returns(validationResult);

        var result = await _controller.Index(requestId, employerAccountId, model, CancellationToken.None);
        var redirectResult = result as RedirectToRouteResult;

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(RouteNames.DeclineAddAccount, Is.EqualTo(redirectResult?.RouteName));
            Assert.That(employerAccountId, Is.EqualTo(redirectResult?.RouteValues?["employerAccountId"]!));
            Assert.That(requestId, Is.EqualTo(redirectResult?.RouteValues?["requestId"]!));
        });
    }

    [Test]
    public async Task PostIndex_OnAcceptPermissions_SetsTempData()
    {
        var requestId = Guid.NewGuid();
        var model = new ReviewAddAccountRequestSubmitViewModel { AcceptAddAccountRequest = true };
        var response = new GetPermissionRequestResponse
        {
            ProviderName = "Test Provider",
            RequestType = RequestType.AddAccount,
            Status = RequestStatus.New,
            Operations = [Operation.CreateCohort],
            RequestedBy = Guid.NewGuid().ToString()
        };

        _outerApiClientMock.Setup(x => x.GetRequest(requestId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        _validatorMock.Setup(x => x.Validate(It.IsAny<ReviewAddAccountRequestSubmitViewModel>()))
            .Returns(new ValidationResult());

        var result = await _controller.Index(requestId, employerAccountId, model, CancellationToken.None) as RedirectToRouteResult;

        _outerApiClientMock.Verify(
            x => x.AcceptAddAccountRequest(
                requestId,
                It.Is<AcceptAddAccountRequestModel>(model => model.ActionedBy == user.GetUserId().ToString()),
                It.IsAny<CancellationToken>()),
            Times.Once
        );

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(RouteNames.YourTrainingProviders, Is.EqualTo(result!.RouteName));

            Assert.That("Test Provider", Is.EqualTo(_controller.TempData[TempDataKeys.NameOfProviderUpdated]));
            Assert.That(RequestType.AddAccount.ToString(), Is.EqualTo(_controller.TempData[TempDataKeys.RequestTypeActioned]));
            Assert.That(RequestAction.Accepted.ToString(), Is.EqualTo(_controller.TempData[TempDataKeys.RequestAction]));
        });
    }
}
