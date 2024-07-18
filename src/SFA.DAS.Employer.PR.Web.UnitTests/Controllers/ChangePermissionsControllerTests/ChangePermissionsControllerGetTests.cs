using AutoFixture.NUnit3;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using RestEase;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Authentication;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Controllers;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Employer.PR.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;
using System.Net;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Controllers.ChangePermissionsControllerTests;
public class ChangePermissionsControllerGetTests
{
    static readonly string YourTrainingProvidersLink = Guid.NewGuid().ToString();

    [Test, MoqAutoData]
    public async Task ReturnsExpectedViewModel_NoOperations(
      string employerAccountId,
      long ukprn,
      long legalEntityId,
      GetPermissionsResponse getPermissionsResponse,
      CancellationToken cancellationToken)
    {
        var operations = new List<Operation>();
        getPermissionsResponse.Operations = operations;
        var outerApiClientMock = new Mock<IOuterApiClient>();

        outerApiClientMock.Setup(o => o.GetPermissions(ukprn, legalEntityId, cancellationToken))
            .ReturnsAsync(new Response<GetPermissionsResponse>(string.Empty, new(HttpStatusCode.OK), () => getPermissionsResponse));

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        ChangePermissionsController sut = new(outerApiClientMock.Object, Mock.Of<IValidator<ChangePermissionsSubmitViewModel>>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        var result = await sut.Index(employerAccountId, legalEntityId, ukprn, cancellationToken);

        ViewResult? viewResult = result.As<ViewResult>();
        ChangePermissionsViewModel? viewModel = viewResult.Model as ChangePermissionsViewModel;

        viewModel!.BackLink.Should().Be(YourTrainingProvidersLink);
        viewModel!.CancelLink.Should().Be(YourTrainingProvidersLink);
        viewModel.ProviderName.Should().Be(getPermissionsResponse.ProviderName);
        viewModel.LegalName.Should().Be(getPermissionsResponse.AccountLegalEntityName);
        viewModel.Ukprn.Should().Be(ukprn);
        viewModel.LegalEntityId.Should().Be(legalEntityId);
        viewModel.PermissionToAddCohorts.Should().Be(SetPermissions.AddRecords.No);
        viewModel.PermissionToAddCohortsOriginal.Should().Be(SetPermissions.AddRecords.No);
        viewModel.PermissionToRecruit.Should().Be(SetPermissions.RecruitApprentices.No);
        viewModel.PermissionToRecruitOriginal.Should().Be(SetPermissions.RecruitApprentices.No);
    }

    [Test]
    [InlineAutoData(Operation.CreateCohort, SetPermissions.AddRecords.Yes)]
    [InlineAutoData(null, SetPermissions.AddRecords.No)]
    public async Task ReturnsExpectedViewModel_AddRecords_OneOperation(
        Operation? operation,
        string expectedAddRecord,
        string employerAccountId,
        long ukprn,
        long legalEntityId,
        GetPermissionsResponse getPermissionsResponse,
        CancellationToken cancellationToken)
    {
        var operations = new List<Operation>();
        if (operation != null)
        {
            operations.Add(operation.Value);
        }

        getPermissionsResponse.Operations = operations;
        var outerApiClientMock = new Mock<IOuterApiClient>();

        outerApiClientMock.Setup(o => o.GetPermissions(ukprn, legalEntityId, cancellationToken))
            .ReturnsAsync(new Response<GetPermissionsResponse>(string.Empty, new(HttpStatusCode.OK), () => getPermissionsResponse));


        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        ChangePermissionsController sut =
            new(outerApiClientMock.Object, Mock.Of<IValidator<ChangePermissionsSubmitViewModel>>())
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
            };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        var result = await sut.Index(employerAccountId, legalEntityId, ukprn, cancellationToken);

        ViewResult? viewResult = result.As<ViewResult>();
        ChangePermissionsViewModel? viewModel = viewResult.Model as ChangePermissionsViewModel;

        viewModel!.PermissionToAddCohorts.Should().Be(expectedAddRecord);
        viewModel.PermissionToAddCohortsOriginal.Should().Be(expectedAddRecord);
    }

