using AutoFixture.NUnit3;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Configuration;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Domain.OuterApi.Permissions;
using SFA.DAS.Employer.PR.Web.Authentication;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Controllers;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Infrastructure.Services;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Employer.PR.Web.Models.Session;
using SFA.DAS.Employer.PR.Web.Services;
using SFA.DAS.Employer.PR.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Controllers.AddPermissionsControllerTests;

public class AddPermissionsControllerPostTests
{
    static readonly string YourTrainingProvidersLink = Guid.NewGuid().ToString();

    [Test, MoqAutoData]
    public async Task Post_Validated_ReturnsExpectedModel(
        [Frozen] Mock<IValidator<AddPermissionsSubmitModel>> validatorMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] AddPermissionsController sut,
        string employerAccountId,
        long ukprn,
        long legalEntityId,
        CancellationToken cancellationToken)
    {
        AddPermissionsSubmitModel submitViewModel = new AddPermissionsSubmitModel
        {
            PermissionToAddCohorts = OperationsMappingService.Yes,
            PermissionToRecruit = OperationsMappingService.Yes
        };

        validatorMock.Setup(v => v.Validate(It.IsAny<AddPermissionsSubmitModel>())).Returns(new ValidationResult());
        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);

        ProviderPermission permission = new()
        { Operations = new List<Operation>(), ProviderName = "provider name", Ukprn = 12345678 };
        permission.Operations.Add(Operation.CreateCohort);

        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel
            { Ukprn = ukprn, SelectedLegalEntityId = legalEntityId, EmployerAccountId = employerAccountId });

        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

        Mock<ITempDataDictionary> tempDataMock = new();
        sut.TempData = tempDataMock.Object;
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        var result = await sut.Index(employerAccountId, submitViewModel, cancellationToken);

        RedirectToRouteResult? redirectToRouteResult = result.As<RedirectToRouteResult>();

        redirectToRouteResult.RouteName.Should().Be(RouteNames.YourTrainingProviders);
    }

    [Test]
    [MoqInlineAutoData(OperationsMappingService.Yes, OperationsMappingService.Yes)]
    [MoqInlineAutoData(OperationsMappingService.Yes, OperationsMappingService.YesWithReview)]
    [MoqInlineAutoData(OperationsMappingService.Yes, OperationsMappingService.No)]
    [MoqInlineAutoData(OperationsMappingService.No, OperationsMappingService.Yes)]
    [MoqInlineAutoData(OperationsMappingService.No, OperationsMappingService.YesWithReview)]
    public async Task Post_Validated_SetsTempData(
        string addRecords,
        string recruitApprentices,
        [Frozen] Mock<IValidator<AddPermissionsSubmitModel>> validatorMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] AddPermissionsController sut,
        string employerAccountId,
        long ukprn,
        long legalEntityId,
        string providerName,
        CancellationToken cancellationToken)
    {
        AddPermissionsSubmitModel submitViewModel = new AddPermissionsSubmitModel
        {
            PermissionToAddCohorts = addRecords,
            PermissionToRecruit = recruitApprentices
        };

        validatorMock.Setup(v => v.Validate(It.IsAny<AddPermissionsSubmitModel>())).Returns(new ValidationResult());
        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);

        ProviderPermission permission = new()
        { Operations = new List<Operation>(), ProviderName = "provider name", Ukprn = 12345678 };
        permission.Operations.Add(Operation.CreateCohort);

        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel
            { Ukprn = ukprn, SelectedLegalEntityId = legalEntityId, EmployerAccountId = employerAccountId, ProviderName = providerName });

        var tempData = new TempDataDictionary(new DefaultHttpContext { User = user }, Mock.Of<ITempDataProvider>());

        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
        sut.TempData = tempData;

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        await sut.Index(employerAccountId, submitViewModel, cancellationToken);

        sut.TempData[TempDataKeys.NameOfProviderAdded]!.ToString().Should().Be(providerName);
    }

    [Test]
    [MoqInlineAutoData(OperationsMappingService.Yes, OperationsMappingService.Yes)]
    [MoqInlineAutoData(OperationsMappingService.Yes, OperationsMappingService.YesWithReview)]
    [MoqInlineAutoData(OperationsMappingService.Yes, OperationsMappingService.No)]
    [MoqInlineAutoData(OperationsMappingService.No, OperationsMappingService.Yes)]
    [MoqInlineAutoData(OperationsMappingService.No, OperationsMappingService.YesWithReview)]
    public async Task Post_Validated_PostExpectedValues(
        string addRecords,
        string recruitApprentices,
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IValidator<AddPermissionsSubmitModel>> validatorMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] AddPermissionsController sut,
        string employerAccountId,
        long ukprn,
        long legalEntityId,
        CancellationToken cancellationToken)
    {
        AddPermissionsSubmitModel submitViewModel = new AddPermissionsSubmitModel
        {
            PermissionToAddCohorts = addRecords,
            PermissionToRecruit = recruitApprentices
        };

        var expectedOperations = new List<Operation>();

        if (addRecords == OperationsMappingService.Yes)
        {
            expectedOperations.Add(Operation.CreateCohort);
        }

        switch (recruitApprentices)
        {
            case OperationsMappingService.Yes:
                expectedOperations.Add(Operation.Recruitment);
                break;
            case OperationsMappingService.YesWithReview:
                expectedOperations.Add(Operation.RecruitmentRequiresReview);
                break;
        }

        validatorMock.Setup(v => v.Validate(It.IsAny<AddPermissionsSubmitModel>())).Returns(new ValidationResult());
        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);

        ProviderPermission permission = new()
        { Operations = new List<Operation>(), ProviderName = "provider name", Ukprn = 12345678 };
        permission.Operations.Add(Operation.CreateCohort);

        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel
            { Ukprn = ukprn, SelectedLegalEntityId = legalEntityId, EmployerAccountId = employerAccountId });

        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        Mock<ITempDataDictionary> tempDataMock = new();
        sut.TempData = tempDataMock.Object;

        /// action
        await sut.Index(employerAccountId, submitViewModel, cancellationToken);

        outerApiClientMock.Verify(o => o.PostPermissions(It.Is<PostPermissionsCommand>(
            c => c.UserRef == UsersForTesting.EmployerUserRef
                 && c.Ukprn == ukprn
                 && c.AccountLegalEntityId == legalEntityId
                 && c.Operations.Count == expectedOperations.Count
                 && c.Operations.Contains(expectedOperations[0])
        ), cancellationToken), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task Post_Validated_SessionModelExpired(
        [Frozen] Mock<IValidator<AddPermissionsSubmitModel>> validatorMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] AddPermissionsController sut,
        string employerAccountId,
        long ukprn,
        long legalEntityId,
        CancellationToken cancellationToken)
    {
        AddPermissionsSubmitModel submitViewModel = new AddPermissionsSubmitModel
        {
            PermissionToAddCohorts = OperationsMappingService.Yes,
            PermissionToRecruit = OperationsMappingService.Yes
        };

        validatorMock.Setup(v => v.Validate(It.IsAny<AddPermissionsSubmitModel>())).Returns(new ValidationResult());
        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);

        ProviderPermission permission = new()
        { Operations = new List<Operation>(), ProviderName = "provider name", Ukprn = 12345678 };
        permission.Operations.Add(Operation.CreateCohort);

        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns((AddTrainingProvidersSessionModel)null!);

        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        var result = await sut.Index(employerAccountId, submitViewModel, cancellationToken);

        RedirectToRouteResult? redirectToRouteResult = result.As<RedirectToRouteResult>();
        redirectToRouteResult.RouteName.Should().Be(RouteNames.YourTrainingProviders);
    }

    [Test, MoqAutoData]
    public async Task Post_Validated_EmployerAccountIdNotMatched(
        [Frozen] Mock<IValidator<AddPermissionsSubmitModel>> validatorMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] AddPermissionsController sut,
        string employerAccountId,
        CancellationToken cancellationToken)
    {
        AddPermissionsSubmitModel submitViewModel = new AddPermissionsSubmitModel
        {
            PermissionToAddCohorts = OperationsMappingService.Yes,
            PermissionToRecruit = OperationsMappingService.Yes
        };

        validatorMock.Setup(v => v.Validate(It.IsAny<AddPermissionsSubmitModel>())).Returns(new ValidationResult());
        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);

        ProviderPermission permission = new()
        { Operations = new List<Operation>(), ProviderName = "provider name", Ukprn = 12345678 };
        permission.Operations.Add(Operation.CreateCohort);

        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel { EmployerAccountId = $"{employerAccountId}_other" });

        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);

        /// Action
        var result = await sut.Index(employerAccountId, submitViewModel, cancellationToken);

        RedirectToRouteResult? redirectToRouteResult = result.As<RedirectToRouteResult>();
        redirectToRouteResult.RouteName.Should().Be(RouteNames.YourTrainingProviders);
    }

    [Test, MoqAutoData]
    public async Task Post_ValidatedAndFailed_ReturnsExpectedModel(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock, [Frozen] Mock<IValidator<AddPermissionsSubmitModel>> validatorMock,
        [Greedy] AddPermissionsController sut, string employerAccountId,
        long ukprn,
        long legalEntityId,
        string legalName,
        string providerName,
        CancellationToken cancellationToken)
    {
        AddPermissionsSubmitModel submitViewModel = new AddPermissionsSubmitModel();
        validatorMock.Setup(m => m.Validate(It.IsAny<AddPermissionsSubmitModel>())).Returns(new ValidationResult(new List<ValidationFailure>()
        {
            new("TestField","Test Message") { ErrorCode = "1001"}
        }));

        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel { EmployerAccountId = employerAccountId, Ukprn = ukprn, SelectedLegalEntityId = legalEntityId, SelectedLegalName = legalName, ProviderName = providerName });

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);

        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);

        var result = await sut.Index(employerAccountId, submitViewModel, cancellationToken);

        ViewResult? viewResult = result.As<ViewResult>();
        AddPermissionsViewModel? viewModel = viewResult.Model as AddPermissionsViewModel;
        viewModel!.Ukprn.Should().Be(ukprn);
        viewModel.LegalEntityId.Should().Be(legalEntityId);
        viewModel.LegalName.Should().Be(legalName);
        viewModel.ProviderName.Should().Be(providerName);
        viewModel.CancelLink.Should().Be(YourTrainingProvidersLink);

        sessionServiceMock.Verify(s => s.Set(It.IsAny<AddTrainingProvidersSessionModel>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task Post_HasAccountsTaskInSession_RedirectsToAccounts(
        [Frozen] Mock<IValidator<AddPermissionsSubmitModel>> validatorMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IConfiguration> configurationMock,
        [Greedy] AddPermissionsController sut,
        long ukprn,
        long legalEntityId,
        string employerAccountId,
        string webUrl,
        CancellationToken cancellationToken
        )
    {
        /// arrange
        validatorMock.Setup(v => v.Validate(It.IsAny<AddPermissionsSubmitModel>())).Returns(new ValidationResult());

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
        sut.TempData = new TempDataDictionary(new DefaultHttpContext { User = user }, Mock.Of<ITempDataProvider>());

        AddPermissionsSubmitModel submitModel = new()
        {
            PermissionToAddCohorts = SetPermissions.AddRecords.Yes,
            PermissionToRecruit = SetPermissions.RecruitApprentices.No
        };

        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel
            { Ukprn = ukprn, SelectedLegalEntityId = legalEntityId, EmployerAccountId = employerAccountId });

        /// main setup
        sessionServiceMock.Setup(s => s.Contains(SessionKeys.AccountTasksKey)).Returns(true);
        configurationMock.Setup(c => c["EnvironmentName"]).Returns("LOCAL");
        configurationMock.Setup(c => c["EmployerAccountWebLocalUrl"]).Returns(webUrl);

        var result = await sut.Index(employerAccountId, submitModel, cancellationToken);

        result.As<RedirectResult>().Url.Contains("training-provider-success");
        sessionServiceMock.Verify(s => s.Delete(SessionKeys.AccountTasksKey), Times.Once);
        sut.TempData.ContainsKey(TempDataKeys.NameOfProviderAdded).Should().BeFalse();
    }

    [Test, MoqAutoData]
    public async Task Post_ValidRequest_RemovesSessionModel(
        [Frozen] Mock<IValidator<AddPermissionsSubmitModel>> validatorMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] AddPermissionsController sut,
        long ukprn,
        long legalEntityId,
        string employerAccountId,
        string webUrl,
        CancellationToken cancellationToken
        )
    {
        /// arrange
        validatorMock.Setup(v => v.Validate(It.IsAny<AddPermissionsSubmitModel>())).Returns(new ValidationResult());

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
        sut.TempData = new TempDataDictionary(new DefaultHttpContext { User = user }, Mock.Of<ITempDataProvider>());

        AddPermissionsSubmitModel submitModel = new()
        {
            PermissionToAddCohorts = SetPermissions.AddRecords.Yes,
            PermissionToRecruit = SetPermissions.RecruitApprentices.No
        };

        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel
            { Ukprn = ukprn, SelectedLegalEntityId = legalEntityId, EmployerAccountId = employerAccountId });

        await sut.Index(employerAccountId, submitModel, cancellationToken);

        sessionServiceMock.Verify(s => s.Delete<AddTrainingProvidersSessionModel>(), Times.Once);
    }
}
