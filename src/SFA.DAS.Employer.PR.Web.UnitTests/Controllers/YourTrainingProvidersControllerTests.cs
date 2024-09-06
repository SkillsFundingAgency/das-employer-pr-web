using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Authentication;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Controllers;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Infrastructure.Services;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Employer.PR.Web.Models.Session;
using SFA.DAS.Employer.PR.Web.UnitTests.TestHelpers;
using SFA.DAS.Encoding;
using SFA.DAS.Testing.AutoFixture;
using Operation = SFA.DAS.Employer.PR.Domain.Models.Operation;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Controllers;
public class YourTrainingProvidersControllerTests
{
    static readonly string SelectLegalEntityUrl = Guid.NewGuid().ToString();
    static readonly string YourTrainingProviderUrl = Guid.NewGuid().ToString();
    static readonly string ChangePermissionsLink = Guid.NewGuid().ToString();

    [Test, MoqAutoData]
    public void Index_CallsOuterApiEndpoint(
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        [Frozen] Mock<IEncodingService> encodingServiceMock,
        GetEmployerRelationshipsQueryResponse response,
        string employerAccountId,
        long accountId
     )
    {
        encodingServiceMock.Setup(e => e.Decode(employerAccountId, EncodingType.AccountId)).Returns(accountId);
        outerApiMock.Setup(o => o.GetAccountLegalEntities(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        YourTrainingProvidersController sut = new(outerApiMock.Object, Mock.Of<ISessionService>(), encodingServiceMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        var result = sut.Index(employerAccountId, new CancellationToken());
        outerApiMock.Verify(o => o.GetAccountLegalEntities(accountId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    [MoqInlineAutoData(true)]
    [MoqInlineAutoData(false)]
    public void ReturnsExpectedLegalEntityModel(
        bool isOwner,
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        [Frozen] Mock<IEncodingService> encodingServiceMock,
        string employerAccountId,
        int accountId,
        string accountName,
        string publicHashedId
    )
    {
        encodingServiceMock.Setup(e => e.Decode(employerAccountId, EncodingType.AccountId)).Returns(accountId);

        var roleToTest = !isOwner ? EmployerUserRole.Viewer : EmployerUserRole.Owner;

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, roleToTest);
        YourTrainingProvidersController sut = new(outerApiMock.Object, Mock.Of<ISessionService>(), encodingServiceMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };


        Permission permission = new() { Operations = new List<Operation>(), ProviderName = "provider name", Ukprn = 12345678 };
        permission.Operations.Add(Operation.CreateCohort);

        List<Permission> permissions = new List<Permission> { permission };
        SetupControllerAndClasses(outerApiMock, accountId, accountName, publicHashedId, permissions, sut, false);
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


    [Test, MoqAutoData]
    public void ReturnsLegalEntitiesWithLegalEntityAndProvidersInAlphabeticalOrder(
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        [Frozen] Mock<IEncodingService> encodingServiceMock,
        string employerAccountId,
        int accountId,
        string publicHashedId
    )
    {
        var providerNameExpectedFirst = "AAA provider";
        var providerNameExpectedSecond = "M1 training";
        var providerNameExpectedThird = "z testing";

        var accountNameExpectedFirst = "A1 Legal Entity";
        var accountNameExpectedSecond = "NN LE";
        var accountNameExpectedThird = "Z1 Last AL";
        var roleToTest = EmployerUserRole.Owner;

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, roleToTest);


        Permission permissionOther = new() { Operations = new List<Operation>() { Operation.CreateCohort }, ProviderName = "provider", Ukprn = 12345678 };
        permissionOther.Operations.Add(Operation.CreateCohort);

        List<Permission> permissions = new List<Permission>
        {
            new() { Operations = new List<Operation>{ Operation.CreateCohort }, ProviderName = providerNameExpectedThird, Ukprn = 12345678 },
            new() { Operations = new List<Operation>{ Operation.CreateCohort }, ProviderName = providerNameExpectedFirst, Ukprn = 12345679 },
            new() { Operations = new List<Operation>{ Operation.CreateCohort }, ProviderName = providerNameExpectedSecond, Ukprn = 12345680 }
        };


        List<AccountLegalEntity> accountLegalEntities = new List<AccountLegalEntity>
        {
            new()
            {
                AccountId = accountId, Id = 1, Name = accountNameExpectedThird, PublicHashedId = "KJGH",
                Permissions = new List<Permission> {permissionOther}
            },
            new()
            {
                AccountId = accountId, Id=2, Name = accountNameExpectedFirst, PublicHashedId = publicHashedId,
                Permissions = permissions
            },
            new()
            {
                AccountId = accountId, Id = 1, Name = accountNameExpectedSecond, PublicHashedId = "AVBC",
                Permissions = new List<Permission> {permissionOther}
            },
        };

        encodingServiceMock.Setup(e => e.Decode(employerAccountId, EncodingType.AccountId)).Returns(accountId);

        outerApiMock.Setup(o => o.GetAccountLegalEntities(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetEmployerRelationshipsQueryResponse(accountLegalEntities));

        YourTrainingProvidersController sut = new(outerApiMock.Object, Mock.Of<ISessionService>(), encodingServiceMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        Mock<ITempDataDictionary> tempDataMock = new();
        sut.TempData = tempDataMock.Object;
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.SelectLegalEntity, SelectLegalEntityUrl);

        Task<IActionResult> result = sut.Index(employerAccountId, new CancellationToken());

        ViewResult? viewResult = result.Result.As<ViewResult>();
        YourTrainingProvidersViewModel? viewModel = viewResult.Model as YourTrainingProvidersViewModel;
        viewModel!.LegalEntities.Count.Should().Be(3);
        LegalEntityModel actualLegalEntity = viewModel.LegalEntities.First();
        actualLegalEntity.Name.Should().Be(accountNameExpectedFirst);

        var firstPermission = actualLegalEntity.Permissions.First();

        firstPermission.ProviderName.Should().Be(providerNameExpectedFirst);
    }

    [Test]
    [MoqInlineAutoData(Operation.CreateCohort, Operation.Recruitment, PermissionModel.PermissionToAddRecordsText, PermissionModel.PermissionToRecruitText)]
    [MoqInlineAutoData(Operation.CreateCohort, Operation.RecruitmentRequiresReview, PermissionModel.PermissionToAddRecordsText, PermissionModel.PermissionToRecruitReviewAdvertsText)]
    [MoqInlineAutoData(Operation.CreateCohort, null, PermissionModel.PermissionToAddRecordsText, PermissionModel.NoPermissionToRecruitText)]
    [MoqInlineAutoData(Operation.Recruitment, null, PermissionModel.NoPermissionToAddRecordsText, PermissionModel.PermissionToRecruitText)]
    [MoqInlineAutoData(Operation.RecruitmentRequiresReview, null, PermissionModel.NoPermissionToAddRecordsText, PermissionModel.PermissionToRecruitReviewAdvertsText)]
    public void ReturnsExpectedPermissionTexts(
        Operation? operation1,
        Operation? operation2,
        string expectedPermissionToAddApprenticesText,
        string expectedPermissionToRecruitApprenticesText,
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        [Frozen] Mock<IEncodingService> encodingServiceMock,
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

        encodingServiceMock.Setup(e => e.Decode(employerAccountId, EncodingType.AccountId)).Returns(accountId);
        YourTrainingProvidersController sut = new(outerApiMock.Object, Mock.Of<ISessionService>(), encodingServiceMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };
        var permissions = new List<Permission> { permission };
        SetupControllerAndClasses(outerApiMock, accountId, accountName, publicHashedId, permissions, sut, false);

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.ChangePermissions, ChangePermissionsLink);

        var result = sut.Index(employerAccountId, new CancellationToken());

        var viewResult = result.Result.As<ViewResult>();
        var viewModel = viewResult.Model as YourTrainingProvidersViewModel;

        var actualLegalEntity = viewModel!.LegalEntities.First();

        var actualPermissionDetails = actualLegalEntity.Permissions.First();

        actualPermissionDetails.Ukprn.Should().Be(ukprn);
        actualPermissionDetails.ProviderName.Should().Be(providerName);
        actualPermissionDetails.PermissionToAddRecords.Should().Be(expectedPermissionToAddApprenticesText);
        actualPermissionDetails.PermissionToRecruitApprentices.Should().Be(expectedPermissionToRecruitApprenticesText);
        actualPermissionDetails.ChangePermissionsLink.Should().Be(ChangePermissionsLink);
    }

    [Test, MoqAutoData]
    public void ReturnsNoLegalEntities1(
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        [Frozen] Mock<IEncodingService> encodingServiceMock,
        [Greedy] YourTrainingProvidersController sut,
        string employerAccountId,
        string providerName,
        long ukprn
        )
    {
        var accountId = 1123;
        var accountName = "account name";
        var publicHashedId = "12123232";

        encodingServiceMock.Setup(e => e.Decode(employerAccountId, EncodingType.AccountId)).Returns(accountId);

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
        var permissions = new List<Permission>();
        SetupControllerAndClasses(outerApiMock, accountId, accountName, publicHashedId, permissions, sut, false);

        var result = sut.Index(employerAccountId, new CancellationToken());

        var viewResult = result.Result.As<ViewResult>();
        var viewModel = viewResult.Model as YourTrainingProvidersViewModel;

        viewModel!.LegalEntities.Count.Should().Be(0);
    }

    [Test, MoqInlineAutoData]
    public void CallsSessionServiceDelete(
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] YourTrainingProvidersController sut,
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
        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
        var permissions = new List<Permission> { permission };
        SetupControllerAndClasses(outerApiMock, accountId, accountName, publicHashedId, permissions, sut, false);

        var result = sut.Index(employerAccountId, new CancellationToken());
        sessionServiceMock.Verify(s => s.Delete<AddTrainingProvidersSessionModel>(), Times.Once);
    }

    [Test, MoqInlineAutoData]
    public void SetsLegalProvidersEntitiesUrl(
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        [Greedy] YourTrainingProvidersController sut,
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
        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
        var permissions = new List<Permission> { permission };
        SetupControllerAndClasses(outerApiMock, accountId, accountName, publicHashedId, permissions, sut, true);

        var result = sut.Index(employerAccountId, new CancellationToken());

        var viewResult = result.Result.As<ViewResult>();
        var viewModel = viewResult.Model as YourTrainingProvidersViewModel;

        viewModel!.AddTrainingProviderUrl.Should().Be(SelectLegalEntityUrl);
    }

    [Test, MoqInlineAutoData]
    public void TempDataSuccessfulAddition_AddsShowBannerAndProviderNameToViewModel(
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] YourTrainingProvidersController sut,
        string employerAccountId,
        string providerName,
        long ukprn
    )
    {
        var isSuccessfulAddition = true;
        var accountId = 1123;
        var accountName = "account name";
        var publicHashedId = "12123232";

        var permission = new Permission { Operations = new List<Operation>(), ProviderName = providerName, Ukprn = ukprn };
        permission.Operations.Add(Operation.CreateCohort);
        permission.Operations.Add(Operation.Recruitment);

        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>()).Returns(
            new AddTrainingProvidersSessionModel
            { SuccessfulAddition = isSuccessfulAddition, ProviderName = providerName });

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
        var permissions = new List<Permission> { permission };
        SetupControllerAndClasses(outerApiMock, accountId, accountName, publicHashedId, permissions, sut, false);

        Mock<ITempDataDictionary> tempDataMock = new();
        tempDataMock.Setup(t => t[TempDataKeys.NameOfProviderAdded]).Returns(providerName);

        sut.TempData = tempDataMock.Object;

        var result = sut.Index(employerAccountId, new CancellationToken());

        var viewResult = result.Result.As<ViewResult>();
        var viewModel = viewResult.Model as YourTrainingProvidersViewModel;
        viewModel!.ShowPermissionsUpdatedBanner().Should().BeTrue();
        viewModel.PermissionsUpdatedForProvider.Should().Be(providerName.ToUpper());
        var expectedText = $"You've added {providerName.ToUpper()} and set their permissions.";
        viewModel.PermissionsUpdatedForProviderText.Should().Be(expectedText);
    }


    [Test, MoqInlineAutoData]
    public void TempDataSuccessfulUpdate_AddsShowBannerAndProviderNameToViewModel(
       [Frozen] Mock<IOuterApiClient> outerApiMock,
       [Frozen] Mock<ISessionService> sessionServiceMock,
       [Greedy] YourTrainingProvidersController sut,
       string employerAccountId,
       string providerName,
       long ukprn
   )
    {
        var isSuccessfulAddition = true;
        var accountId = 1123;
        var accountName = "account name";
        var publicHashedId = "12123232";

        var permission = new Permission { Operations = new List<Operation>(), ProviderName = providerName, Ukprn = ukprn };
        permission.Operations.Add(Operation.CreateCohort);
        permission.Operations.Add(Operation.Recruitment);

        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>()).Returns(
            new AddTrainingProvidersSessionModel
            { SuccessfulAddition = isSuccessfulAddition, ProviderName = providerName });

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
        var permissions = new List<Permission> { permission };
        SetupControllerAndClasses(outerApiMock, accountId, accountName, publicHashedId, permissions, sut, false);

        Mock<ITempDataDictionary> tempDataMock = new();
        tempDataMock.Setup(t => t[TempDataKeys.NameOfProviderUpdated]).Returns(providerName);

        sut.TempData = tempDataMock.Object;

        var result = sut.Index(employerAccountId, new CancellationToken());

        var viewResult = result.Result.As<ViewResult>();
        var viewModel = viewResult.Model as YourTrainingProvidersViewModel;
        viewModel!.ShowPermissionsUpdatedBanner().Should().BeTrue();
        viewModel.PermissionsUpdatedForProvider.Should().Be(providerName.ToUpper());
        var expectedText = $"You've set permissions for {providerName.ToUpper()}"; ;
        viewModel.PermissionsUpdatedForProviderText.Should().Be(expectedText);
    }

    private static void SetupControllerAndClasses(Mock<IOuterApiClient> outerApiMock, int accountId, string accountName,
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

        outerApiMock.Setup(o => o.GetAccountLegalEntities(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        Mock<ITempDataDictionary> tempDataMock = new();
        sut.TempData = tempDataMock.Object;

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.SelectLegalEntity, SelectLegalEntityUrl);
    }
}
