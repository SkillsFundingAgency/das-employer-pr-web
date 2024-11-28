using AutoFixture.NUnit3;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Controllers.Requests;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Infrastructure.Services;
using SFA.DAS.Employer.PR.Web.Models.Requests;
using SFA.DAS.Employer.PR.Web.Models.Session;
using SFA.DAS.Employer.PR.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Controllers.RequestsControllerTests;
public class ChangeNameControllerPostTests
{
    public static readonly string CreateAccountCheckDetailsLink = Guid.NewGuid().ToString();

    [Test, MoqAutoData]
    public void Post_Validated_ReturnsExpectedModel(
       [Frozen] Mock<IValidator<ChangeNamesViewModel>> validatorMock,
       [Frozen] Mock<IOuterApiClient> outerApiClientMock,
       [Frozen] Mock<ISessionService> sessionServiceMock,
       [Greedy] ChangeNameController sut,
        Guid requestId,
        string firstName,
        string lastName,
        CancellationToken cancellationToken)
    {
        validatorMock.Setup(v => v.Validate(It.IsAny<ChangeNamesViewModel>())).Returns(new ValidationResult());

        sut.AddDefaultContext();

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.CreateAccountCheckDetails, CreateAccountCheckDetailsLink);

        ChangeNamesViewModel submitModel = new() { EmployerContactFirstName = firstName, EmployerContactLastName = lastName };

        var result = sut.Index(requestId, submitModel, cancellationToken);

        RedirectToRouteResult? redirectToRouteResult = result.As<RedirectToRouteResult>();
        redirectToRouteResult.RouteName.Should().Be(RouteNames.CreateAccountCheckDetails);
        sessionServiceMock.Verify(s => s.Set(It.Is<AccountCreationSessionModel>(x => x.FirstName == firstName && x.LastName == lastName)), Times.Once);
    }

    [Test, MoqAutoData]
    public void Post_Validated_TrimsNames(
        [Frozen] Mock<IValidator<ChangeNamesViewModel>> validatorMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ChangeNameController sut,
        Guid requestId,
        string firstName,
        string lastName,
        CancellationToken cancellationToken)
    {
        validatorMock.Setup(v => v.Validate(It.IsAny<ChangeNamesViewModel>())).Returns(new ValidationResult());

        sut.AddDefaultContext();

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.CreateAccountCheckDetails, CreateAccountCheckDetailsLink);

        ChangeNamesViewModel submitModel = new() { EmployerContactFirstName = $" {firstName}", EmployerContactLastName = $"{lastName} " };

        var result = sut.Index(requestId, submitModel, cancellationToken);

        sessionServiceMock.Verify(s => s.Set(It.Is<AccountCreationSessionModel>(x => x.FirstName == firstName && x.LastName == lastName)), Times.Once);
    }


    [Test, MoqAutoData]
    public void Post_Validated_IfSessionIsNull_RedirectsToCreateAccountCheckDetails(
        [Frozen] Mock<IValidator<ChangeNamesViewModel>> validatorMock,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] ChangeNameController sut,
        Guid requestId,
        string firstName,
        string lastName,
        CancellationToken cancellationToken)
    {
        validatorMock.Setup(v => v.Validate(It.IsAny<ChangeNamesViewModel>())).Returns(new ValidationResult());

        sessionServiceMock.Setup(x => x.Get<AccountCreationSessionModel>())
            .Returns((AccountCreationSessionModel)null!);

        sut.AddDefaultContext();

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.CreateAccountCheckDetails, CreateAccountCheckDetailsLink);

        ChangeNamesViewModel submitModel = new() { EmployerContactFirstName = firstName, EmployerContactLastName = lastName };

        var result = sut.Index(requestId, submitModel, cancellationToken);

        RedirectToRouteResult? redirectToRouteResult = result.As<RedirectToRouteResult>();
        redirectToRouteResult.RouteName.Should().Be(RouteNames.CreateAccountCheckDetails);
        sessionServiceMock.Verify(s => s.Get<AccountCreationSessionModel>(), Times.Once);
    }

    [Test, MoqAutoData]
    public void Post_ValidationFailed_ReturnsExpectedModel(
        [Frozen] Mock<IValidator<ChangeNamesViewModel>> validatorMock,
        [Greedy] ChangeNameController sut,
        GetPermissionRequestResponse permissionRequest,
        string firstName,
        string lastName,
        CancellationToken cancellationToken)
    {
        var requestId = permissionRequest.RequestId;

        validatorMock.Setup(v => v.Validate(It.IsAny<ChangeNamesViewModel>())).Returns(new ValidationResult(new List<ValidationFailure>()
             {
                 new("TestField","Test Message") { ErrorCode = "1001"}
             }));

        sut.AddDefaultContext();

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.CreateAccountCheckDetails, CreateAccountCheckDetailsLink);

        ChangeNamesViewModel submitViewModel = new ChangeNamesViewModel { EmployerContactFirstName = firstName, EmployerContactLastName = lastName };

        var result = sut.Index(requestId, submitViewModel, cancellationToken);

        ViewResult? viewResult = result.As<ViewResult>();
        var viewModel = viewResult.Model as ChangeNamesViewModel;
        viewModel!.EmployerContactFirstName.Should().Be(firstName);
        viewModel.EmployerContactLastName.Should().Be(lastName);
    }

    [Test, MoqAutoData]
    public void Post_ValidationFailed_FirstAndLastNamesAreTrimmedInSessionModel(
        [Frozen] Mock<IValidator<ChangeNamesViewModel>> validatorMock,
        [Greedy] ChangeNameController sut,
        GetPermissionRequestResponse permissionRequest,
        string firstName,
        string lastName,
        CancellationToken cancellationToken)
    {
        var requestId = permissionRequest.RequestId;

        validatorMock.Setup(v => v.Validate(It.IsAny<ChangeNamesViewModel>())).Returns(new ValidationResult(new List<ValidationFailure>()
        {
            new("TestField","Test Message") { ErrorCode = "1001"}
        }));

        sut.AddDefaultContext();

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.CreateAccountCheckDetails, CreateAccountCheckDetailsLink);

        ChangeNamesViewModel submitViewModel = new ChangeNamesViewModel { EmployerContactFirstName = $" {firstName}", EmployerContactLastName = $"{lastName} " };

        var result = sut.Index(requestId, submitViewModel, cancellationToken);

        ViewResult? viewResult = result.As<ViewResult>();
        var viewModel = viewResult.Model as ChangeNamesViewModel;
        viewModel!.EmployerContactFirstName.Should().Be(firstName);
        viewModel.EmployerContactLastName.Should().Be(lastName);
    }
}
