using AutoFixture.NUnit3;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
using SFA.DAS.Encoding;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Controllers.SelectLegalEntityControllerTests;
public class SelectLegalEntityControllerGetTests
{
    static readonly string YourTrainingProvidersLink = Guid.NewGuid().ToString();

    [Test, MoqAutoData]
    public async Task ReturnsExpectedLegalEntityModel_MatchingMultipleLegalEntities(
        string employerAccountId,
        int accountId,
        string accountName,
        string publicHashedId,
        CancellationToken cancellationToken)
    {
        Permission permission = new() { Operations = new(), ProviderName = "provider name", Ukprn = 12345678 };
        permission.Operations.Add(Operation.CreateCohort);

        List<Permission> permissions = new() { permission };
        List<AccountLegalEntity> accountLegalEntities = new List<AccountLegalEntity>
        {
            new() {AccountId = accountId,Id=1 , Name=accountName, PublicHashedId = publicHashedId, Permissions = permissions},
            new() {AccountId = 1123,Id=1, Name= $"{accountName}x", PublicHashedId = "12123232", Permissions = permissions}
        };
        var sessionServiceMock = new Mock<ISessionService>();
        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel { EmployerAccountId = employerAccountId, AccountLegalEntities = accountLegalEntities });

        var outerApiClientMock = new Mock<IOuterApiClient>();
        outerApiClientMock.Setup(o => o.GetAccountLegalEntities(It.IsAny<long>(), cancellationToken)).ReturnsAsync(new GetEmployerRelationshipsQueryResponse(accountLegalEntities));

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        SelectLegalEntityController sut = new(outerApiClientMock.Object, sessionServiceMock.Object, Mock.Of<IValidator<SelectLegalEntitiesSubmitViewModel>>(), Mock.Of<IEncodingService>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        Mock<ITempDataDictionary> tempDataMock = new();
        sut.TempData = tempDataMock.Object;
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        var result = await sut.Index(employerAccountId, cancellationToken);

        ViewResult? viewResult = result.As<ViewResult>();
        SelectLegalEntitiesViewModel? viewModel = viewResult.Model as SelectLegalEntitiesViewModel;

        viewModel!.LegalEntities.Count.Should().Be(2);
        viewModel.BackLink.Should().Be(YourTrainingProvidersLink);
        LegalEntityModel actualLegalEntity = viewModel.LegalEntities.First();
        actualLegalEntity.AccountId.Should().Be(accountId);
        actualLegalEntity.Name.Should().Be(accountName);
        actualLegalEntity.LegalEntityPublicHashedId.Should().Be(publicHashedId);
    }

    [Test, MoqAutoData]
    public async Task ReturnsExpectedLegalEntityModel_MatchingSingleLegalEntity(
      string employerAccountId,
      int accountId,
      string accountName,
      string publicHashedId,
      CancellationToken cancellationToken)
    {
        Permission permission = new() { Operations = new List<Operation>(), ProviderName = "provider name", Ukprn = 12345678 };
        permission.Operations.Add(Operation.CreateCohort);

        List<Permission> permissions = new List<Permission> { permission };

        var firstLegalEntity = new AccountLegalEntity
        {
            AccountId = accountId,
            Id = 1,
            Name = accountName,
            PublicHashedId = publicHashedId,
            Permissions = permissions
        };
        List<AccountLegalEntity> accountLegalEntities = new List<AccountLegalEntity>
        {
           firstLegalEntity
        };

        var outerApiClientMock = new Mock<IOuterApiClient>();
        outerApiClientMock.Setup(o => o.GetAccountLegalEntities(It.IsAny<long>(), cancellationToken)).ReturnsAsync(new GetEmployerRelationshipsQueryResponse(accountLegalEntities));



        var sessionServiceMock = new Mock<ISessionService>();
        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel { EmployerAccountId = employerAccountId, AccountLegalEntities = accountLegalEntities });

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        SelectLegalEntityController sut = new(outerApiClientMock.Object, sessionServiceMock.Object, Mock.Of<IValidator<SelectLegalEntitiesSubmitViewModel>>(), Mock.Of<IEncodingService>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        var result = await sut.Index(employerAccountId, cancellationToken);


        RedirectToRouteResult? redirectToRouteResult = result.As<RedirectToRouteResult>();
        redirectToRouteResult.RouteName.Should().Be(RouteNames.SelectTrainingProvider);
        sessionServiceMock.Verify(x => x.Set(It.Is<AddTrainingProvidersSessionModel>(
            s => s.SelectedLegalEntityId == firstLegalEntity.Id &&
                 s.SelectedLegalName == firstLegalEntity.Name
        )), Times.AtLeastOnce);
    }

    [Test, MoqInlineAutoData]
    public async Task SetsUpSessionModelWithAccountLegalEntities(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        GetEmployerRelationshipsQueryResponse employerRelationshipsQueryResponse,
        string employerAccountId,
        CancellationToken cancellationToken
    )
    {
        outerApiClientMock.Setup(o => o.GetAccountLegalEntities(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employerRelationshipsQueryResponse);
        sessionServiceMock.Setup(s => s.Get<AddTrainingProvidersSessionModel>()).Returns((AddTrainingProvidersSessionModel)null!);

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        SelectLegalEntityController sut = new(outerApiClientMock.Object, sessionServiceMock.Object, Mock.Of<IValidator<SelectLegalEntitiesSubmitViewModel>>(), Mock.Of<IEncodingService>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        var result = await sut.Index(employerAccountId, cancellationToken);
        sessionServiceMock.Verify(s => s.Set(
            It.Is<AddTrainingProvidersSessionModel>(m => m.EmployerAccountId == employerAccountId
                    && m.AccountLegalEntities.Count == employerRelationshipsQueryResponse.AccountLegalEntities.Count)), Times.Once);
    }
}