using FluentValidation;
using FluentValidation.Results;
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
public class SelectTrainingProviderControllerPostTests
{
    static readonly string YourTrainingProvidersLink = Guid.NewGuid().ToString();

    [Test, MoqAutoData]
    public void Post_Validated_ReturnsExpectedModel_SetsSessionModel(
        Mock<IValidator<SelectTrainingProviderSubmitViewModel>> validatorMock,
        string employerAccountId,
        int ukprn,
        string name)
    {
        var sessionServiceMock = new Mock<ISessionService>();
        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel { AccountLegalEntities = new() { new AccountLegalEntity() } });

        SelectTrainingProviderSubmitViewModel submitModel = new SelectTrainingProviderSubmitViewModel
        {
            Name = name,
            Ukprn = ukprn.ToString(),
        };

        validatorMock.Setup(v => v.Validate(It.IsAny<SelectTrainingProviderSubmitViewModel>())).Returns(new ValidationResult());
        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        SelectTrainingProviderController sut = new(sessionServiceMock.Object, validatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        var result = sut.Index(employerAccountId, submitModel);

        var redirectResult = result.As<RedirectToActionResult>();
        redirectResult!.ActionName.Should().Be("Index");
        redirectResult.ControllerName.Should().Be("SelectTrainingProvider");

        sessionServiceMock.Verify(s => s.Set(It.Is<AddTrainingProvidersSessionModel>(x => x.ProviderName == name && x.Ukprn == ukprn)), Times.Once);
    }

    [Test, MoqAutoData]
    public void Post_ValidatedAndFailed_ReturnsExpectedModel(
        Mock<IValidator<SelectTrainingProviderSubmitViewModel>> validatorMock,
        string employerAccountId)
    {
        var sessionServiceMock = new Mock<ISessionService>();
        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel { AccountLegalEntities = new() { new AccountLegalEntity() } });

        SelectTrainingProviderSubmitViewModel submitModel = new SelectTrainingProviderSubmitViewModel();
        validatorMock.Setup(m => m.Validate(It.IsAny<SelectTrainingProviderSubmitViewModel>())).Returns(new ValidationResult(new List<ValidationFailure>()
        {
            new("TestField","Test Message") { ErrorCode = "1001"}
        }));

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        SelectTrainingProviderController sut = new(sessionServiceMock.Object, validatorMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        var result = sut.Index(employerAccountId, submitModel);

        ViewResult? viewResult = result.As<ViewResult>();
        SelectTrainingProviderViewModel? viewModel = viewResult.Model as SelectTrainingProviderViewModel;

        viewModel!.BackLink.Should().Be(YourTrainingProvidersLink);
        sessionServiceMock.Verify(s => s.Set(It.IsAny<AddTrainingProvidersSessionModel>()), Times.Never);
    }
}
