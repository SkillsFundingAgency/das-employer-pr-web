using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using RestEase;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Domain.OuterApi.Permissions;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Authentication;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Controllers;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Employer.PR.Web.Services;
using SFA.DAS.Employer.PR.Web.UnitTests.TestHelpers;
using SFA.DAS.Encoding;
using SFA.DAS.Testing.AutoFixture;
using System.Net;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Controllers.ChangePermissionsControllerTests;

public class ChangePermissionsControllerPostTests
{
    static readonly string YourTrainingProvidersLink = Guid.NewGuid().ToString();

    [Test, MoqAutoData]
    public async Task Post_Validated_RedirectsToYourTrainingProviders(
        Mock<IValidator<ChangePermissionsSubmitViewModel>> validatorMock,
        string employerAccountId,
        long ukprn,
        long legalEntityId,
        GetPermissionsResponse getPermissionsResponse,
        CancellationToken cancellationToken)
    {
        ChangePermissionsSubmitViewModel submitViewModel = new ChangePermissionsSubmitViewModel
        {
            PermissionToAddCohorts = OperationsMappingService.Yes,
            PermissionToRecruit = OperationsMappingService.Yes,
            LegalEntityId = legalEntityId,
            Ukprn = ukprn
        };
        validatorMock.Setup(v => v.Validate(It.IsAny<ChangePermissionsSubmitViewModel>())).Returns(new ValidationResult());
        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);

        ProviderPermission permission = new()
        { Operations = new List<Operation>(), ProviderName = "provider name", Ukprn = 12345678 };
        permission.Operations.Add(Operation.CreateCohort);

        var outerApiClientMock = new Mock<IOuterApiClient>();

        outerApiClientMock.Setup(o => o.GetPermissions(ukprn, legalEntityId, cancellationToken))
            .ReturnsAsync(new Response<GetPermissionsResponse>(string.Empty, new(HttpStatusCode.OK), () => getPermissionsResponse));

        ChangePermissionsController sut = new(outerApiClientMock.Object, Mock.Of<IEncodingService>(), validatorMock.Object)
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

    [Test, MoqAutoData]
    public async Task Post_GetPermissionsDoesNotExist_RedirectsToYourTrainingProviders(
      Mock<IValidator<ChangePermissionsSubmitViewModel>> validatorMock,
      string employerAccountId,
      long ukprn,
      long legalEntityId,
      GetPermissionsResponse getPermissionsResponse,
      CancellationToken cancellationToken)
    {
        ChangePermissionsSubmitViewModel submitViewModel = new ChangePermissionsSubmitViewModel
        {
            PermissionToAddCohorts = OperationsMappingService.Yes,
            PermissionToRecruit = OperationsMappingService.Yes,
            LegalEntityId = legalEntityId,
            Ukprn = ukprn
        };
        validatorMock.Setup(v => v.Validate(It.IsAny<ChangePermissionsSubmitViewModel>())).Returns(new ValidationResult());
        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);

        ProviderPermission permission = new()
        { Operations = new List<Operation>(), ProviderName = "provider name", Ukprn = 12345678 };
        permission.Operations.Add(Operation.CreateCohort);

        var outerApiClientMock = new Mock<IOuterApiClient>();

        outerApiClientMock.Setup(o => o.GetPermissions(ukprn, legalEntityId, cancellationToken))
            .ReturnsAsync(new Response<GetPermissionsResponse>(string.Empty, new(HttpStatusCode.NotFound), () => getPermissionsResponse));

