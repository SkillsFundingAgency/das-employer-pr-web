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
    public async Task Get_AccountWithMultipleLegalEntities_ReturnsView(
        string employerAccountId,
        List<AccountLegalEntity> accountLegalEntities,
        CancellationToken cancellationToken)
    {
        ProviderPermission permission = new() { Operations = new(), ProviderName = "provider name", Ukprn = 12345678 };
        permission.Operations.Add(Operation.CreateCohort);

        var sessionServiceMock = new Mock<ISessionService>();
        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel { EmployerAccountId = employerAccountId, AccountLegalEntities = accountLegalEntities });

        var outerApiClientMock = new Mock<IOuterApiClient>();
        outerApiClientMock.Setup(o => o.GetAccountLegalEntities(It.IsAny<long>(), cancellationToken)).ReturnsAsync(new GetAccountLegalEntitiesResponse(accountLegalEntities));

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        SelectLegalEntityController sut = new(outerApiClientMock.Object, sessionServiceMock.Object, Mock.Of<IValidator<SelectLegalEntitiesSubmitViewModel>>(), Mock.Of<IEncodingService>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        Mock<ITempDataDictionary> tempDataMock = new();
        sut.TempData = tempDataMock.Object;
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);

        /// Action
        var result = await sut.Index(employerAccountId, cancellationToken);

        ViewResult? viewResult = result.As<ViewResult>();
        var viewModel = viewResult.Model.As<SelectLegalEntitiesViewModel>();

        viewModel.BackLink.Should().Be(YourTrainingProvidersLink);
        viewModel.LegalEntities.Should().HaveCount(accountLegalEntities.Count);
    }

    [Test, MoqAutoData]
    public async Task Get_AccountWithSingleLegalEntity_RedirectsToSelectProvider(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IEncodingService> encodingServiceMock,
        [Greedy] SelectLegalEntityController sut,
        AccountLegalEntity accountLegalEntity,
        long accountLegalEntityId,
        string employerAccountId,
        CancellationToken cancellationToken)
    {
        encodingServiceMock.Setup(e => e.Decode(accountLegalEntity.AccountLegalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId)).Returns(accountLegalEntityId);

        List<AccountLegalEntity> accountLegalEntities = [accountLegalEntity];
        outerApiClientMock.Setup(o => o.GetAccountLegalEntities(It.IsAny<long>(), cancellationToken)).ReturnsAsync(new GetAccountLegalEntitiesResponse(accountLegalEntities));

        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel { EmployerAccountId = employerAccountId, AccountLegalEntities = accountLegalEntities });

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);

        /// Action
        var result = await sut.Index(employerAccountId, cancellationToken);

        RedirectToRouteResult? redirectToRouteResult = result.As<RedirectToRouteResult>();
        redirectToRouteResult.RouteName.Should().Be(RouteNames.SelectTrainingProvider);
        sessionServiceMock.Verify(x => x.Set(It.Is<AddTrainingProvidersSessionModel>(
            s => s.SelectedLegalEntityId == accountLegalEntityId &&
                 s.SelectedLegalName == accountLegalEntity.AccountLegalEntityName
        )), Times.AtLeastOnce);
    }

    [Test, MoqInlineAutoData]
    public async Task Get_SessionModelNotSet_SetsUpSessionModelWithAccountLegalEntities(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IEncodingService> encodingServiceMock,
        [Greedy] SelectLegalEntityController sut,
        GetAccountLegalEntitiesResponse employerAccountLegalEntitiesResponse,
        string employerAccountId,
        CancellationToken cancellationToken
    )
    {
        outerApiClientMock.Setup(o => o.GetAccountLegalEntities(It.IsAny<long>(), It.IsAny<CancellationToken>())).ReturnsAsync(employerAccountLegalEntitiesResponse);
        sessionServiceMock.Setup(s => s.Get<AddTrainingProvidersSessionModel>()).Returns(() => null);

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);

        /// Action
        await sut.Index(employerAccountId, cancellationToken);

        sessionServiceMock.Verify(s => s.Set(
            It.Is<AddTrainingProvidersSessionModel>(m => m.EmployerAccountId == employerAccountId
                    && m.AccountLegalEntities.Count == employerAccountLegalEntitiesResponse.LegalEntities.Count)), Times.Once);

        outerApiClientMock.Verify(o => o.GetAccountLegalEntities(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Once);

        encodingServiceMock.Verify(e => e.Decode(employerAccountId, EncodingType.AccountId), Times.Once);
    }

    [Test, MoqInlineAutoData]
    public async Task Get_SessionModelSet_GetsLegalEntitiesFromSessionModel(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IEncodingService> encodingServiceMock,
        [Greedy] SelectLegalEntityController sut,
        AddTrainingProvidersSessionModel sessionModel,
        string employerAccountId,
        CancellationToken cancellationToken
    )
    {
        sessionServiceMock.Setup(s => s.Get<AddTrainingProvidersSessionModel>()).Returns(sessionModel);

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);

        /// Action
        await sut.Index(employerAccountId, cancellationToken);

        sessionServiceMock.Verify(s => s.Set(It.IsAny<AddTrainingProvidersSessionModel>()), Times.Never);
        outerApiClientMock.Verify(o => o.GetAccountLegalEntities(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.Never);
        encodingServiceMock.Verify(e => e.Decode(employerAccountId, EncodingType.AccountId), Times.Never);
    }
}
