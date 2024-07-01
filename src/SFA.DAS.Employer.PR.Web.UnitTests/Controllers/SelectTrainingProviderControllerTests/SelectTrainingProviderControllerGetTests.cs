using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Web.Authentication;
using SFA.DAS.Employer.PR.Web.Controllers;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Infrastructure.Services;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Employer.PR.Web.Models.Session;
using SFA.DAS.Employer.PR.Web.UnitTests.TestHelpers;
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
            new(sessionServiceMock.Object,
                Mock.Of<IValidator<SelectTrainingProviderSubmitViewModel>>())
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
            };
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, BackLink);

        var result = sut.Index(employerAccountId);

        RedirectToActionResult? redirectToActionResult = result.As<RedirectToActionResult>();

        redirectToActionResult.ActionName.Should().Be("Index");
        redirectToActionResult.ControllerName.Should().Be("YourTrainingProviders");
    }

    [Test, MoqAutoData]
    public void SelectTrainingProviderGet_SingleLegalEntity_SetsBackLinkToExpected(
        string employerAccountId,
        long legalEntityId,
        string legalName
     )
    {
        Mock<ISessionService> sessionServiceMock = new Mock<ISessionService>();
        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel
            {
                LegalEntityId = legalEntityId,
                LegalName = legalName,
                AccountLegalEntities = new List<AccountLegalEntity> { new AccountLegalEntity() }
            });
        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        SelectTrainingProviderController sut = new(sessionServiceMock.Object, Mock.Of<IValidator<SelectTrainingProviderSubmitViewModel>>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, BackLink);

        var result = sut.Index(employerAccountId);
        ViewResult? viewResult = result.As<ViewResult>();
        SelectTrainingProviderViewModel? viewModel = viewResult.Model as SelectTrainingProviderViewModel;

        viewModel!.BackLink.Should().Be(BackLink);
    }

    [Test, MoqAutoData]
    public void SelectTrainingProviderGet_MultipleLegalEntity_SetsBackLinkToExpected(
        string employerAccountId,
        long legalEntityId,
        string legalName
    )
    {
        Mock<ISessionService> sessionServiceMock = new Mock<ISessionService>();
        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel
            {
                LegalEntityId = legalEntityId,
                LegalName = legalName,
                AccountLegalEntities = new List<AccountLegalEntity> { new AccountLegalEntity(), new AccountLegalEntity() }
            });
        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        SelectTrainingProviderController sut = new(sessionServiceMock.Object, Mock.Of<IValidator<SelectTrainingProviderSubmitViewModel>>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.SelectLegalEntity, BackLink);

        var result = sut.Index(employerAccountId);
        ViewResult? viewResult = result.As<ViewResult>();
        SelectTrainingProviderViewModel? viewModel = viewResult.Model as SelectTrainingProviderViewModel;

        viewModel!.BackLink.Should().Be(BackLink);
    }
}