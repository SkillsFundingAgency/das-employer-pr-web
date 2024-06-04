using AutoFixture.NUnit3;
using FluentValidation;
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
public class SelectLegalEntitiesControllerGetTests
{
    static readonly string YourTrainingProvidersLink = Guid.NewGuid().ToString();

    [Test, MoqAutoData]
    public void SelectLegalEntitiesGet_CallsOuterApiEndpoint(
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        GetEmployerRelationshipsQueryResponse response,
        string employerAccountId
    )
    {
        outerApiMock.Setup(o => o.GetAccountLegalEntities(employerAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        SelectLegalEntityController sut = new(outerApiMock.Object, Mock.Of<ISessionService>(), Mock.Of<IValidator<SelectLegalEntitiesSubmitViewModel>>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);

        var result = sut.Index(employerAccountId, new CancellationToken());
        outerApiMock.Verify(o => o.GetAccountLegalEntities(employerAccountId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test, MoqAutoData]
    public void ReturnsExpectedLegalEntityModel(
        [Frozen] Mock<IOuterApiClient> outerApiMock,
       string employerAccountId,
       int accountId,
       string accountName,
       string publicHashedId)
    {
        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        SelectLegalEntityController sut = new(outerApiMock.Object, Mock.Of<ISessionService>(), Mock.Of<IValidator<SelectLegalEntitiesSubmitViewModel>>())
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
        Task<IActionResult> result = sut.Index(employerAccountId, new CancellationToken());

        ViewResult? viewResult = result.Result.As<ViewResult>();
        SelectLegalEntitiesViewModel? viewModel = viewResult.Model as SelectLegalEntitiesViewModel;

        viewModel!.LegalEntities.Count.Should().Be(1);
        viewModel.BackLink.Should().Be(YourTrainingProvidersLink);
        viewModel.CancelUrl.Should().Be(YourTrainingProvidersLink);
        LegalEntityModel actualLegalEntity = viewModel.LegalEntities.First();
        actualLegalEntity.AccountId.Should().Be(accountId);
        actualLegalEntity.Name.Should().Be(accountName);
        actualLegalEntity.LegalEntityPublicHashedId.Should().Be(publicHashedId);
    }

    [Test, MoqInlineAutoData]
    public void CallsSessionServiceGet(
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        string employerAccountId,
        string providerName,
        long ukprn
    )
    {
        var permission = new Permission { Operations = new List<Operation>(), ProviderName = providerName, Ukprn = ukprn };
        permission.Operations.Add(Operation.CreateCohort);
        permission.Operations.Add(Operation.Recruitment);

        var permissions = new List<Permission> { permission };
        var accountLegalEntities = new List<AccountLegalEntity>
        {
            new() {AccountId = 1123,Id=1, Name="account name", PublicHashedId = "12123232", Permissions = permissions}
        };

        GetEmployerRelationshipsQueryResponse response = new GetEmployerRelationshipsQueryResponse(accountLegalEntities);

        outerApiMock.Setup(o => o.GetAccountLegalEntities(employerAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        SelectLegalEntityController sut = new(outerApiMock.Object, sessionServiceMock.Object, Mock.Of<IValidator<SelectLegalEntitiesSubmitViewModel>>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        var result = sut.Index(employerAccountId, new CancellationToken());
        sessionServiceMock.Verify(s => s.Get<AddTrainingProvidersSessionModel>(), Times.Once);
    }
}
