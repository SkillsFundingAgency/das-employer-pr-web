using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
    public void Get_NoSessionModel_RedirectsToYourTrainingProviders(
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] AddPermissionsController sut,
        string employerAccountId)
    {
        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns((AddTrainingProvidersSessionModel)null!);
        sessionServiceMock.Setup(s => s.Contains(SessionKeys.AccountTasksKey)).Returns(false);

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

        var result = sut.Index(employerAccountId);

        RedirectToRouteResult? redirectToRouteResult = result.As<RedirectToRouteResult>();
        redirectToRouteResult.RouteName.Should().Be(RouteNames.YourTrainingProviders);
    }

    [Test, MoqAutoData]
    public void Get_SessionModel_BuildsViewModel(
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] AddPermissionsController sut,
        long legalEntityId,
        string legalName,
        string providerName,
        long ukprn,
        string employerAccountId)
    {
        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel { EmployerAccountId = employerAccountId, SelectedLegalEntityId = legalEntityId, SelectedLegalName = legalName, ProviderName = providerName, Ukprn = ukprn });
        sessionServiceMock.Setup(s => s.Contains(SessionKeys.AccountTasksKey)).Returns(false);

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

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
    public void Get_SessionHasNoAccountTasks_SetsCancelLinkToYourTrainingProvider(
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] AddPermissionsController sut,
        long legalEntityId,
        string legalName,
        string providerName,
        long ukprn,
        string employerAccountId)
    {
        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel { EmployerAccountId = employerAccountId, SelectedLegalEntityId = legalEntityId, SelectedLegalName = legalName, ProviderName = providerName, Ukprn = ukprn });
        sessionServiceMock.Setup(s => s.Contains(SessionKeys.AccountTasksKey)).Returns(false);

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);

        var result = sut.Index(employerAccountId);

        ViewResult? viewResult = result.As<ViewResult>();
        AddPermissionsViewModel? viewModel = viewResult.Model as AddPermissionsViewModel;
        viewModel!.CancelLink.Should().Be(YourTrainingProvidersLink);
    }

    [Test, MoqAutoData]
    public void Get_SessionHasAccountTasks_SetsCancelLinkToAccountTaskList(
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Frozen] Mock<IConfiguration> configurationMock,
        [Greedy] AddPermissionsController sut,
        long legalEntityId,
        string legalName,
        string providerName,
        long ukprn,
        string employerAccountId,
        string employerWebUrl)
    {
        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel { EmployerAccountId = employerAccountId, SelectedLegalEntityId = legalEntityId, SelectedLegalName = legalName, ProviderName = providerName, Ukprn = ukprn });
        sessionServiceMock.Setup(s => s.Contains(SessionKeys.AccountTasksKey)).Returns(true);

        configurationMock.Setup(c => c["EnvironmentName"]).Returns("LOCAL");
        configurationMock.Setup(c => c["EmployerAccountWebLocalUrl"]).Returns(employerWebUrl);

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);

        var result = sut.Index(employerAccountId);

        ViewResult? viewResult = result.As<ViewResult>();
        AddPermissionsViewModel? viewModel = viewResult.Model as AddPermissionsViewModel;
        viewModel!.CancelLink.Should().Contain("tasklist");
    }
}
