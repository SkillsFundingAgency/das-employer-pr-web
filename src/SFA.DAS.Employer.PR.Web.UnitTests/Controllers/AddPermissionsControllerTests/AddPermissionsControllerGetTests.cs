using AutoFixture.NUnit3;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Web.Authentication;
using SFA.DAS.Employer.PR.Web.Controllers;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Infrastructure.Services;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Employer.PR.Web.Models.Session;
using SFA.DAS.Employer.PR.Web.UnitTests.TestHelpers;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Controllers.AddPermissionsControllerTests;
public class AddPermissionsControllerGetTests
{
    static readonly string YourTrainingProvidersLink = Guid.NewGuid().ToString();
    static readonly string BackLink = Guid.NewGuid().ToString();

    [Test, MoqAutoData]
    public void NoSessionModel_RedirectsToYourTrainingProviders(
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        string employerAccountId)
    {
        var sessionServiceMock = new Mock<ISessionService>();
        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns((AddTrainingProvidersSessionModel)null!);

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        AddPermissionsController sut = new(outerApiMock.Object, sessionServiceMock.Object, Mock.Of<IValidator<AddPermissionsSubmitViewViewModel>>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        var result = sut.Index(employerAccountId);

        RedirectToRouteResult? redirectToRouteResult = result.As<RedirectToRouteResult>();
        redirectToRouteResult.RouteName.Should().Be(RouteNames.YourTrainingProviders);
    }

    [Test, MoqAutoData]
    public void SessionModel_BuildsViewModel(
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        long legalEntityId,
        string legalName,
        string providerName,
        long ukprn,
        string employerAccountId)
    {
        var sessionServiceMock = new Mock<ISessionService>();
        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel { EmployerAccountId = employerAccountId, SelectedLegalEntityId = legalEntityId, SelectedLegalName = legalName, ProviderName = providerName, Ukprn = ukprn });

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        AddPermissionsController sut = new(outerApiMock.Object, sessionServiceMock.Object, Mock.Of<IValidator<AddPermissionsSubmitViewViewModel>>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);

        var result = sut.Index(employerAccountId);

        ViewResult? viewResult = result.As<ViewResult>();
        AddPermissionsViewModel? viewModel = viewResult.Model as AddPermissionsViewModel;
        viewModel!.Ukprn.Should().Be(ukprn);
        viewModel.LegalEntityId.Should().Be(legalEntityId);
        viewModel.ProviderName.Should().Be(providerName);
        viewModel.LegalName.Should().Be(legalName);
    }

    [Test, MoqAutoData]
    public void SessionModel_CancelLinkAsExpected(
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        long legalEntityId,
        string legalName,
        string providerName,
        long ukprn,
        string employerAccountId)
    {
        var sessionServiceMock = new Mock<ISessionService>();
        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel { EmployerAccountId = employerAccountId, SelectedLegalEntityId = legalEntityId, SelectedLegalName = legalName, ProviderName = providerName, Ukprn = ukprn });

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        AddPermissionsController sut = new(outerApiMock.Object, sessionServiceMock.Object, Mock.Of<IValidator<AddPermissionsSubmitViewViewModel>>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);

        var result = sut.Index(employerAccountId);

        ViewResult? viewResult = result.As<ViewResult>();
        AddPermissionsViewModel? viewModel = viewResult.Model as AddPermissionsViewModel;
        viewModel!.CancelLink.Should().Be(YourTrainingProvidersLink);
    }

    [Test, MoqAutoData]
    public void SessionModel_BackLinkAsExpected(
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        long legalEntityId,
        string legalName,
        string providerName,
        long ukprn,
        string employerAccountId)
    {
        var sessionServiceMock = new Mock<ISessionService>();
        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel { EmployerAccountId = employerAccountId, SelectedLegalEntityId = legalEntityId, SelectedLegalName = legalName, ProviderName = providerName, Ukprn = ukprn });

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        AddPermissionsController sut = new(outerApiMock.Object, sessionServiceMock.Object, Mock.Of<IValidator<AddPermissionsSubmitViewViewModel>>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.SelectTrainingProvider, BackLink);

        var result = sut.Index(employerAccountId);

        ViewResult? viewResult = result.As<ViewResult>();
        AddPermissionsViewModel? viewModel = viewResult.Model as AddPermissionsViewModel;
        viewModel!.BackLink.Should().Be(BackLink);
    }
}
