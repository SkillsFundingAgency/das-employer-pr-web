using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Interfaces;
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

namespace SFA.DAS.Employer.PR.Web.UnitTests.Controllers.SelectTrainingProviderControllerTests;

public class SelectTrainingProviderControllerGetTests
{
    static readonly string BackLink = Guid.NewGuid().ToString();

    [Test, MoqAutoData]
    public void SelectTrainingProviderGet_SessionModelNotSet_RedirectToYourTrainingProviders(
        string employerAccountId
    )
    {
        var sessionServiceMock = new Mock<ISessionService>();

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        SelectTrainingProviderController sut =
            new(Mock.Of<IOuterApiClient>(), sessionServiceMock.Object, Mock.Of<IEncodingService>(),
                Mock.Of<IValidator<SelectTrainingProviderSubmitModel>>())
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
            };
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, BackLink);

        var result = sut.Index(employerAccountId);

        RedirectToRouteResult? redirectToRouteResult = result.As<RedirectToRouteResult>();
        redirectToRouteResult.RouteName.Should().Be(RouteNames.YourTrainingProviders);
    }

    [Test, MoqAutoData]
    public void SelectTrainingProviderGet_SingleLegalEntity_SetsBackLinkToExpected(
        AccountLegalEntity accountLegalEntity,
        string employerAccountId,
        long legalEntityId,
        string legalName
     )
    {
        Mock<ISessionService> sessionServiceMock = new Mock<ISessionService>();

        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel
            {
                EmployerAccountId = employerAccountId,
                SelectedLegalEntityId = legalEntityId,
                SelectedLegalName = legalName,
                AccountLegalEntities = [accountLegalEntity]
            });
        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        SelectTrainingProviderController sut = new(Mock.Of<IOuterApiClient>(), sessionServiceMock.Object,
            Mock.Of<IEncodingService>(), Mock.Of<IValidator<SelectTrainingProviderSubmitModel>>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, BackLink);

        var result = sut.Index(employerAccountId);
        ViewResult? viewResult = result.As<ViewResult>();
        SelectTrainingProviderModel? viewModel = viewResult.Model as SelectTrainingProviderModel;

        viewModel!.BackLink.Should().Be(BackLink);
    }

    [Test, MoqAutoData]
    public void SelectTrainingProviderGet_MultipleLegalEntity_SetsBackLinkToExpected(
        List<AccountLegalEntity> accountLegalEntities,
        string employerAccountId,
        long legalEntityId,
        string legalName
    )
    {
        Mock<ISessionService> sessionServiceMock = new Mock<ISessionService>();

        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel
            {
                EmployerAccountId = employerAccountId,
                SelectedLegalEntityId = legalEntityId,
                SelectedLegalName = legalName,
                AccountLegalEntities = accountLegalEntities
            });
        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        SelectTrainingProviderController sut = new(Mock.Of<IOuterApiClient>(), sessionServiceMock.Object, Mock.Of<IEncodingService>(), Mock.Of<IValidator<SelectTrainingProviderSubmitModel>>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.SelectLegalEntity, BackLink);

        var result = sut.Index(employerAccountId);
        ViewResult? viewResult = result.As<ViewResult>();
        SelectTrainingProviderModel? viewModel = viewResult.Model as SelectTrainingProviderModel;

        viewModel!.BackLink.Should().Be(BackLink);
    }
}