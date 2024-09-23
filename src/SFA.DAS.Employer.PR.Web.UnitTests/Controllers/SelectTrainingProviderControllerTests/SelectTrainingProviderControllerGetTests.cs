using AutoFixture.NUnit3;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Web.Authentication;
using SFA.DAS.Employer.PR.Web.Controllers;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Infrastructure.Services;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Employer.PR.Web.UnitTests.TestHelpers;
using SFA.DAS.Encoding;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Controllers.SelectTrainingProviderControllerTests;

public class SelectTrainingProviderControllerGetTests
{
    [Test, AutoData]
    public void SelectTrainingProviderGet_SessionModelNotSet_RedirectToYourTrainingProviders(
        string employerAccountId)
    {
        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        SelectTrainingProviderController sut =
            new(Mock.Of<IOuterApiClient>(), Mock.Of<ISessionService>(), Mock.Of<IEncodingService>(),
                Mock.Of<IValidator<SelectTrainingProviderViewModel>>())
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
            };

        var result = sut.Index(employerAccountId);

        result.As<RedirectToRouteResult>().RouteName.Should().Be(RouteNames.YourTrainingProviders);
    }
}
