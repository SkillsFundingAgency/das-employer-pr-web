using AutoFixture.NUnit3;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Web.Authentication;
using SFA.DAS.Employer.PR.Web.Controllers;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Infrastructure.Services;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Employer.PR.Web.Models.Session;
using SFA.DAS.Employer.PR.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Controllers.SelectLegalEntityControllerTests;
public class SelectLegalEntityControllerPostTests
{
    static readonly string YourTrainingProvidersLink = Guid.NewGuid().ToString();

    [Test, MoqAutoData]
    public void Post_Validated_ReturnsExpectedModel(
        [Frozen] Mock<IValidator<SelectLegalEntitiesSubmitViewModel>> validatorMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] SelectLegalEntityController sut,
        string employerAccountId,
        int accountId,
        string accountName,
        string publicHashedId)
    {
        var accountLegalEntityId = 1;
        SelectLegalEntitiesSubmitViewModel submitModel = new SelectLegalEntitiesSubmitViewModel
        {
            LegalEntityId = accountLegalEntityId,
            LegalName = "legal name"
        };

        validatorMock.Setup(v => v.Validate(It.IsAny<SelectLegalEntitiesSubmitViewModel>())).Returns(new ValidationResult());
        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);

        ProviderPermission permission = new() { Operations = new List<Operation>(), ProviderName = "provider name", Ukprn = 12345678 };
        permission.Operations.Add(Operation.CreateCohort);

        List<ProviderPermission> permissions = new List<ProviderPermission> { permission };
        List<LegalEntity> accountLegalEntities = new List<LegalEntity>
        {
            new() {AccountId = accountId,Id= accountLegalEntityId, Name=accountName, PublicHashedId = publicHashedId, ProviderPermissions = permissions}
        };

        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel { AccountLegalEntities = accountLegalEntities });

        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        var result = sut.Index(employerAccountId, submitModel);

        RedirectToRouteResult? redirectToRouteResult = result.As<RedirectToRouteResult>();
        redirectToRouteResult.RouteName.Should().Be(RouteNames.SelectTrainingProvider);
    }

    [Test, MoqAutoData]

    public void Post_Validated_SetsSessionModel(
        [Frozen] Mock<IValidator<SelectLegalEntitiesSubmitViewModel>> validatorMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] SelectLegalEntityController sut,
        string employerAccountId,
        int accountId,
        string accountName,
        string publicHashedId)
    {
        var accountLegalEntityId = 1;
        SelectLegalEntitiesSubmitViewModel submitModel = new SelectLegalEntitiesSubmitViewModel { LegalEntityId = accountLegalEntityId };

        validatorMock.Setup(v => v.Validate(It.IsAny<SelectLegalEntitiesSubmitViewModel>())).Returns(new ValidationResult());
        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);

        ProviderPermission permission = new() { Operations = new List<Operation>(), ProviderName = "provider name", Ukprn = 12345678 };
        permission.Operations.Add(Operation.CreateCohort);

        List<ProviderPermission> permissions = new List<ProviderPermission> { permission };
        List<LegalEntity> accountLegalEntities = new List<LegalEntity>
        {
            new() {AccountId = accountId,Id= accountLegalEntityId, Name=accountName, PublicHashedId = publicHashedId, ProviderPermissions = permissions},
            new() {AccountId = 1,Name = "test", ProviderPermissions = permissions,PublicHashedId = "xyz"}
        };

        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel { AccountLegalEntities = accountLegalEntities });

        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };


        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        var result = sut.Index(employerAccountId, submitModel);
        sessionServiceMock.Verify(s => s.Set(It.Is<AddTrainingProvidersSessionModel>(s => s.SelectedLegalEntityId == accountLegalEntityId && s.SelectedLegalName == accountName)), Times.Once);
    }

    [Test, MoqAutoData]

    public void Post_Validated_RedirectIfEmployerAccountDoesNotMatch(
        [Frozen] Mock<IValidator<SelectLegalEntitiesSubmitViewModel>> validatorMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] SelectLegalEntityController sut,
        string employerAccountId,
        int accountId,
        string accountName,
        string publicHashedId,
        CancellationToken cancellationToken)
    {
        var accountLegalEntityId = 1;
        SelectLegalEntitiesSubmitViewModel submitModel = new SelectLegalEntitiesSubmitViewModel { LegalEntityId = accountLegalEntityId };

        validatorMock.Setup(v => v.Validate(It.IsAny<SelectLegalEntitiesSubmitViewModel>())).Returns(new ValidationResult());
        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);

        ProviderPermission permission = new() { Operations = new List<Operation>(), ProviderName = "provider name", Ukprn = 12345678 };
        permission.Operations.Add(Operation.CreateCohort);

        List<ProviderPermission> permissions = new List<ProviderPermission> { permission };
        List<LegalEntity> accountLegalEntities = new List<LegalEntity>
        {
            new() {AccountId = accountId,Id= accountLegalEntityId, Name=accountName, PublicHashedId = publicHashedId, ProviderPermissions = permissions},
            new() {AccountId = 1,Name = "test", ProviderPermissions = permissions,PublicHashedId = "xyz"}
        };

        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel { AccountLegalEntities = accountLegalEntities });

        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };


        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        var result = sut.Index(employerAccountId, submitModel);

        sessionServiceMock.Verify(s => s.Set(It.Is<AddTrainingProvidersSessionModel>(s => s.SelectedLegalEntityId == accountLegalEntityId && s.SelectedLegalName == accountName)), Times.Once);
    }


    [Test, MoqAutoData]
    public void Post_ValidatedAndFailed_ReturnsExpectedModel(
        [Frozen] Mock<IValidator<SelectLegalEntitiesSubmitViewModel>> validatorMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] SelectLegalEntityController sut,
        string employerAccountId,
        int accountId,
        string accountName,
        string publicHashedId)
    {
        SelectLegalEntitiesSubmitViewModel submitModel = new SelectLegalEntitiesSubmitViewModel();
        validatorMock.Setup(m => m.Validate(It.IsAny<SelectLegalEntitiesSubmitViewModel>())).Returns(new ValidationResult(new List<ValidationFailure>()
        {
            new("TestField","Test Message") { ErrorCode = "1001"}
        }));

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);


        ProviderPermission permission = new() { Operations = new List<Operation>(), ProviderName = "provider name", Ukprn = 12345678 };
        permission.Operations.Add(Operation.CreateCohort);

        List<ProviderPermission> permissions = new List<ProviderPermission> { permission };
        List<LegalEntity> accountLegalEntities = new List<LegalEntity>
        {
            new() {AccountId = accountId,Id=1 , Name=accountName, PublicHashedId = publicHashedId, ProviderPermissions = permissions}
        };

        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };


        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel { AccountLegalEntities = accountLegalEntities });

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        var result = sut.Index(employerAccountId, submitModel);

        ViewResult? viewResult = result.As<ViewResult>();
        SelectLegalEntitiesViewModel? viewModel = viewResult.Model as SelectLegalEntitiesViewModel;

        viewModel!.LegalEntities.Count.Should().Be(1);
        LegalEntityModel actualLegalEntity = viewModel.LegalEntities.First();
        actualLegalEntity.AccountId.Should().Be(accountId);
        actualLegalEntity.Name.Should().Be(accountName);
        actualLegalEntity.LegalEntityPublicHashedId.Should().Be(publicHashedId);
        sessionServiceMock.Verify(s => s.Set(It.IsAny<AddTrainingProvidersSessionModel>()), Times.Never);
    }
}
