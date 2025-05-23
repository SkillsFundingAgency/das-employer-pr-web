﻿using AutoFixture.NUnit3;
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
using SFA.DAS.Employer.PR.Web.Services;
using SFA.DAS.Employer.PR.Web.UnitTests.TestHelpers;
using SFA.DAS.Encoding;
using SFA.DAS.Testing.AutoFixture;
using System.Net;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Controllers.ChangePermissionsControllerTests;
public class ChangePermissionsControllerGetTests
{
    static readonly string YourTrainingProvidersLink = Guid.NewGuid().ToString();


    [Test, MoqAutoData]
    public async Task Index_PermissionsDoNotExist_RedirectsToYourTrainingProviders(
      string employerAccountId,
      long ukprn,
      long legalEntityId,
      string legalEntityPublicHashedId,
      GetPermissionsResponse getPermissionsResponse,
      CancellationToken cancellationToken)
    {
        var operations = new List<Operation>();
        getPermissionsResponse.Operations = operations;
        var outerApiClientMock = new Mock<IOuterApiClient>();

        outerApiClientMock.Setup(o => o.GetPermissions(ukprn, legalEntityId, cancellationToken))
            .ReturnsAsync(new Response<GetPermissionsResponse>(string.Empty, new(HttpStatusCode.NotFound), () => getPermissionsResponse));

        var encodingServiceMock = new Mock<IEncodingService>();
        encodingServiceMock.Setup(x => x.Decode(legalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId)).Returns(legalEntityId);

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        ChangePermissionsController sut = new(outerApiClientMock.Object, encodingServiceMock.Object, Mock.Of<IValidator<ChangePermissionsSubmitModel>>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        var result = await sut.Index(employerAccountId, legalEntityPublicHashedId, ukprn, cancellationToken);

        RedirectToRouteResult? redirectToRouteResult = result.As<RedirectToRouteResult>();

        redirectToRouteResult.RouteName.Should().Be(RouteNames.YourTrainingProviders);
    }

    [Test, MoqAutoData]
    public async Task Index_ReturnsExpectedViewModel_ModelMatchesExpected(
      string employerAccountId,
      long ukprn,
      long legalEntityId,
      string legalEntityPublicHashedId,
      GetPermissionsResponse getPermissionsResponse,
      CancellationToken cancellationToken)
    {
        var operations = new List<Operation>();
        getPermissionsResponse.Operations = operations;
        var outerApiClientMock = new Mock<IOuterApiClient>();

        outerApiClientMock.Setup(o => o.GetPermissions(ukprn, legalEntityId, cancellationToken))
            .ReturnsAsync(new Response<GetPermissionsResponse>(string.Empty, new(HttpStatusCode.OK), () => getPermissionsResponse));

        var encodingServiceMock = new Mock<IEncodingService>();
        encodingServiceMock.Setup(x => x.Decode(legalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId)).Returns(legalEntityId);

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        ChangePermissionsController sut = new(outerApiClientMock.Object, encodingServiceMock.Object, Mock.Of<IValidator<ChangePermissionsSubmitModel>>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        var result = await sut.Index(employerAccountId, legalEntityPublicHashedId, ukprn, cancellationToken);

        ViewResult? viewResult = result.As<ViewResult>();
        ChangePermissionsViewModel? viewModel = viewResult.Model as ChangePermissionsViewModel;

        Assert.Multiple(() =>
        {
            Assert.That(viewModel!.BackLink, Is.EqualTo(YourTrainingProvidersLink));
            Assert.That(viewModel.ProviderName, Is.EqualTo(getPermissionsResponse.ProviderName));
            Assert.That(viewModel.LegalName, Is.EqualTo(getPermissionsResponse.AccountLegalEntityName));
            Assert.That(viewModel.Ukprn, Is.EqualTo(ukprn));
            Assert.That(viewModel.LegalEntityId, Is.EqualTo(legalEntityId));
            Assert.That(viewModel.PermissionToAddCohorts, Is.EqualTo(OperationsMappingService.No));
            Assert.That(viewModel.PermissionToAddCohortsOriginal, Is.EqualTo(OperationsMappingService.No));
            Assert.That(viewModel.PermissionToRecruit, Is.EqualTo(OperationsMappingService.No));
            Assert.That(viewModel.PermissionToRecruitOriginal, Is.EqualTo(OperationsMappingService.No));
        });
    }

    [Test]
    [InlineAutoData(Operation.CreateCohort, OperationsMappingService.Yes)]
    [InlineAutoData(null, OperationsMappingService.No)]
    public async Task Index_ReturnsExpectedAddRecordOperation_MatchesExpected(
        Operation? operation,
        string expectedAddRecord,
        string employerAccountId,
        long ukprn,
        long legalEntityId,
        string legalEntityPublicHashedId,
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

        var encodingServiceMock = new Mock<IEncodingService>();
        encodingServiceMock.Setup(x => x.Decode(legalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId)).Returns(legalEntityId);

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        ChangePermissionsController sut =
            new(outerApiClientMock.Object, encodingServiceMock.Object, Mock.Of<IValidator<ChangePermissionsSubmitModel>>())
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
            };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        var result = await sut.Index(employerAccountId, legalEntityPublicHashedId, ukprn, cancellationToken);

        ViewResult? viewResult = result.As<ViewResult>();
        ChangePermissionsViewModel? viewModel = viewResult.Model as ChangePermissionsViewModel;

        viewModel!.PermissionToAddCohorts.Should().Be(expectedAddRecord);
        viewModel.PermissionToAddCohortsOriginal.Should().Be(expectedAddRecord);
    }

    [Test]
    [InlineAutoData(Operation.Recruitment, OperationsMappingService.Yes)]
    [InlineAutoData(Operation.RecruitmentRequiresReview, OperationsMappingService.YesWithReview)]
    [InlineAutoData(null, OperationsMappingService.No)]
    public async Task Index_ReturnsExpectedAddRecruitment_MatchesExpected(
        Operation? operation,
        string expectedRecruitApprentices,
        string employerAccountId,
        long ukprn,
        long legalEntityId,
        string legalEntityPublicHashedId,
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

        var encodingServiceMock = new Mock<IEncodingService>();
        encodingServiceMock.Setup(x => x.Decode(legalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId)).Returns(legalEntityId);

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        ChangePermissionsController sut =
            new(outerApiClientMock.Object, encodingServiceMock.Object, Mock.Of<IValidator<ChangePermissionsSubmitModel>>())
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
            };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        var result = await sut.Index(employerAccountId, legalEntityPublicHashedId, ukprn, cancellationToken);

        ViewResult? viewResult = result.As<ViewResult>();
        ChangePermissionsViewModel? viewModel = viewResult.Model as ChangePermissionsViewModel;

        viewModel!.PermissionToRecruit.Should().Be(expectedRecruitApprentices);
        viewModel.PermissionToRecruit.Should().Be(expectedRecruitApprentices);
    }

