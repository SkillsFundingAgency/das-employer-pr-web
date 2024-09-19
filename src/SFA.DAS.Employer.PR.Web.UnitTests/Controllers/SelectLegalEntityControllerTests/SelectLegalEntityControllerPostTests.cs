using AutoFixture.NUnit3;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
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
        AccountLegalEntity accountLegalEntity)
    {
        validatorMock.Setup(v => v.Validate(It.IsAny<SelectLegalEntitiesSubmitViewModel>())).Returns(new ValidationResult());
        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);

        List<AccountLegalEntity> accountLegalEntities = [accountLegalEntity];
        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel { AccountLegalEntities = accountLegalEntities });

        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);

        SelectLegalEntitiesSubmitViewModel submitModel = new() { LegalEntityPublicHashedId = accountLegalEntity.AccountLegalEntityPublicHashedId };

        /// Action
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
        List<AccountLegalEntity> accountLegalEntities,
        AccountLegalEntity selectedAccountLegalEntity)
    {
        SelectLegalEntitiesSubmitViewModel submitModel = new SelectLegalEntitiesSubmitViewModel { LegalEntityPublicHashedId = selectedAccountLegalEntity.AccountLegalEntityPublicHashedId };

        validatorMock.Setup(v => v.Validate(It.IsAny<SelectLegalEntitiesSubmitViewModel>())).Returns(new ValidationResult());
        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);

        accountLegalEntities.Add(selectedAccountLegalEntity);

        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel { AccountLegalEntities = accountLegalEntities });

        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);

        /// Action
        sut.Index(employerAccountId, submitModel);

        sessionServiceMock.Verify(s => s.Set(It.Is<AddTrainingProvidersSessionModel>(s => s.SelectedLegalEntityId == selectedAccountLegalEntity.AccountLegalEntityId && s.SelectedLegalName == selectedAccountLegalEntity.AccountLegalEntityName)), Times.Once);
    }

    [Test, MoqAutoData]
    public void Post_ValidationFailed_ReturnsExpectedModel(
        [Frozen] Mock<IValidator<SelectLegalEntitiesSubmitViewModel>> validatorMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] SelectLegalEntityController sut,
        List<AccountLegalEntity> accountLegalEntities,
        string employerAccountId)
    {
        SelectLegalEntitiesSubmitViewModel submitModel = new();

        validatorMock.Setup(m => m.Validate(It.IsAny<SelectLegalEntitiesSubmitViewModel>())).Returns(new ValidationResult(new List<ValidationFailure>()
        {
            new("TestField","Test Message") { ErrorCode = "1001"}
        }));

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);

        ProviderPermission permission = new() { Operations = new List<Operation>(), ProviderName = "provider name", Ukprn = 12345678 };
        permission.Operations.Add(Operation.CreateCohort);

        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel { AccountLegalEntities = accountLegalEntities });

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        var result = sut.Index(employerAccountId, submitModel);

        ViewResult? viewResult = result.As<ViewResult>();
        SelectLegalEntitiesViewModel? viewModel = viewResult.Model as SelectLegalEntitiesViewModel;

        viewModel!.LegalEntities.Count.Should().Be(accountLegalEntities.Count);
        sessionServiceMock.Verify(s => s.Set(It.IsAny<AddTrainingProvidersSessionModel>()), Times.Never);
    }

    [Test, MoqAutoData]
    public void Post_SessionModelNotSet_RedirectsToYourProvidersPage(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] SelectLegalEntityController sut,
        string employerAccountId)
    {
        SelectLegalEntitiesSubmitViewModel submitModel = new();

        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>()).Returns(() => null);

        var result = sut.Index(employerAccountId, submitModel);

        result.As<RedirectToRouteResult>().Should().NotBeNull();
        result.As<RedirectToRouteResult>().RouteName.Should().Be(RouteNames.YourTrainingProviders);
    }
}