        ChangePermissionsController sut = new(outerApiClientMock.Object, Mock.Of<IEncodingService>(), validatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

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
    [MoqInlineAutoData(OperationsMappingService.No, OperationsMappingService.No)]
    public async Task Post_Validated_SetsTempData(
        string addRecords,
        string recruitApprentices,
        Mock<IValidator<ChangePermissionsSubmitViewModel>> validatorMock,
        string employerAccountId,
        long ukprn,
        long legalEntityId,
        string providerName,
        GetPermissionsResponse getPermissionsResponse,
        CancellationToken cancellationToken)
    {
        ChangePermissionsSubmitViewModel submitViewModel = new ChangePermissionsSubmitViewModel
        {
            PermissionToAddCohorts = addRecords,
            PermissionToRecruit = recruitApprentices,
            Ukprn = ukprn,
            LegalEntityId = legalEntityId
        };

        getPermissionsResponse.ProviderName = providerName;

        validatorMock.Setup(v => v.Validate(It.IsAny<ChangePermissionsSubmitViewModel>())).Returns(new ValidationResult());
        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);

        ProviderPermission permission = new()
        { Operations = new List<Operation>(), ProviderName = "provider name", Ukprn = 12345678 };
        permission.Operations.Add(Operation.CreateCohort);

        var tempData = new TempDataDictionary(new DefaultHttpContext { User = user }, Mock.Of<ITempDataProvider>());

        var outerApiClientMock = new Mock<IOuterApiClient>();

        outerApiClientMock.Setup(o => o.GetPermissions(ukprn, legalEntityId, cancellationToken))
            .ReturnsAsync(new Response<GetPermissionsResponse>(string.Empty, new(HttpStatusCode.OK), () => getPermissionsResponse));

        ChangePermissionsController sut = new(outerApiClientMock.Object, Mock.Of<IEncodingService>(), validatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };
        sut.TempData = tempData;

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        await sut.Index(employerAccountId, submitViewModel, cancellationToken);

        sut.TempData[TempDataKeys.NameOfProviderUpdated]!.ToString().Should().Be(providerName);
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
        Mock<IOuterApiClient> outerApiClientMock,
        Mock<IValidator<ChangePermissionsSubmitViewModel>> validatorMock,
        string employerAccountId,
        long ukprn,
        long legalEntityId,
        string providerName,
        GetPermissionsResponse getPermissionsResponse,
        CancellationToken cancellationToken)
    {
        ChangePermissionsSubmitViewModel submitViewModel = new ChangePermissionsSubmitViewModel
        {
            PermissionToAddCohorts = addRecords,
            PermissionToRecruit = recruitApprentices,
            Ukprn = ukprn,
            LegalEntityId = legalEntityId
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

        validatorMock.Setup(v => v.Validate(It.IsAny<ChangePermissionsSubmitViewModel>())).Returns(new ValidationResult());
        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);

        ProviderPermission permission = new()
        { Operations = new List<Operation>(), ProviderName = "provider name", Ukprn = 12345678 };
        permission.Operations.Add(Operation.CreateCohort);

        outerApiClientMock.Setup(o => o.GetPermissions(ukprn, legalEntityId, cancellationToken))
            .ReturnsAsync(new Response<GetPermissionsResponse>(string.Empty, new(HttpStatusCode.OK), () => getPermissionsResponse));

        ChangePermissionsController sut = new(outerApiClientMock.Object, Mock.Of<IEncodingService>(), validatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        Mock<ITempDataDictionary> tempDataMock = new();
        sut.TempData = tempDataMock.Object;

        await sut.Index(employerAccountId, submitViewModel, cancellationToken);

        var command = new PostPermissionsCommand(UsersForTesting.EmployerUserRef, ukprn, legalEntityId,
            expectedOperations);

        outerApiClientMock.Verify(o => o.PostPermissions(It.Is<PostPermissionsCommand>(
            c => c.UserRef == UsersForTesting.EmployerUserRef
                 && c.Ukprn == ukprn
                 && c.AccountLegalEntityId == legalEntityId
                 && c.Operations.Count == expectedOperations.Count
                 && c.Operations.Contains(expectedOperations.First())
        ), cancellationToken), Times.Once);
    }

    [Test]
    [MoqInlineAutoData]
    public async Task Post_Validated_RemovePermissions(
        string addRecords,
        string recruitApprentices,
        Mock<IOuterApiClient> outerApiClientMock,
        Mock<IValidator<ChangePermissionsSubmitViewModel>> validatorMock,
        string employerAccountId,
        long ukprn,
        long legalEntityId,
        string providerName,
        GetPermissionsResponse getPermissionsResponse,
        CancellationToken cancellationToken)
    {
        ChangePermissionsSubmitViewModel submitViewModel = new ChangePermissionsSubmitViewModel
        {
            PermissionToAddCohorts = addRecords,
            PermissionToRecruit = recruitApprentices,
            Ukprn = ukprn,
            LegalEntityId = legalEntityId
        };

        var expectedOperations = new List<Operation>();

        validatorMock.Setup(v => v.Validate(It.IsAny<ChangePermissionsSubmitViewModel>())).Returns(new ValidationResult());
        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);

        ProviderPermission permission = new()
        { Operations = new List<Operation>(), ProviderName = "provider name", Ukprn = 12345678 };
        permission.Operations.Add(Operation.CreateCohort);

        outerApiClientMock.Setup(o => o.GetPermissions(ukprn, legalEntityId, cancellationToken))
            .ReturnsAsync(new Response<GetPermissionsResponse>(string.Empty, new(HttpStatusCode.OK), () => getPermissionsResponse));

        ChangePermissionsController sut = new(outerApiClientMock.Object, Mock.Of<IEncodingService>(), validatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        Mock<ITempDataDictionary> tempDataMock = new();
        sut.TempData = tempDataMock.Object;

        await sut.Index(employerAccountId, submitViewModel, cancellationToken);

        var command = new PostPermissionsCommand(UsersForTesting.EmployerUserRef, ukprn, legalEntityId,
            expectedOperations);

        outerApiClientMock.Verify(o => o.PostPermissions(It.Is<PostPermissionsCommand>(
            c => c.UserRef == UsersForTesting.EmployerUserRef
                 && c.Ukprn == ukprn
                 && c.AccountLegalEntityId == legalEntityId
                 && c.Operations.Count == 0
        ), cancellationToken), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task Post_ValidatedAndFailed_ReturnsExpectedModel(
      Mock<IOuterApiClient> outerApiClientMock,
      Mock<IValidator<ChangePermissionsSubmitViewModel>> validatorMock,
      string employerAccountId,
      long ukprn,
      long legalEntityId,
      string legalName,
      string providerName,
      CancellationToken cancellationToken
  )
    {
        ChangePermissionsSubmitViewModel submitViewModel = new ChangePermissionsSubmitViewModel
        {
            PermissionToAddCohorts = OperationsMappingService.No,
            PermissionToRecruit = OperationsMappingService.No,
            Ukprn = ukprn,
            LegalEntityId = legalEntityId
        };

        GetPermissionsResponse getPermissionsResponse = new GetPermissionsResponse
        {
            Operations = new List<Operation>(),
            AccountLegalEntityName = legalName,
            ProviderName = providerName
        };

        validatorMock.Setup(m => m.Validate(It.IsAny<ChangePermissionsSubmitViewModel>())).Returns(new ValidationResult(new List<ValidationFailure>()
            {
                new("TestField","Test Message") { ErrorCode = "1001"}
            }));

        outerApiClientMock.Setup(o => o.GetPermissions(ukprn, legalEntityId, cancellationToken))
            .ReturnsAsync(new Response<GetPermissionsResponse>(string.Empty, new(HttpStatusCode.OK), () => getPermissionsResponse));


        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);

        ChangePermissionsController sut = new(outerApiClientMock.Object, Mock.Of<IEncodingService>(), validatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);

        var result = await sut.Index(employerAccountId, submitViewModel, cancellationToken);

        ViewResult? viewResult = result.As<ViewResult>();
        ChangePermissionsViewModel? viewModel = viewResult.Model as ChangePermissionsViewModel;
        viewModel!.Ukprn.Should().Be(ukprn);
        viewModel.LegalEntityId.Should().Be(legalEntityId);
        viewModel.LegalName.Should().Be(legalName);
        viewModel.ProviderName.Should().Be(providerName);
        viewModel.CancelLink.Should().Be(YourTrainingProvidersLink);
        viewModel.BackLink.Should().Be(YourTrainingProvidersLink);
    }
}