using AutoFixture.NUnit3;
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

namespace SFA.DAS.Employer.PR.Web.UnitTests.Controllers.SelectLegalEntityControllerTests;
public class SelectLegalEntityControllerGetTests
{
    static readonly string YourTrainingProvidersLink = Guid.NewGuid().ToString();

    [Test, MoqAutoData]
    public void ReturnsExpectedLegalEntityModel_MatchingMultipleLegalEntities(
        string employerAccountId,
        int accountId,
        string accountName,
        string publicHashedId)
    {
        Permission permission = new() { Operations = new List<Operation>(), ProviderName = "provider name", Ukprn = 12345678 };
        permission.Operations.Add(Operation.CreateCohort);

        List<Permission> permissions = new List<Permission> { permission };
        List<AccountLegalEntity> accountLegalEntities = new List<AccountLegalEntity>
        {
            new() {AccountId = accountId,Id=1 , Name=accountName, PublicHashedId = publicHashedId, Permissions = permissions},
            new() {AccountId = 1123,Id=1, Name="account name", PublicHashedId = "12123232", Permissions = permissions}
        };
        var sessionServiceMock = new Mock<ISessionService>();
        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel { AccountLegalEntities = accountLegalEntities });

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        SelectLegalEntityController sut = new(sessionServiceMock.Object, Mock.Of<IValidator<SelectLegalEntitiesSubmitViewModel>>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        var result = sut.Index(employerAccountId);

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
    public void ReturnsExpectedLegalEntityModel_MatchingSingleLegalEntity(
      string employerAccountId,
      int accountId,
      string accountName,
      string publicHashedId)
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
        var sessionServiceMock = new Mock<ISessionService>();
        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns(new AddTrainingProvidersSessionModel { AccountLegalEntities = accountLegalEntities });

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        SelectLegalEntityController sut = new(sessionServiceMock.Object, Mock.Of<IValidator<SelectLegalEntitiesSubmitViewModel>>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        var result = sut.Index(employerAccountId);

        RedirectToActionResult? redirectToActionResult = result.As<RedirectToActionResult>();

        redirectToActionResult.ActionName.Should().Be("Index");
        redirectToActionResult.ControllerName.Should().Be("SelectTrainingProvider");
        sessionServiceMock.Verify(x => x.Set(It.Is<AddTrainingProvidersSessionModel>(
            s => s.LegalEntityId == firstLegalEntity.Id &&
                 s.LegalName == firstLegalEntity.Name
        )), Times.AtLeastOnce);
    }

    [Test, MoqAutoData]
    public void RedirectsToYourTrainingProvidersIfSessionModelNotSetUp(
      string employerAccountId,
      int accountId,
      string accountName,
      string publicHashedId)
    {
        Permission permission = new() { Operations = new List<Operation>(), ProviderName = "provider name", Ukprn = 12345678 };
        permission.Operations.Add(Operation.CreateCohort);

        List<Permission> permissions = new List<Permission> { permission };
        List<AccountLegalEntity> accountLegalEntities = new List<AccountLegalEntity>
        {
            new() {AccountId = accountId,Id=1 , Name=accountName, PublicHashedId = publicHashedId, Permissions = permissions},
            new() {AccountId = 1123,Id=1, Name="account name", PublicHashedId = "12123232", Permissions = permissions}
        };
        var sessionServiceMock = new Mock<ISessionService>();
        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>())
            .Returns((AddTrainingProvidersSessionModel)null!);

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        SelectLegalEntityController sut = new(sessionServiceMock.Object, Mock.Of<IValidator<SelectLegalEntitiesSubmitViewModel>>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        var result = sut.Index(employerAccountId);

        RedirectToActionResult? redirectToActionResult = result.As<RedirectToActionResult>();

        redirectToActionResult.ActionName.Should().Be("Index");
        redirectToActionResult.ControllerName.Should().Be("YourTrainingProviders");
    }

    [Test, MoqInlineAutoData]
    public void CallsSessionServiceGet(
        [Frozen] Mock<ISessionService> sessionServiceMock,
        string employerAccountId,
        string providerName,
        long ukprn
    )
    {
        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        SelectLegalEntityController sut = new(sessionServiceMock.Object, Mock.Of<IValidator<SelectLegalEntitiesSubmitViewModel>>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.YourTrainingProviders, YourTrainingProvidersLink);
        var result = sut.Index(employerAccountId);
        sessionServiceMock.Verify(s => s.Get<AddTrainingProvidersSessionModel>(), Times.Once);
    }
}