    [Test]
    [InlineAutoData(Operation.Recruitment, SetPermissions.RecruitApprentices.Yes)]
    [InlineAutoData(Operation.RecruitmentRequiresReview, SetPermissions.RecruitApprentices.YesWithReview)]
    [InlineAutoData(null, SetPermissions.RecruitApprentices.No)]
    public async Task ReturnsExpectedViewModel_AddRecruitment_OneOperation(
        Operation? operation,
        string expectedRecruitApprentices,
        string employerAccountId,
        long ukprn,
        long legalEntityId,
        GetPermissionsResponse getPermissionsResponse,
        CancellationToken cancellationToken)
    {
        var operations = new List<Operation>();
        if (operation != null)
        {
            operations.Add(operation.Value);
        }

        getPermissionsResponse.Operations = operations;
        var outerApiClientMock = new Mock<IOuterApiClient>();

        outerApiClientMock.Setup(o => o.GetPermissions(ukprn, legalEntityId, cancellationToken))
            .ReturnsAsync(new Response<GetPermissionsResponse>(string.Empty, new(HttpStatusCode.OK), () => getPermissionsResponse));

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        ChangePermissionsController sut =
            new(outerApiClientMock.Object, Mock.Of<IValidator<ChangePermissionsSubmitViewModel>>())
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
            };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        var result = await sut.Index(employerAccountId, legalEntityId, ukprn, cancellationToken);

        ViewResult? viewResult = result.As<ViewResult>();
        ChangePermissionsViewModel? viewModel = viewResult.Model as ChangePermissionsViewModel;

        viewModel!.PermissionToRecruit.Should().Be(expectedRecruitApprentices);
        viewModel.PermissionToRecruit.Should().Be(expectedRecruitApprentices);
    }

    [Test]
    [InlineAutoData(null, SetPermissions.AddRecords.No, Operation.Recruitment, SetPermissions.RecruitApprentices.Yes)]
    [InlineAutoData(null, SetPermissions.AddRecords.No, Operation.RecruitmentRequiresReview, SetPermissions.RecruitApprentices.YesWithReview)]
    [InlineAutoData(null, SetPermissions.AddRecords.No, null, SetPermissions.RecruitApprentices.No)]
    [InlineAutoData(Operation.CreateCohort, SetPermissions.AddRecords.Yes, Operation.Recruitment, SetPermissions.RecruitApprentices.Yes)]
    [InlineAutoData(Operation.CreateCohort, SetPermissions.AddRecords.Yes, Operation.RecruitmentRequiresReview, SetPermissions.RecruitApprentices.YesWithReview)]
    [InlineAutoData(Operation.CreateCohort, SetPermissions.AddRecords.Yes, null, SetPermissions.RecruitApprentices.No)]
    public async Task ReturnsExpectedViewModel_TwoOperations(
        Operation? addRecordsOperation,
        string expectedAddRecord,
        Operation? addRecruitmentOperation,
        string expectedRecruitApprentices,
        string employerAccountId,
        long ukprn,
        long legalEntityId,
        GetPermissionsResponse getPermissionsResponse,
        CancellationToken cancellationToken)
    {
        var operations = new List<Operation>();
        if (addRecordsOperation != null)
        {
            operations.Add(addRecordsOperation.Value);
        }
        if (addRecruitmentOperation != null)
        {
            operations.Add(addRecruitmentOperation.Value);
        }

        getPermissionsResponse.Operations = operations;
        var outerApiClientMock = new Mock<IOuterApiClient>();

        outerApiClientMock.Setup(o => o.GetPermissions(ukprn, legalEntityId, cancellationToken))
            .ReturnsAsync(new Response<GetPermissionsResponse>(string.Empty, new(HttpStatusCode.OK), () => getPermissionsResponse));

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        ChangePermissionsController sut =
            new(outerApiClientMock.Object, Mock.Of<IValidator<ChangePermissionsSubmitViewModel>>())
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
            };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        var result = await sut.Index(employerAccountId, legalEntityId, ukprn, cancellationToken);

        ViewResult? viewResult = result.As<ViewResult>();
        ChangePermissionsViewModel? viewModel = viewResult.Model as ChangePermissionsViewModel;

        viewModel!.PermissionToAddCohorts.Should().Be(expectedAddRecord);
        viewModel.PermissionToAddCohortsOriginal.Should().Be(expectedAddRecord);
        viewModel.PermissionToRecruit.Should().Be(expectedRecruitApprentices);
        viewModel.PermissionToRecruit.Should().Be(expectedRecruitApprentices);
    }
}
