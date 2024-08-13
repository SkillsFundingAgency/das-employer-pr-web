using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
using SFA.DAS.Employer.PR.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Controllers.AddPermissionsControllerTests;

public class AddPermissionsControllerPostTests
{

    static readonly string YourTrainingProvidersLink = Guid.NewGuid().ToString();

    [Test, MoqAutoData]
    public async Task Post_Validated_ReturnsExpectedModel(
        Mock<IValidator<AddPermissionsSubmitViewViewModel>> validatorMock,
        string employerAccountId,
        long ukprn,
        long legalEntityId,
        CancellationToken cancellationToken)
    {
        var sessionServiceMock = new Mock<ISessionService>();

        AddPermissionsSubmitViewViewModel submitViewModel = new AddPermissionsSubmitViewViewModel
        {
            PermissionToAddCohorts = SetPermissions.AddRecords.Yes,
            PermissionToRecruit = SetPermissions.RecruitApprentices.Yes
        };

        validatorMock.Setup(v => v.Validate(It.IsAny<AddPermissionsSubmitViewViewModel>())).Returns(new ValidationResult());
        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);

        Permission permission = new()
        { Operations = new List<Operation>(), ProviderName = "provider name", Ukprn = 12345678 };
        permission.Operations.Add(Operation.CreateCohort);

        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel
            { Ukprn = ukprn, SelectedLegalEntityId = legalEntityId, EmployerAccountId = employerAccountId });

        AddPermissionsController sut = new(Mock.Of<IOuterApiClient>(), sessionServiceMock.Object, validatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        Mock<ITempDataDictionary> tempDataMock = new();
        sut.TempData = tempDataMock.Object;
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        var result = await sut.Index(employerAccountId, submitViewModel, cancellationToken);

        RedirectToRouteResult? redirectToRouteResult = result.As<RedirectToRouteResult>();

        redirectToRouteResult.RouteName.Should().Be(RouteNames.YourTrainingProviders);
    }

    [Test]
    [MoqInlineAutoData(SetPermissions.AddRecords.Yes, SetPermissions.RecruitApprentices.Yes)]
    [MoqInlineAutoData(SetPermissions.AddRecords.Yes, SetPermissions.RecruitApprentices.YesWithReview)]
    [MoqInlineAutoData(SetPermissions.AddRecords.Yes, SetPermissions.RecruitApprentices.No)]
    [MoqInlineAutoData(SetPermissions.AddRecords.No, SetPermissions.RecruitApprentices.Yes)]
    [MoqInlineAutoData(SetPermissions.AddRecords.No, SetPermissions.RecruitApprentices.YesWithReview)]
    public async Task Post_Validated_SetsTempData(
        string addRecords,
        string recruitApprentices,
        Mock<IValidator<AddPermissionsSubmitViewViewModel>> validatorMock,
        string employerAccountId,
        long ukprn,
        long legalEntityId,
        string providerName,
        CancellationToken cancellationToken)
    {
        var sessionServiceMock = new Mock<ISessionService>();

        AddPermissionsSubmitViewViewModel submitViewModel = new AddPermissionsSubmitViewViewModel
        {
            PermissionToAddCohorts = addRecords,
            PermissionToRecruit = recruitApprentices
        };

        validatorMock.Setup(v => v.Validate(It.IsAny<AddPermissionsSubmitViewViewModel>())).Returns(new ValidationResult());
        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);

        Permission permission = new()
        { Operations = new List<Operation>(), ProviderName = "provider name", Ukprn = 12345678 };
        permission.Operations.Add(Operation.CreateCohort);

        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel
            { Ukprn = ukprn, SelectedLegalEntityId = legalEntityId, EmployerAccountId = employerAccountId, ProviderName = providerName });

        var tempData = new TempDataDictionary(new DefaultHttpContext { User = user }, Mock.Of<ITempDataProvider>());

        AddPermissionsController sut = new(Mock.Of<IOuterApiClient>(), sessionServiceMock.Object, validatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };
        sut.TempData = tempData;

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        await sut.Index(employerAccountId, submitViewModel, cancellationToken);

        sut.TempData[TempDataKeys.NameOfProviderAdded]!.ToString().Should().Be(providerName);
    }

    [Test]
    [MoqInlineAutoData(SetPermissions.AddRecords.Yes, SetPermissions.RecruitApprentices.Yes)]
    [MoqInlineAutoData(SetPermissions.AddRecords.Yes, SetPermissions.RecruitApprentices.YesWithReview)]
    [MoqInlineAutoData(SetPermissions.AddRecords.Yes, SetPermissions.RecruitApprentices.No)]
    [MoqInlineAutoData(SetPermissions.AddRecords.No, SetPermissions.RecruitApprentices.Yes)]
    [MoqInlineAutoData(SetPermissions.AddRecords.No, SetPermissions.RecruitApprentices.YesWithReview)]
    public async Task Post_Validated_PostExpectedValues(
        string addRecords,
        string recruitApprentices,
        Mock<IOuterApiClient> outerApiClientMock,
        Mock<IValidator<AddPermissionsSubmitViewViewModel>> validatorMock,
        string employerAccountId,
        long ukprn,
        long legalEntityId,
        CancellationToken cancellationToken)
    {
        var sessionServiceMock = new Mock<ISessionService>();

        AddPermissionsSubmitViewViewModel submitViewModel = new AddPermissionsSubmitViewViewModel
        {
            PermissionToAddCohorts = addRecords,
            PermissionToRecruit = recruitApprentices
        };

        var expectedOperations = new List<Operation>();

        if (addRecords == SetPermissions.AddRecords.Yes)
        {
            expectedOperations.Add(Operation.CreateCohort);
        }

        switch (recruitApprentices)
        {
            case SetPermissions.RecruitApprentices.Yes:
                expectedOperations.Add(Operation.Recruitment);
                break;
            case SetPermissions.RecruitApprentices.YesWithReview:
                expectedOperations.Add(Operation.RecruitmentRequiresReview);
                break;
        }

        validatorMock.Setup(v => v.Validate(It.IsAny<AddPermissionsSubmitViewViewModel>())).Returns(new ValidationResult());
        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);

        Permission permission = new()
        { Operations = new List<Operation>(), ProviderName = "provider name", Ukprn = 12345678 };
        permission.Operations.Add(Operation.CreateCohort);

        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel
            { Ukprn = ukprn, SelectedLegalEntityId = legalEntityId, EmployerAccountId = employerAccountId });

        AddPermissionsController sut = new(outerApiClientMock.Object, sessionServiceMock.Object, validatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        Mock<ITempDataDictionary> tempDataMock = new();
        sut.TempData = tempDataMock.Object;

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
        Mock<IValidator<AddPermissionsSubmitViewViewModel>> validatorMock,
        string employerAccountId,
        long ukprn,
        long legalEntityId,
        CancellationToken cancellationToken)
    {
        var sessionServiceMock = new Mock<ISessionService>();

        AddPermissionsSubmitViewViewModel submitViewModel = new AddPermissionsSubmitViewViewModel
        {
            PermissionToAddCohorts = SetPermissions.AddRecords.Yes,
            PermissionToRecruit = SetPermissions.RecruitApprentices.Yes
        };

        validatorMock.Setup(v => v.Validate(It.IsAny<AddPermissionsSubmitViewViewModel>())).Returns(new ValidationResult());
        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);

        Permission permission = new()
        { Operations = new List<Operation>(), ProviderName = "provider name", Ukprn = 12345678 };
        permission.Operations.Add(Operation.CreateCohort);

        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns((AddTrainingProvidersSessionModel)null!);

        AddPermissionsController sut = new(Mock.Of<IOuterApiClient>(), sessionServiceMock.Object, validatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        var result = await sut.Index(employerAccountId, submitViewModel, cancellationToken);

        RedirectToRouteResult? redirectToRouteResult = result.As<RedirectToRouteResult>();
        redirectToRouteResult.RouteName.Should().Be(RouteNames.YourTrainingProviders);
    }

    [Test, MoqAutoData]
    public async Task Post_Validated_EmployerAccountIdNotMatched(
        Mock<IValidator<AddPermissionsSubmitViewViewModel>> validatorMock,
        string employerAccountId,
        CancellationToken cancellationToken)
    {
        var sessionServiceMock = new Mock<ISessionService>();

        AddPermissionsSubmitViewViewModel submitViewModel = new AddPermissionsSubmitViewViewModel
        {
            PermissionToAddCohorts = SetPermissions.AddRecords.Yes,
            PermissionToRecruit = SetPermissions.RecruitApprentices.Yes
        };

        validatorMock.Setup(v => v.Validate(It.IsAny<AddPermissionsSubmitViewViewModel>())).Returns(new ValidationResult());
        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);

        Permission permission = new()
        { Operations = new List<Operation>(), ProviderName = "provider name", Ukprn = 12345678 };
        permission.Operations.Add(Operation.CreateCohort);

        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel { EmployerAccountId = $"{employerAccountId}_other" });

        AddPermissionsController sut = new(Mock.Of<IOuterApiClient>(), sessionServiceMock.Object, validatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        var result = await sut.Index(employerAccountId, submitViewModel, cancellationToken);


        RedirectToRouteResult? redirectToRouteResult = result.As<RedirectToRouteResult>();
        redirectToRouteResult.RouteName.Should().Be(RouteNames.YourTrainingProviders);
    }

    [Test, MoqAutoData]
    public async Task Post_ValidatedAndFailed_ReturnsExpectedModel(
        Mock<IOuterApiClient> outerApiClientMock,
        Mock<IValidator<AddPermissionsSubmitViewViewModel>> validatorMock,
        string employerAccountId,
        long ukprn,
        long legalEntityId,
        string legalName,
        string providerName,
        CancellationToken cancellationToken
    )
    {
        var sessionServiceMock = new Mock<ISessionService>();
        AddPermissionsSubmitViewViewModel submitViewModel = new AddPermissionsSubmitViewViewModel();
        validatorMock.Setup(m => m.Validate(It.IsAny<AddPermissionsSubmitViewViewModel>())).Returns(new ValidationResult(new List<ValidationFailure>()
        {
            new("TestField","Test Message") { ErrorCode = "1001"}
        }));

        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel { EmployerAccountId = employerAccountId, Ukprn = ukprn, SelectedLegalEntityId = legalEntityId, SelectedLegalName = legalName, ProviderName = providerName });

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);

        AddPermissionsController sut = new(outerApiClientMock.Object, sessionServiceMock.Object, validatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

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
}
