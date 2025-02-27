﻿using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using SFA.DAS.Employer.PR.Domain.Common;
using SFA.DAS.Employer.PR.Domain.Constants;
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
    static readonly string ChangePermissionsLink = Guid.NewGuid().ToString();
    static readonly string RequestIdLink = Guid.NewGuid().ToString();

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
        outerApiMock.Setup(o => o.GetEmployerRelationships(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        YourTrainingProvidersController sut = new(outerApiMock.Object, Mock.Of<ISessionService>(), encodingServiceMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };

        sut.Index(employerAccountId, new CancellationToken());

        outerApiMock.Verify(o => o.GetEmployerRelationships(accountId, It.IsAny<CancellationToken>()), Times.Once);
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


        ProviderPermission permission = new() { Operations = new List<Operation>(), ProviderName = "provider name", Ukprn = 12345678 };
        permission.Operations.Add(Operation.CreateCohort);

        List<ProviderPermission> permissions = new List<ProviderPermission> { permission };
        SetupControllerAndClasses(outerApiMock, accountId, accountName, publicHashedId, permissions, sut, false);
        Task<IActionResult> result = sut.Index(employerAccountId, new CancellationToken());

        ViewResult? viewResult = result.Result.As<ViewResult>();
        YourTrainingProvidersViewModel? viewModel = viewResult.Model as YourTrainingProvidersViewModel;

        viewModel!.IsOwner.Should().Be(isOwner);
        viewModel.LegalEntities.Count.Should().Be(1);
        LegalEntityModel actualLegalEntity = viewModel.LegalEntities[0];
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


        ProviderPermission permissionOther = new() { Operations = new List<Operation>() { Operation.CreateCohort }, ProviderName = "provider", Ukprn = 12345678 };
        permissionOther.Operations.Add(Operation.CreateCohort);

        List<ProviderPermission> permissions = new List<ProviderPermission>
        {
            new() { Operations = new List<Operation>{ Operation.CreateCohort }, ProviderName = providerNameExpectedThird, Ukprn = 12345678 },
            new() { Operations = new List<Operation>{ Operation.CreateCohort }, ProviderName = providerNameExpectedFirst, Ukprn = 12345679 },
            new() { Operations = new List<Operation>{ Operation.CreateCohort }, ProviderName = providerNameExpectedSecond, Ukprn = 12345680 }
        };


        List<LegalEntity> accountLegalEntities = new List<LegalEntity>
        {
            new()
            {
                AccountId = accountId, Id = 1, Name = accountNameExpectedThird, PublicHashedId = "KJGH",
                Permissions = new List<ProviderPermission> {permissionOther}
            },
            new()
            {
                AccountId = accountId, Id=2, Name = accountNameExpectedFirst, PublicHashedId = publicHashedId,
                Permissions = permissions
            },
            new()
            {
                AccountId = accountId, Id = 1, Name = accountNameExpectedSecond, PublicHashedId = "AVBC",
                Permissions = new List<ProviderPermission> {permissionOther}
            },
        };

        encodingServiceMock.Setup(e => e.Decode(employerAccountId, EncodingType.AccountId)).Returns(accountId);

        outerApiMock.Setup(o => o.GetEmployerRelationships(It.IsAny<long>(), It.IsAny<CancellationToken>()))
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
        LegalEntityModel actualLegalEntity = viewModel.LegalEntities[0];
        actualLegalEntity.Name.Should().Be(accountNameExpectedFirst);

        var firstPermission = actualLegalEntity.PermissionDetails[0];

        firstPermission.ProviderName.Should().Be(providerNameExpectedFirst);
    }

    [Test]
    [MoqInlineAutoData(Operation.CreateCohort, Operation.Recruitment, ManageRequests.YesWithEmployerRecordReview, ManageRequests.Yes)]
    [MoqInlineAutoData(Operation.CreateCohort, Operation.RecruitmentRequiresReview, ManageRequests.YesWithEmployerRecordReview, ManageRequests.YesWithEmployerAdvertReview)]
    [MoqInlineAutoData(Operation.CreateCohort, null, ManageRequests.YesWithEmployerRecordReview, ManageRequests.No)]
    [MoqInlineAutoData(Operation.Recruitment, null, ManageRequests.No, ManageRequests.Yes)]
    [MoqInlineAutoData(Operation.RecruitmentRequiresReview, null, ManageRequests.No, ManageRequests.YesWithEmployerAdvertReview)]
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

        var permission = new ProviderPermission { Operations = new List<Operation>(), ProviderName = providerName, Ukprn = ukprn };
        if (operation1 != null) permission.Operations.Add(operation1.Value);
        if (operation2 != null) permission.Operations.Add(operation2.Value);

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);

        encodingServiceMock.Setup(e => e.Decode(employerAccountId, EncodingType.AccountId)).Returns(accountId);
        YourTrainingProvidersController sut = new(outerApiMock.Object, Mock.Of<ISessionService>(), encodingServiceMock.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } }
        };
        var permissions = new List<ProviderPermission> { permission };
        SetupControllerAndClasses(outerApiMock, accountId, accountName, publicHashedId, permissions, sut, false);

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.ChangePermissions, ChangePermissionsLink);

        var result = sut.Index(employerAccountId, new CancellationToken());

        var viewResult = result.Result.As<ViewResult>();
        var viewModel = viewResult.Model as YourTrainingProvidersViewModel;

        var actualLegalEntity = viewModel!.LegalEntities[0];

        var actualPermissionDetails = actualLegalEntity.PermissionDetails[0];

        actualPermissionDetails.Ukprn.Should().Be(ukprn);
        actualPermissionDetails.ProviderName.Should().Be(providerName);
        actualPermissionDetails.PermissionToAddRecords.Should().Be(expectedPermissionToAddApprenticesText);
        actualPermissionDetails.PermissionToRecruitApprentices.Should().Be(expectedPermissionToRecruitApprenticesText);
        actualPermissionDetails.ActionLink.Should().Be(ChangePermissionsLink);
        actualPermissionDetails.ActionLinkText.Should().Be(YourTrainingProviders.ChangePermissionsActionText);
    }

    [Test]
    [MoqAutoData]
    public void EmployerRelationship_HasOutstandingRequest_ReturnsExpectedActionText(
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        [Frozen] Mock<IEncodingService> encodingServiceMock,
        string employerAccountId,
        string providerName,
        long ukprn
    )
    {
        var accountId = 1123;
        var accountName = "Account Name";
        var publicHashedId = "12123232";

        var permission = new ProviderPermission
        {
            Operations = [],
            ProviderName = providerName,
            Ukprn = ukprn
        };

        var request = new PermissionRequest()
        {
            RequestId = Guid.NewGuid(),
            ProviderName = "ProviderName",
            Ukprn = ukprn,
            Operations = [],
            RequestType = RequestType.Permission
        };

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);

        encodingServiceMock.Setup(e => e.Decode(employerAccountId, EncodingType.AccountId)).Returns(accountId);

        YourTrainingProvidersController sut = new(outerApiMock.Object, Mock.Of<ISessionService>(), encodingServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            }
        };

        var permissions = new List<ProviderPermission> { permission };
        var requests = new List<PermissionRequest> { request };

        SetupControllerAndClasses(
            outerApiMock,
            accountId,
            accountName,
            publicHashedId,
            permissions,
            sut,
            false,
            requests
        );

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.Requests, RequestIdLink);
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.UpdatePermissions, RequestIdLink);

        var result = sut.Index(employerAccountId, new CancellationToken());

        var viewResult = result.Result.As<ViewResult>();
        var viewModel = viewResult.Model as YourTrainingProvidersViewModel;

        var actualLegalEntity = viewModel!.LegalEntities[0];

        var actualPermissionDetails = actualLegalEntity.PermissionDetails[0];

        Assert.Multiple(() =>
        {
            Assert.That(actualPermissionDetails.ActionLink, Is.EqualTo(RequestIdLink));
            Assert.That(actualPermissionDetails.ActionLinkText, Is.EqualTo(YourTrainingProviders.ViewRequestActionText));
        });
    }

    [Test]
    [MoqAutoData]
    public void EmployerRelationship_HasOutstandingAddAccountRequest_ShowNoPermissionsAssigned(
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        [Frozen] Mock<IEncodingService> encodingServiceMock,
        string employerAccountId,
        string providerName,
        long ukprn
    )
    {
        var accountId = 1123;
        var accountName = "Account Name";
        var publicHashedId = "12123232";

        var request = new PermissionRequest()
        {
            RequestId = Guid.NewGuid(),
            ProviderName = "ProviderName",
            Ukprn = ukprn,
            Operations = [Operation.CreateCohort, Operation.Recruitment],
            RequestType = RequestType.AddAccount
        };

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);

        encodingServiceMock.Setup(e => e.Decode(employerAccountId, EncodingType.AccountId)).Returns(accountId);

        YourTrainingProvidersController sut = new(outerApiMock.Object, Mock.Of<ISessionService>(), encodingServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            }
        };

        var requests = new List<PermissionRequest> { request };

        SetupControllerAndClasses(
            outerApiMock,
            accountId,
            accountName,
            publicHashedId,
            [],
            sut,
            false,
            requests
        );

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.Requests, RequestIdLink);
        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.AddAccount, RequestIdLink);

        var result = sut.Index(employerAccountId, new CancellationToken());

        var viewResult = result.Result.As<ViewResult>();
        var viewModel = viewResult.Model as YourTrainingProvidersViewModel;

        var actualLegalEntity = viewModel!.LegalEntities[0];

        var actualPermissionDetails = actualLegalEntity.PermissionDetails[0];

        Assert.Multiple(() =>
        {
            Assert.That(actualPermissionDetails.PermissionToAddRecords, Is.EqualTo(ManageRequests.No));
            Assert.That(actualPermissionDetails.PermissionToRecruitApprentices, Is.EqualTo(ManageRequests.No));
            Assert.That(actualPermissionDetails.HasOutstandingRequest, Is.True);
            Assert.That(actualPermissionDetails.ActionLink, Is.EqualTo(RequestIdLink));
            Assert.That(actualPermissionDetails.ActionLinkText, Is.EqualTo(YourTrainingProviders.ViewRequestActionText));
        });
    }

    [Test, MoqAutoData]
    public void ReturnsNoLegalEntities(
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
        var permissions = new List<ProviderPermission>();
        SetupControllerAndClasses(outerApiMock, accountId, accountName, publicHashedId, permissions, sut, false);

        var result = sut.Index(employerAccountId, new CancellationToken());

        var viewResult = result.Result.As<ViewResult>();
        var viewModel = viewResult.Model as YourTrainingProvidersViewModel;

        viewModel!.LegalEntities.Count.Should().Be(0);
    }

    [Test, MoqInlineAutoData]
    public async Task CallsSessionServiceDelete(
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

        var permission = new ProviderPermission { Operations = new List<Operation>(), ProviderName = providerName, Ukprn = ukprn };
        permission.Operations.Add(Operation.CreateCohort);
        permission.Operations.Add(Operation.Recruitment);

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
        var permissions = new List<ProviderPermission> { permission };
        SetupControllerAndClasses(outerApiMock, accountId, accountName, publicHashedId, permissions, sut, false);

        await sut.Index(employerAccountId, new CancellationToken());
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

        var permission = new ProviderPermission { Operations = new List<Operation>(), ProviderName = providerName, Ukprn = ukprn };
        permission.Operations.Add(Operation.CreateCohort);
        permission.Operations.Add(Operation.Recruitment);

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
        var permissions = new List<ProviderPermission> { permission };
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

        var permission = new ProviderPermission { Operations = new List<Operation>(), ProviderName = providerName, Ukprn = ukprn };
        permission.Operations.Add(Operation.CreateCohort);
        permission.Operations.Add(Operation.Recruitment);

        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>()).Returns(
            new AddTrainingProvidersSessionModel
            { SuccessfulAddition = isSuccessfulAddition, ProviderName = providerName });

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
        var permissions = new List<ProviderPermission> { permission };
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

        var permission = new ProviderPermission { Operations = new List<Operation>(), ProviderName = providerName, Ukprn = ukprn };
        permission.Operations.Add(Operation.CreateCohort);
        permission.Operations.Add(Operation.Recruitment);

        sessionServiceMock.Setup(x => x.Get<AddTrainingProvidersSessionModel>()).Returns(
            new AddTrainingProvidersSessionModel
            { SuccessfulAddition = isSuccessfulAddition, ProviderName = providerName });

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);
        sut.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
        var permissions = new List<ProviderPermission> { permission };
        SetupControllerAndClasses(outerApiMock, accountId, accountName, publicHashedId, permissions, sut, false);

        Mock<ITempDataDictionary> tempDataMock = new();
        tempDataMock.Setup(t => t[TempDataKeys.NameOfProviderUpdated]).Returns(providerName);

        sut.TempData = tempDataMock.Object;

        var result = sut.Index(employerAccountId, new CancellationToken());

        var viewResult = result.Result.As<ViewResult>();
        var viewModel = viewResult.Model as YourTrainingProvidersViewModel;
        viewModel!.ShowPermissionsUpdatedBanner().Should().BeTrue();
        viewModel.PermissionsUpdatedForProvider.Should().Be(providerName.ToUpper());
        var expectedText = $"You've set permissions for {providerName.ToUpper()}";
        viewModel.PermissionsUpdatedForProviderText.Should().Be(expectedText);
    }

    [Test]
    [MoqInlineAutoData]
    public void SuccessBanner_UpdatePermissionsAccepted_AddsCorrectValuesToModel(
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] YourTrainingProvidersController sut,
        string employerAccountId,
        string providerName,
        long ukprn
    )
    {
        var permission = new ProviderPermission
        {
            Operations = [Operation.CreateCohort, Operation.Recruitment],
            ProviderName = providerName,
            Ukprn = ukprn
        };

        sessionServiceMock.Setup(x =>
            x.Get<AddTrainingProvidersSessionModel>()).Returns(
                new AddTrainingProvidersSessionModel
                {
                    SuccessfulAddition = true,
                    ProviderName = providerName
                }
        );

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);

        sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = user
            }
        };

        var permissions = new List<ProviderPermission> { permission };

        SetupControllerAndClasses(
            outerApiMock,
            accountId: 1123,
            accountName: "Skills Training",
            publicHashedId: "12123232",
            permissions,
            sut,
            multipleAccounts: false
        );

        Mock<ITempDataDictionary> tempDataMock = new();

        tempDataMock.Setup(t => t[TempDataKeys.NameOfProviderUpdated]).Returns(providerName);
        tempDataMock.Setup(t => t[TempDataKeys.RequestTypeActioned]).Returns(RequestType.Permission.ToString());
        tempDataMock.Setup(t => t[TempDataKeys.RequestAction]).Returns(RequestAction.Accepted.ToString());

        sut.TempData = tempDataMock.Object;

        var result = sut.Index(employerAccountId, new CancellationToken());

        var viewResult = result.Result.As<ViewResult>();
        var viewModel = viewResult.Model as YourTrainingProvidersViewModel;

        Assert.Multiple(() =>
        {
            Assert.That(viewModel!.ShowPermissionsUpdatedBanner(), Is.True);
            Assert.That(viewModel.PermissionsUpdatedForProvider, Is.EqualTo(providerName.ToUpper()));
            Assert.That(viewModel.PermissionsUpdatedForProviderText, Is.EqualTo($"You've set {providerName.ToUpper()}’s permissions."));
        });
    }

    [Test]
    [MoqInlineAutoData]
    public void SuccessBanner_UpdatePermissionsDeclined_AddsCorrectValuesToModel(
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] YourTrainingProvidersController sut,
        string employerAccountId,
        string providerName,
        long ukprn
    )
    {
        var permission = new ProviderPermission
        {
            Operations = [Operation.CreateCohort, Operation.Recruitment],
            ProviderName = providerName,
            Ukprn = ukprn
        };

        sessionServiceMock.Setup(x =>
            x.Get<AddTrainingProvidersSessionModel>()).Returns(
                new AddTrainingProvidersSessionModel
                {
                    SuccessfulAddition = true,
                    ProviderName = providerName
                }
        );

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);

        sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = user
            }
        };

        var permissions = new List<ProviderPermission> { permission };

        SetupControllerAndClasses(
            outerApiMock,
            accountId: 1123,
            accountName: "Skills Training",
            publicHashedId: "12123232",
            permissions,
            sut,
            multipleAccounts: false
        );

        Mock<ITempDataDictionary> tempDataMock = new();

        tempDataMock.Setup(t => t[TempDataKeys.NameOfProviderUpdated]).Returns(providerName);
        tempDataMock.Setup(t => t[TempDataKeys.RequestTypeActioned]).Returns(RequestType.Permission.ToString());
        tempDataMock.Setup(t => t[TempDataKeys.RequestAction]).Returns(RequestAction.Declined.ToString());

        sut.TempData = tempDataMock.Object;

        var result = sut.Index(employerAccountId, new CancellationToken());

        var viewResult = result.Result.As<ViewResult>();
        var viewModel = viewResult.Model as YourTrainingProvidersViewModel;

        Assert.Multiple(() =>
        {
            Assert.That(viewModel!.ShowPermissionsUpdatedBanner(), Is.True);
            Assert.That(viewModel.PermissionsUpdatedForProvider, Is.EqualTo(providerName.ToUpper()));
            Assert.That(viewModel.PermissionsUpdatedForProviderText, Is.EqualTo($"You've declined {providerName.ToUpper()}’s permission request."));
        });
    }

    [Test]
    [MoqInlineAutoData]
    public void SuccessBanner_AddAccountRequestAccepted_AddsCorrectValuesToModel(
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        [Frozen] Mock<ISessionService> sessionServiceMock,
        [Greedy] YourTrainingProvidersController sut,
        string employerAccountId,
        string providerName,
        long ukprn
    )
    {
        var permission = new ProviderPermission
        {
            Operations = [Operation.CreateCohort, Operation.Recruitment],
            ProviderName = providerName,
            Ukprn = ukprn
        };

        sessionServiceMock.Setup(x =>
            x.Get<AddTrainingProvidersSessionModel>()).Returns(
                new AddTrainingProvidersSessionModel
                {
                    SuccessfulAddition = true,
                    ProviderName = providerName
                }
        );

        ClaimsPrincipal user = UsersForTesting.GetUserWithClaims(employerAccountId, EmployerUserRole.Owner);

        sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = user
            }
        };

        var permissions = new List<ProviderPermission> { permission };

        SetupControllerAndClasses(
            outerApiMock,
            accountId: 1123,
            accountName: "Skills Training",
            publicHashedId: "12123232",
            permissions,
            sut,
            multipleAccounts: false
        );

        Mock<ITempDataDictionary> tempDataMock = new();

        tempDataMock.Setup(t => t[TempDataKeys.NameOfProviderUpdated]).Returns(providerName);
        tempDataMock.Setup(t => t[TempDataKeys.RequestTypeActioned]).Returns(RequestType.AddAccount.ToString());
        tempDataMock.Setup(t => t[TempDataKeys.RequestAction]).Returns(RequestAction.Accepted.ToString());

        sut.TempData = tempDataMock.Object;

        var result = sut.Index(employerAccountId, new CancellationToken());

        var viewResult = result.Result.As<ViewResult>();
        var viewModel = viewResult.Model as YourTrainingProvidersViewModel;

        Assert.Multiple(() =>
        {
            Assert.That(viewModel!.ShowPermissionsUpdatedBanner(), Is.True);
            Assert.That(viewModel.PermissionsUpdatedForProvider, Is.EqualTo(providerName.ToUpper()));
            Assert.That(viewModel.PermissionsUpdatedForProviderText, Is.EqualTo($"You've added {providerName.ToUpper()} and set their permissions."));
        });
    }

    [Test, MoqAutoData]
    public async Task IgnoresAnyCreateAccountRequests(
        [Frozen] Mock<IOuterApiClient> outerApiClientMock,
        [Frozen] Mock<IEncodingService> encodingServiceMock,
        [Greedy] YourTrainingProvidersController sut,
        string hashedEmployerAccountId,
        long employerAccountId,
        LegalEntity legalEntityWithCreateAccountRequest,
        PermissionRequest createAccountPermissionRequest,
        LegalEntity legalEntityWithAddAccountRequest,
        PermissionRequest addAccountPermissionRequest,
        CancellationToken cancellationToken)
    {
        encodingServiceMock.Setup(e => e.Decode(hashedEmployerAccountId, EncodingType.AccountId)).Returns(employerAccountId);

        sut.AddDefaultContext().AddUrlHelperMock().AddUrlForRoute(RouteNames.ChangePermissions, ChangePermissionsLink);
        Mock<ITempDataDictionary> tempDataMock = new();
        sut.TempData = tempDataMock.Object;

        createAccountPermissionRequest.RequestType = RequestType.CreateAccount;
        legalEntityWithCreateAccountRequest.Requests.Clear();
        legalEntityWithCreateAccountRequest.Permissions.Clear();
        legalEntityWithCreateAccountRequest.Requests.Add(createAccountPermissionRequest);
        addAccountPermissionRequest.RequestType = RequestType.AddAccount;
        legalEntityWithAddAccountRequest.Requests.Clear();
        legalEntityWithAddAccountRequest.Permissions.Clear();
        legalEntityWithAddAccountRequest.Requests.Add(addAccountPermissionRequest);
        GetEmployerRelationshipsQueryResponse relationshipsResponse = new([legalEntityWithCreateAccountRequest, legalEntityWithAddAccountRequest]);
        relationshipsResponse.AccountLegalEntities.Add(legalEntityWithCreateAccountRequest);
        outerApiClientMock.Setup(o => o.GetEmployerRelationships(employerAccountId, cancellationToken)).ReturnsAsync(relationshipsResponse);

        /// Act
        var response = await sut.Index(hashedEmployerAccountId, cancellationToken);

        response.As<ViewResult>().Model.As<YourTrainingProvidersViewModel>().LegalEntities.Should().NotContain(l => l.LegalEntityId == legalEntityWithCreateAccountRequest.Id);
        response.As<ViewResult>().Model.As<YourTrainingProvidersViewModel>().LegalEntities.Should().Contain(l => l.LegalEntityId == legalEntityWithAddAccountRequest.Id);
    }

    private static void SetupControllerAndClasses(
        Mock<IOuterApiClient> outerApiMock,
        int accountId,
        string accountName,
        string publicHashedId,
        List<ProviderPermission> permissions,
        YourTrainingProvidersController sut,
        bool multipleAccounts,
        List<PermissionRequest>? requests = null
    )
    {
        List<LegalEntity> accountLegalEntities = new List<LegalEntity>
        {
            new()
            {
                AccountId = accountId, Id = 1, Name = accountName, PublicHashedId = publicHashedId,
                Permissions = permissions,
                Requests = requests ?? []
            }
        };

        if (multipleAccounts)
        {
            accountLegalEntities.Add(new LegalEntity
            { AccountId = 1234, Id = 2, Name = "name 2", PublicHashedId = "AFC", Permissions = permissions });
        }

        GetEmployerRelationshipsQueryResponse response = new GetEmployerRelationshipsQueryResponse(accountLegalEntities);

        outerApiMock.Setup(o => o.GetEmployerRelationships(accountId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        Mock<ITempDataDictionary> tempDataMock = new();
        sut.TempData = tempDataMock.Object;

        sut.AddUrlHelperMock().AddUrlForRoute(RouteNames.SelectLegalEntity, SelectLegalEntityUrl);
    }
}
