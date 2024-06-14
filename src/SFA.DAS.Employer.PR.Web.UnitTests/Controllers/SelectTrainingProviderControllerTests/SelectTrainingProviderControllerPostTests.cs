using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Authentication;
using SFA.DAS.Employer.PR.Web.Controllers;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Infrastructure.Services;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Employer.PR.Web.Models.Session;
using SFA.DAS.Employer.PR.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Controllers.SelectTrainingProviderControllerTests;
public class SelectTrainingProviderControllerPostTests
{
    static readonly string YourTrainingProvidersLink = Guid.NewGuid().ToString();

    [Test, MoqAutoData]
    public async Task Post_Validated_ReturnsExpectedModel_SetsSessionModel(
        Mock<IValidator<SelectTrainingProviderSubmitModel>> validatorMock,
        string employerAccountId,
        int ukprn,
        string name,
        long accountLegalEntityId,
        CancellationToken cancellationToken)
    {
        var sessionServiceMock = new Mock<ISessionService>();
        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel
            {
                LegalEntityId = accountLegalEntityId,
                AccountLegalEntities = new() { new AccountLegalEntity() }
            });

        var outerApiClientMock = new Mock<IOuterApiClient>();

        outerApiClientMock.Setup(o => o.GetPermissions(ukprn, accountLegalEntityId, cancellationToken))
            .ReturnsAsync(new GetPermissionsResponse { Operations = new List<Operation>() });

        SelectTrainingProviderSubmitModel submitModel = new SelectTrainingProviderSubmitModel
        {
            Name = name,
            Ukprn = ukprn.ToString(),
        };

        validatorMock.Setup(v => v.Validate(It.IsAny<SelectTrainingProviderSubmitModel>())).Returns(new ValidationResult());
        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        SelectTrainingProviderController sut = new(outerApiClientMock.Object, sessionServiceMock.Object, validatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        var result = await sut.Index(employerAccountId, submitModel, cancellationToken);

        var redirectResult = result.As<RedirectToActionResult>();
        redirectResult!.ActionName.Should().Be("Index");
        redirectResult.ControllerName.Should().Be("SetPermissions");

        sessionServiceMock.Verify(s => s.Set(It.Is<AddTrainingProvidersSessionModel>(x => x.ProviderName == name && x.Ukprn == ukprn)), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task Post_ValidatedAndFailed_ReturnsExpectedModel(
        Mock<IValidator<SelectTrainingProviderSubmitModel>> validatorMock,
        string employerAccountId,
        CancellationToken cancellationToken)
    {
        var sessionServiceMock = new Mock<ISessionService>();
        var outerApiClientMock = new Mock<IOuterApiClient>();
        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel { AccountLegalEntities = new() { new AccountLegalEntity() } });

        SelectTrainingProviderSubmitModel submitModel = new SelectTrainingProviderSubmitModel();
        validatorMock.Setup(m => m.Validate(It.IsAny<SelectTrainingProviderSubmitModel>())).Returns(new ValidationResult(new List<ValidationFailure>()
        {
            new("TestField","Test Message") { ErrorCode = "1001"}
        }));

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        SelectTrainingProviderController sut = new(outerApiClientMock.Object, sessionServiceMock.Object, validatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        var result = await sut.Index(employerAccountId, submitModel, cancellationToken);

        ViewResult? viewResult = result.As<ViewResult>();
        SelectTrainingProviderModel? viewModel = viewResult.Model as SelectTrainingProviderModel;

        viewModel!.BackLink.Should().Be(YourTrainingProvidersLink);
        sessionServiceMock.Verify(s => s.Set(It.IsAny<AddTrainingProvidersSessionModel>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task Post_LegalEntityUkprnAlreadyExists_DirectsToShutterPage(
        Mock<IValidator<SelectTrainingProviderSubmitModel>> validatorMock,
        string employerAccountId,
        int ukprn,
        string name,
        long accountLegalEntityId,
        CancellationToken cancellationToken)
    {
        var sessionServiceMock = new Mock<ISessionService>();
        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel
            {
                LegalEntityId = accountLegalEntityId,
                AccountLegalEntities = new() { new AccountLegalEntity() }
            });

        var outerApiClientMock = new Mock<IOuterApiClient>();

        outerApiClientMock.Setup(o => o.GetPermissions(ukprn, accountLegalEntityId, cancellationToken))
            .ReturnsAsync(new GetPermissionsResponse { Operations = new List<Operation> { Operation.CreateCohort } });

        SelectTrainingProviderSubmitModel submitModel = new SelectTrainingProviderSubmitModel
        {
            Name = name,
            Ukprn = ukprn.ToString(),
        };

        validatorMock.Setup(v => v.Validate(It.IsAny<SelectTrainingProviderSubmitModel>())).Returns(new ValidationResult());
        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        SelectTrainingProviderController sut = new(outerApiClientMock.Object, sessionServiceMock.Object, validatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        var result = await sut.Index(employerAccountId, submitModel, cancellationToken);

        var viewResult = result.As<ViewResult>();
        var viewModel = viewResult.Model as AddPermissionsShutterPageViewModel;
        viewModel!.ProviderName.Should().Be(name);
        viewModel.Ukprn.Should().Be(ukprn);
        viewModel.SetPermissionsLink.Should().Be(YourTrainingProvidersLink);
        viewModel.ReturnToYourTrainingProvidersLink.Should().Be(YourTrainingProvidersLink);
        viewResult.ViewName.Should().Be(SelectTrainingProviderController.ShutterPageViewPath);
    }
}