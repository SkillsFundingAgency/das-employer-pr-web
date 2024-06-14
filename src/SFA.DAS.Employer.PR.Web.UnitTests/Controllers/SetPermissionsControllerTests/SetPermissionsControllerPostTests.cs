using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
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

namespace SFA.DAS.Employer.PR.Web.UnitTests.Controllers.SetPermissionsControllerTests;

public class SetPermissionsControllerPostTests
{

    static readonly string YourTrainingProvidersLink = Guid.NewGuid().ToString();

    [Test, MoqAutoData]
    public async Task Post_Validated_ReturnsExpectedModel(
        Mock<IValidator<SetPermissionsSubmitViewModel>> validatorMock,
        string employerAccountId,
        long ukprn,
        long legalEntityId,
        CancellationToken cancellationToken)
    {
        var sessionServiceMock = new Mock<ISessionService>();

        SetPermissionsSubmitViewModel submitModel = new SetPermissionsSubmitViewModel
        {
            AddRecords = SetPermissions.AddRecords.Yes,
            RecruitApprentices = SetPermissions.RecruitApprentices.Yes
        };

        validatorMock.Setup(v => v.Validate(It.IsAny<SetPermissionsSubmitViewModel>())).Returns(new ValidationResult());
        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);

        Permission permission = new()
        { Operations = new List<Operation>(), ProviderName = "provider name", Ukprn = 12345678 };
        permission.Operations.Add(Operation.CreateCohort);

        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel
            { Ukprn = ukprn, LegalEntityId = legalEntityId, EmployerAccountId = employerAccountId });

        SetPermissionsController sut = new(Mock.Of<IOuterApiClient>(), sessionServiceMock.Object, validatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        var result = await sut.Index(employerAccountId, submitModel, cancellationToken);

        RedirectToActionResult? redirectToActionResult = result.As<RedirectToActionResult>();

        redirectToActionResult.ActionName.Should().Be("Index");
        redirectToActionResult.ControllerName.Should().Be("YourTrainingProviders");
    }

    [Test]
    [MoqInlineAutoData(SetPermissions.AddRecords.Yes, SetPermissions.RecruitApprentices.Yes)]
    [MoqInlineAutoData(SetPermissions.AddRecords.Yes, SetPermissions.RecruitApprentices.YesWithReview)]
    [MoqInlineAutoData(SetPermissions.AddRecords.Yes, SetPermissions.RecruitApprentices.No)]
    [MoqInlineAutoData(SetPermissions.AddRecords.No, SetPermissions.RecruitApprentices.Yes)]
    [MoqInlineAutoData(SetPermissions.AddRecords.No, SetPermissions.RecruitApprentices.YesWithReview)]
    public async Task Post_Validated_SetsSessionModel(
        string addRecords,
        string recruitApprentices,
        Mock<IValidator<SetPermissionsSubmitViewModel>> validatorMock,
        string employerAccountId,
        long ukprn,
        long legalEntityId,
        CancellationToken cancellationToken)
    {
        var sessionServiceMock = new Mock<ISessionService>();

        SetPermissionsSubmitViewModel submitModel = new SetPermissionsSubmitViewModel
        {
            AddRecords = addRecords,
            RecruitApprentices = recruitApprentices
        };

        validatorMock.Setup(v => v.Validate(It.IsAny<SetPermissionsSubmitViewModel>())).Returns(new ValidationResult());
        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);

        Permission permission = new()
        { Operations = new List<Operation>(), ProviderName = "provider name", Ukprn = 12345678 };
        permission.Operations.Add(Operation.CreateCohort);

        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel
            { Ukprn = ukprn, LegalEntityId = legalEntityId, EmployerAccountId = employerAccountId });

        SetPermissionsController sut = new(Mock.Of<IOuterApiClient>(), sessionServiceMock.Object, validatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        await sut.Index(employerAccountId, submitModel, cancellationToken);

        sessionServiceMock.Verify(s => s.Set(It.Is<AddTrainingProvidersSessionModel>(s => s.AddRecords == addRecords
            && s.RecruitApprentices == recruitApprentices
            && s.SuccessfulAddition == true)), Times.Once);
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
        Mock<IValidator<SetPermissionsSubmitViewModel>> validatorMock,
        string employerAccountId,
        long ukprn,
        long legalEntityId,
        CancellationToken cancellationToken)
    {
        var sessionServiceMock = new Mock<ISessionService>();

        SetPermissionsSubmitViewModel submitModel = new SetPermissionsSubmitViewModel
        {
            AddRecords = addRecords,
            RecruitApprentices = recruitApprentices
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

        validatorMock.Setup(v => v.Validate(It.IsAny<SetPermissionsSubmitViewModel>())).Returns(new ValidationResult());
        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);

        Permission permission = new()
        { Operations = new List<Operation>(), ProviderName = "provider name", Ukprn = 12345678 };
        permission.Operations.Add(Operation.CreateCohort);

        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel
            { Ukprn = ukprn, LegalEntityId = legalEntityId, EmployerAccountId = employerAccountId });

        SetPermissionsController sut = new(outerApiClientMock.Object, sessionServiceMock.Object, validatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);

        await sut.Index(employerAccountId, submitModel, cancellationToken);

        var command = new PostPermissionsCommand(UsersForTesting.NameIdentifierValue, ukprn, legalEntityId,
            expectedOperations);

        outerApiClientMock.Verify(o => o.PostPermissions(It.Is<PostPermissionsCommand>(
            c => c.UserRef == UsersForTesting.NameIdentifierValue
                 && c.Ukprn == ukprn
                 && c.AccountLegalEntityId == legalEntityId
                 && c.Operations.Count == expectedOperations.Count
                 && c.Operations.Contains(expectedOperations.First())
        ), cancellationToken), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task Post_Validated_SessionModelExpired(
        Mock<IValidator<SetPermissionsSubmitViewModel>> validatorMock,
        string employerAccountId,
        long ukprn,
        long legalEntityId,
        CancellationToken cancellationToken)
    {
        var sessionServiceMock = new Mock<ISessionService>();

        SetPermissionsSubmitViewModel submitModel = new SetPermissionsSubmitViewModel
        {
            AddRecords = SetPermissions.AddRecords.Yes,
            RecruitApprentices = SetPermissions.RecruitApprentices.Yes
        };

        validatorMock.Setup(v => v.Validate(It.IsAny<SetPermissionsSubmitViewModel>())).Returns(new ValidationResult());
        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);

        Permission permission = new()
        { Operations = new List<Operation>(), ProviderName = "provider name", Ukprn = 12345678 };
        permission.Operations.Add(Operation.CreateCohort);

        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns((AddTrainingProvidersSessionModel)null!);

        SetPermissionsController sut = new(Mock.Of<IOuterApiClient>(), sessionServiceMock.Object, validatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        var result = await sut.Index(employerAccountId, submitModel, cancellationToken);

        RedirectToActionResult? redirectToActionResult = result.As<RedirectToActionResult>();

        redirectToActionResult.ActionName.Should().Be("Index");
        redirectToActionResult.ControllerName.Should().Be("YourTrainingProviders");
    }

    [Test, MoqAutoData]
    public async Task Post_Validated_EmployerAccountIdNotMatched(
        Mock<IValidator<SetPermissionsSubmitViewModel>> validatorMock,
        string employerAccountId,
        CancellationToken cancellationToken)
    {
        var sessionServiceMock = new Mock<ISessionService>();

        SetPermissionsSubmitViewModel submitModel = new SetPermissionsSubmitViewModel
        {
            AddRecords = SetPermissions.AddRecords.Yes,
            RecruitApprentices = SetPermissions.RecruitApprentices.Yes
        };

        validatorMock.Setup(v => v.Validate(It.IsAny<SetPermissionsSubmitViewModel>())).Returns(new ValidationResult());
        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);

        Permission permission = new()
        { Operations = new List<Operation>(), ProviderName = "provider name", Ukprn = 12345678 };
        permission.Operations.Add(Operation.CreateCohort);

        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel { EmployerAccountId = $"{employerAccountId}_other" });

        SetPermissionsController sut = new(Mock.Of<IOuterApiClient>(), sessionServiceMock.Object, validatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        var result = await sut.Index(employerAccountId, submitModel, cancellationToken);

        RedirectToActionResult? redirectToActionResult = result.As<RedirectToActionResult>();

        redirectToActionResult.ActionName.Should().Be("Index");
        redirectToActionResult.ControllerName.Should().Be("YourTrainingProviders");
    }

    [Test, MoqAutoData]
    public async Task Post_ValidatedAndFailed_ReturnsExpectedModel(
        Mock<IOuterApiClient> outerApiClientMock,
        Mock<IValidator<SetPermissionsSubmitViewModel>> validatorMock,
        string employerAccountId,
        long ukprn,
        long legalEntityId,
        string legalName,
        string providerName,
        CancellationToken cancellationToken
    )
    {
        var sessionServiceMock = new Mock<ISessionService>();
        SetPermissionsSubmitViewModel submitModel = new SetPermissionsSubmitViewModel();
        validatorMock.Setup(m => m.Validate(It.IsAny<SetPermissionsSubmitViewModel>())).Returns(new ValidationResult(new List<ValidationFailure>()
        {
            new("TestField","Test Message") { ErrorCode = "1001"}
        }));

        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel { EmployerAccountId = employerAccountId, Ukprn = ukprn, LegalEntityId = legalEntityId, LegalName = legalName, ProviderName = providerName });

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);

        SetPermissionsController sut = new(outerApiClientMock.Object, sessionServiceMock.Object, validatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);

        var result = await sut.Index(employerAccountId, submitModel, cancellationToken);

        ViewResult? viewResult = result.As<ViewResult>();
        SetPermissionsViewModel? viewModel = viewResult.Model as SetPermissionsViewModel;
        viewModel!.Ukprn.Should().Be(ukprn);
        viewModel.LegalEntityId.Should().Be(legalEntityId);
        viewModel.LegalName.Should().Be(legalName);
        viewModel.ProviderName.Should().Be(providerName);
        viewModel.CancelLink.Should().Be(YourTrainingProvidersLink);

        sessionServiceMock.Verify(s => s.Set(It.IsAny<AddTrainingProvidersSessionModel>()), Times.Never);
    }
}
