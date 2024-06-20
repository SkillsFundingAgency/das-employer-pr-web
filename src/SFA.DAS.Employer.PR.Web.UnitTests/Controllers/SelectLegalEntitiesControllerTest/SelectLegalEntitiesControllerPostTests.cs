using AutoFixture.NUnit3;
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

namespace SFA.DAS.Employer.PR.Web.UnitTests.Controllers.SelectLegalEntitiesControllerTest;
public class SelectLegalEntitiesControllerPostTests
{
    static readonly string YourTrainingProvidersLink = Guid.NewGuid().ToString();

    [Test, MoqAutoData]
    public void Post_Validated_ReturnsExpectedModel(
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        Mock<IValidator<SelectLegalEntitiesSubmitViewModel>> validatorMock,
       string employerAccountId,
       int accountId,
       string accountName,
       string publicHashedId)
    {
        var sessionServiceMock = new Mock<ISessionService>();
        var accountLegalEntityId = 1;
        SelectLegalEntitiesSubmitViewModel submitModel = new SelectLegalEntitiesSubmitViewModel
        {
            LegalEntityId = accountLegalEntityId
        };

        validatorMock.Setup(v => v.Validate(It.IsAny<SelectLegalEntitiesSubmitViewModel>())).Returns(new ValidationResult());
        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        SelectLegalEntityController sut = new(outerApiMock.Object, sessionServiceMock.Object, validatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        Permission permission = new() { Operations = new List<Operation>(), ProviderName = "provider name", Ukprn = 12345678 };
        permission.Operations.Add(Operation.CreateCohort);

        List<Permission> permissions = new List<Permission> { permission };
        List<AccountLegalEntity> accountLegalEntities = new List<AccountLegalEntity>
        {
            new() {AccountId = accountId,Id= accountLegalEntityId, Name=accountName, PublicHashedId = publicHashedId, Permissions = permissions}
        };

        GetEmployerRelationshipsQueryResponse response = new GetEmployerRelationshipsQueryResponse(accountLegalEntities);

        outerApiMock.Setup(o => o.GetAccountLegalEntities(employerAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        Task<IActionResult> result = sut.Index(employerAccountId, submitModel, new CancellationToken());

        ViewResult? viewResult = result.Result.As<ViewResult>();
        SelectLegalEntitiesViewModel? viewModel = viewResult.Model as SelectLegalEntitiesViewModel;

        viewModel!.LegalEntities.Count.Should().Be(1);
        LegalEntityModel actualLegalEntity = viewModel.LegalEntities.First();
        actualLegalEntity.AccountId.Should().Be(accountId);
        actualLegalEntity.Name.Should().Be(accountName);
        actualLegalEntity.LegalEntityPublicHashedId.Should().Be(publicHashedId);
    }

    [Test]
    [MoqInlineAutoData(true)]
    [MoqInlineAutoData(false)]
    public void Post_Validated_SetsSessionModel(
        bool isSelected,
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        Mock<IValidator<SelectLegalEntitiesSubmitViewModel>> validatorMock,
       string employerAccountId,
       int accountId,
       string accountName,
       string publicHashedId)
    {
        var sessionServiceMock = new Mock<ISessionService>();
        var accountLegalEntityId = 1;
        SelectLegalEntitiesSubmitViewModel submitModel = new SelectLegalEntitiesSubmitViewModel();
        if (isSelected)
        {
            submitModel.LegalEntityId = accountLegalEntityId;
        }
        validatorMock.Setup(v => v.Validate(It.IsAny<SelectLegalEntitiesSubmitViewModel>())).Returns(new ValidationResult());
        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        SelectLegalEntityController sut = new(outerApiMock.Object, sessionServiceMock.Object, validatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        Permission permission = new() { Operations = new List<Operation>(), ProviderName = "provider name", Ukprn = 12345678 };
        permission.Operations.Add(Operation.CreateCohort);

        List<Permission> permissions = new List<Permission> { permission };
        List<AccountLegalEntity> accountLegalEntities = new List<AccountLegalEntity>
        {
            new() {AccountId = accountId,Id= accountLegalEntityId, Name=accountName, PublicHashedId = publicHashedId, Permissions = permissions}
        };

        GetEmployerRelationshipsQueryResponse response = new GetEmployerRelationshipsQueryResponse(accountLegalEntities);

        outerApiMock.Setup(o => o.GetAccountLegalEntities(employerAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        Task<IActionResult> result = sut.Index(employerAccountId, submitModel, new CancellationToken());

        ViewResult? viewResult = result.Result.As<ViewResult>();
        SelectLegalEntitiesViewModel? viewModel = viewResult.Model as SelectLegalEntitiesViewModel;

        viewModel!.LegalEntities.Count.Should().Be(1);
        if (isSelected)
        {
            sessionServiceMock.Verify(s => s.Set(It.Is<AddTrainingProvidersSessionModel>(s => s.LegalEntityId == accountLegalEntityId && s.LegalName == accountName)), Times.Once);
        }
        else
        {
            sessionServiceMock.Verify(s => s.Set(It.IsAny<AddTrainingProvidersSessionModel>()), Times.Never);
        }
    }

    [Test, MoqAutoData]
    public void Post_ValidatedAndFailed_ReturnsExpectedModel(
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        Mock<IValidator<SelectLegalEntitiesSubmitViewModel>> validatorMock,
       string employerAccountId,
       int accountId,
       string accountName,
       string publicHashedId)
    {
        var sessionServiceMock = new Mock<ISessionService>();
        SelectLegalEntitiesSubmitViewModel submitModel = new SelectLegalEntitiesSubmitViewModel();
        validatorMock.Setup(m => m.Validate(It.IsAny<SelectLegalEntitiesSubmitViewModel>())).Returns(new ValidationResult(new List<ValidationFailure>()
        {
            new("TestField","Test Message") { ErrorCode = "1001"}
        }));

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        SelectLegalEntityController sut = new(outerApiMock.Object, sessionServiceMock.Object, validatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        Permission permission = new() { Operations = new List<Operation>(), ProviderName = "provider name", Ukprn = 12345678 };
        permission.Operations.Add(Operation.CreateCohort);

        List<Permission> permissions = new List<Permission> { permission };
        List<AccountLegalEntity> accountLegalEntities = new List<AccountLegalEntity>
        {
            new() {AccountId = accountId,Id=1 , Name=accountName, PublicHashedId = publicHashedId, Permissions = permissions}
        };

        GetEmployerRelationshipsQueryResponse response = new GetEmployerRelationshipsQueryResponse(accountLegalEntities);

        outerApiMock.Setup(o => o.GetAccountLegalEntities(employerAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        Task<IActionResult> result = sut.Index(employerAccountId, submitModel, new CancellationToken());

        ViewResult? viewResult = result.Result.As<ViewResult>();
        SelectLegalEntitiesViewModel? viewModel = viewResult.Model as SelectLegalEntitiesViewModel;

        viewModel!.LegalEntities.Count.Should().Be(1);
        LegalEntityModel actualLegalEntity = viewModel.LegalEntities.First();
        actualLegalEntity.AccountId.Should().Be(accountId);
        actualLegalEntity.Name.Should().Be(accountName);
        actualLegalEntity.LegalEntityPublicHashedId.Should().Be(publicHashedId);
        sessionServiceMock.Verify(s => s.Set(It.IsAny<AddTrainingProvidersSessionModel>()), Times.Never);
    }

}
