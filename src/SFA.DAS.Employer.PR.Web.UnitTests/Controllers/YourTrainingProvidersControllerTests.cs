using AutoFixture.NUnit3;
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
using Operation = SFA.DAS.Employer.PR.Domain.Models.Operation;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Controllers;
public class YourTrainingProvidersControllerTests
{
    static readonly string SelectLegalEntityUrl = Guid.NewGuid().ToString();
    static readonly string YourTrainingProviderUrl = Guid.NewGuid().ToString();

    [Test, MoqAutoData]
    public void Index_CallsOuterApiEndpoint(
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        GetEmployerRelationshipsQueryResponse response,
        string employerAccountId
     )
    {
        outerApiMock.Setup(o => o.GetAccountLegalEntities(employerAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        YourTrainingProvidersController sut = new(outerApiMock.Object, Mock.Of<ISessionService>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        var result = sut.Index(employerAccountId, new CancellationToken());
        outerApiMock.Verify(o => o.GetAccountLegalEntities(employerAccountId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    [MoqInlineAutoData(true)]
    [MoqInlineAutoData(false)]
    public void ReturnsExpectedLegalEntityModel(
        bool isOwner,
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        string employerAccountId,
        int accountId,
        string accountName,
        string publicHashedId
    )
    {
        var roleToTest = !isOwner ? EmployerUserRole.Viewer : EmployerUserRole.Owner;

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, roleToTest);
        YourTrainingProvidersController sut = new(outerApiMock.Object, Mock.Of<ISessionService>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        Permission permission = new() { Operations = new List<Operation>(), ProviderName = "provider name", Ukprn = 12345678 };
        permission.Operations.Add(Operation.CreateCohort);

        List<Permission> permissions = new List<Permission> { permission };
        SetupControllerAndClasses(outerApiMock, employerAccountId, accountId, accountName, publicHashedId, permissions, sut, false);
        Task<IActionResult> result = sut.Index(employerAccountId, new CancellationToken());

        ViewResult? viewResult = result.Result.As<ViewResult>();
        YourTrainingProvidersViewModel? viewModel = viewResult.Model as YourTrainingProvidersViewModel;

        viewModel!.IsOwner.Should().Be(isOwner);
        viewModel.LegalEntities.Count.Should().Be(1);
        LegalEntityModel actualLegalEntity = viewModel.LegalEntities.First();
        actualLegalEntity.AccountId.Should().Be(accountId);
        actualLegalEntity.Name.Should().Be(accountName);
        actualLegalEntity.LegalEntityPublicHashedId.Should().Be(publicHashedId);
    }

    [Test]
    [MoqInlineAutoData(Operation.CreateCohort, Operation.Recruitment, PermissionModel.PermissionToAddRecordsText, PermissionModel.PermissionToRecruitText)]
    [MoqInlineAutoData(Operation.CreateCohort, Operation.RecruitmentRequiresReview, PermissionModel.PermissionToAddRecordsText, PermissionModel.PermissionToRecruitReviewAdvertsText)]
    [MoqInlineAutoData(Operation.CreateCohort, null, PermissionModel.PermissionToAddRecordsText, PermissionModel.NoPermissionToRecruitText)]
    [MoqInlineAutoData(Operation.Recruitment, null, PermissionModel.NoPermissionToAddRecordsText, PermissionModel.PermissionToRecruitText)]
    [MoqInlineAutoData(Operation.RecruitmentRequiresReview, null, PermissionModel.NoPermissionToAddRecordsText, PermissionModel.PermissionToRecruitReviewAdvertsText)]
    [MoqInlineAutoData(null, null, PermissionModel.NoPermissionToAddRecordsText, PermissionModel.NoPermissionToRecruitText)]
    public void ReturnsExpectedPermissionTexts(
        Operation? operation1,
        Operation? operation2,
        string expectedPermissionToAddApprenticesText,
        string expectedPermissionToRecruitApprenticesText,
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        string employerAccountId,
        string providerName,
        long ukprn
    )
    {
        var accountId = 1123;
        var accountName = "account name";
        var publicHashedId = "12123232";

        var permission = new Permission { Operations = new List<Operation>(), ProviderName = providerName, Ukprn = ukprn };
        if (operation1 != null) permission.Operations.Add(operation1.Value);
        if (operation2 != null) permission.Operations.Add(operation2.Value);

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        YourTrainingProvidersController sut = new(outerApiMock.Object, Mock.Of<ISessionService>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };
        var permissions = new List<Permission> { permission };
        SetupControllerAndClasses(outerApiMock, employerAccountId, accountId, accountName, publicHashedId, permissions, sut, false);

        var result = sut.Index(employerAccountId, new CancellationToken());

        var viewResult = result.Result.As<ViewResult>();
        var viewModel = viewResult.Model as YourTrainingProvidersViewModel;

        var actualLegalEntity = viewModel!.LegalEntities.First();

        var actualPermissionDetails = actualLegalEntity.Permissions.First();

        actualPermissionDetails.Ukprn.Should().Be(ukprn);
        actualPermissionDetails.ProviderName.Should().Be(providerName);
        actualPermissionDetails.PermissionToAddRecords.Should().Be(expectedPermissionToAddApprenticesText);
        actualPermissionDetails.PermissionToRecruitApprentices.Should().Be(expectedPermissionToRecruitApprenticesText);
        actualPermissionDetails.ChangePermissionsLink.Should().Be("#");
    }

    [Test, MoqInlineAutoData]
    public void CallsSessionServiceDelete(
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        string employerAccountId,
        string providerName,
        long ukprn
    )
    {
        var accountId = 1123;
        var accountName = "account name";
        var publicHashedId = "12123232";

        var permission = new Permission { Operations = new List<Operation>(), ProviderName = providerName, Ukprn = ukprn };
        permission.Operations.Add(Operation.CreateCohort);
        permission.Operations.Add(Operation.Recruitment);

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        YourTrainingProvidersController sut = new(outerApiMock.Object, sessionServiceMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };
        var permissions = new List<Permission> { permission };
        SetupControllerAndClasses(outerApiMock, employerAccountId, accountId, accountName, publicHashedId, permissions, sut, false);

        var result = sut.Index(employerAccountId, new CancellationToken());
        sessionServiceMock.Verify(s => s.Delete<AddTrainingProvidersSessionModel>(), Times.Once);
    }

    [Test, MoqInlineAutoData]
    public void SetsYourTrainingProviderUrl(
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        string employerAccountId,
        string providerName,
        long ukprn
  )
    {
        var accountId = 1123;
        var accountName = "account name";
        var publicHashedId = "12123232";

        var permission = new Permission { Operations = new List<Operation>(), ProviderName = providerName, Ukprn = ukprn };
        permission.Operations.Add(Operation.CreateCohort);
        permission.Operations.Add(Operation.Recruitment);


        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        YourTrainingProvidersController sut = new(outerApiMock.Object, Mock.Of<ISessionService>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };
        var permissions = new List<Permission> { permission };
        SetupControllerAndClasses(outerApiMock, employerAccountId, accountId, accountName, publicHashedId, permissions, sut, true);

        var result = sut.Index(employerAccountId, new CancellationToken());

        var viewResult = result.Result.As<ViewResult>();
        var viewModel = viewResult.Model as YourTrainingProvidersViewModel;

        viewModel!.AddTrainingProviderUrl.Should().Be(SelectLegalEntityUrl);
    }

    [Test, MoqInlineAutoData]
    public void SetsLegalProvidersEntitiesUrl(
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        string employerAccountId,
        string providerName,
        long ukprn
    )
    {
        var accountId = 1123;
        var accountName = "account name";
        var publicHashedId = "12123232";

        var permission = new Permission { Operations = new List<Operation>(), ProviderName = providerName, Ukprn = ukprn };
        permission.Operations.Add(Operation.CreateCohort);
        permission.Operations.Add(Operation.Recruitment);


        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        YourTrainingProvidersController sut = new(outerApiMock.Object, Mock.Of<ISessionService>())
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };
        var permissions = new List<Permission> { permission };
        SetupControllerAndClasses(outerApiMock, employerAccountId, accountId, accountName, publicHashedId, permissions, sut, true);

        var result = sut.Index(employerAccountId, new CancellationToken());

        var viewResult = result.Result.As<ViewResult>();
        var viewModel = viewResult.Model as YourTrainingProvidersViewModel;

        viewModel!.AddTrainingProviderUrl.Should().Be(SelectLegalEntityUrl);
    }

    private static void SetupControllerAndClasses(Mock<IOuterApiClient> outerApiMock, string employerAccountId, int accountId, string accountName,
        string publicHashedId, List<Permission> permissions, YourTrainingProvidersController sut, bool multipleAccounts)
    {
        List<AccountLegalEntity> accountLegalEntities = new List<AccountLegalEntity>
        {
            new()
            {
                AccountId = accountId, Id = 1, Name = accountName, PublicHashedId = publicHashedId,
                Permissions = permissions
            }
        };

        if (multipleAccounts)
        {
            accountLegalEntities.Add(new AccountLegalEntity
            { AccountId = 1234, Id = 2, Name = "name 2", PublicHashedId = "AFC", Permissions = permissions });
        }

        GetEmployerRelationshipsQueryResponse response = new GetEmployerRelationshipsQueryResponse(accountLegalEntities);

        outerApiMock.Setup(o => o.GetAccountLegalEntities(employerAccountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        if (multipleAccounts)
        {
            sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.SelectLegalEntity, SelectLegalEntityUrl);
        }
        else
        {
            sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.SelectTrainingProvider, YourTrainingProviderUrl);
        }
    }
}