    [Test]
    [InlineAutoData(null, OperationsMappingService.No, Operation.Recruitment, OperationsMappingService.Yes)]
    [InlineAutoData(null, OperationsMappingService.No, Operation.RecruitmentRequiresReview,
        OperationsMappingService.YesWithReview)]
    [InlineAutoData(null, OperationsMappingService.No, null, OperationsMappingService.No)]
    [InlineAutoData(Operation.CreateCohort, OperationsMappingService.Yes, Operation.Recruitment,
        OperationsMappingService.Yes)]
    [InlineAutoData(Operation.CreateCohort, OperationsMappingService.Yes, Operation.RecruitmentRequiresReview,
        OperationsMappingService.YesWithReview)]
    [InlineAutoData(Operation.CreateCohort, OperationsMappingService.Yes, null, OperationsMappingService.No)]
    public async Task Index_ReturnsExpectedOperations_MatchesExpected(
        Operation? addRecordsOperation,
        string expectedAddRecord,
        Operation? addRecruitmentOperation,
        string expectedRecruitApprentices,
        string employerAccountId,
        long ukprn,
        long legalEntityId,
        string legalEntityPublicHashedId,
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
            .ReturnsAsync(new Response<GetPermissionsResponse>(string.Empty, new(HttpStatusCode.OK),
                () => getPermissionsResponse));

        var encodingServiceMock = new Mock<IEncodingService>();
        encodingServiceMock.Setup(x => x.Decode(legalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId))
            .Returns(legalEntityId);

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        ChangePermissionsController sut =
            new(outerApiClientMock.Object, encodingServiceMock.Object,
                Mock.Of<IValidator<ChangePermissionsSubmitModel>>())
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
            };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        var result = await sut.Index(employerAccountId, legalEntityPublicHashedId, ukprn, cancellationToken);

        ViewResult? viewResult = result.As<ViewResult>();
        ChangePermissionsViewModel? viewModel = viewResult.Model as ChangePermissionsViewModel;

        Assert.Multiple(() =>
        {
            Assert.That(viewModel!.PermissionToAddCohorts, Is.EqualTo(expectedAddRecord));
            Assert.That(viewModel.PermissionToAddCohortsOriginal, Is.EqualTo(expectedAddRecord));
            Assert.That(viewModel.PermissionToRecruit, Is.EqualTo(expectedRecruitApprentices));
            Assert.That(viewModel.PermissionToRecruit, Is.EqualTo(expectedRecruitApprentices));
        });
    }
}
