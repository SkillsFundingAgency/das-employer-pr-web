using SFA.DAS.Employer.PR.Domain.Common;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Helpers;
using SFA.DAS.Employer.PR.Web.Models;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Helpers;

public sealed class ReviewRequestHelperTests
{
    [Test]
    public void IsValidRequest_WhenResponseIsNull_ReturnsFalse()
    {
        var sut = ReviewRequestHelper.IsValidRequest(null, RequestType.Permission);
        Assert.That(sut, Is.False);
    }

    [Test]
    public void IsValidRequest_WhenStatusIsInvalid_ReturnsFalse()
    {
        var response = new GetPermissionRequestResponse
        {
            RequestType = RequestType.Permission,
            Status = RequestStatus.Accepted,
            ProviderName = "Provider Name",
            RequestedBy = Guid.NewGuid().ToString()
        };

        var sut = ReviewRequestHelper.IsValidRequest(response, RequestType.Permission);
        Assert.That(sut, Is.False);
    }

    [Test]
    public void IsValidRequest_WhenResponseIsValid_ReturnsTrue()
    {
        var response = new GetPermissionRequestResponse
        {
            RequestType = RequestType.Permission,
            Status = RequestStatus.New,
            ProviderName = "Provider Name",
            RequestedBy = Guid.NewGuid().ToString()
        };

        var sut = ReviewRequestHelper.IsValidRequest(response, RequestType.Permission);
        Assert.That(sut, Is.True);
    }

    [Test]
    public void MapOperationsToDescriptions_WhenCreateCohortOperationExists_SetsAddAccountRequestTextToYesReviewRecords()
    {
        var sut = new ReviewAddAccountRequestViewModel() { ProviderName = "Provider Name", ViewYourTrainingProvidersLink = "link" };
        var operations = new[] { Operation.CreateCohort };

        ReviewRequestHelper.MapOperationsToDescriptions(ref sut, operations);

        Assert.That(sut.AddApprenticeRecordsText, Is.EqualTo(ManageRequests.YesWithEmployerRecordReview));
    }

    [Test]
    public void MapOperationsToDescriptions_WhenCreateCohortOperationDoesNotExist_SetsAddApprenticeRecordsTextToNo()
    {
        var sut = new ReviewAddAccountRequestViewModel() { ProviderName = "Provider Name", ViewYourTrainingProvidersLink = "link" };

        ReviewRequestHelper.MapOperationsToDescriptions(ref sut, []);

        Assert.That(sut.AddApprenticeRecordsText, Is.EqualTo(ManageRequests.No));
    }

    [Test]
    public void MapOperationsToDescriptions_WhenRecruitmentOperationExists_SetsRecruitApprenticesTextToYes()
    {
        var sut = new ReviewAddAccountRequestViewModel() { ProviderName = "Provider Name", ViewYourTrainingProvidersLink = "link" };
        var operations = new[] { Operation.Recruitment };

        ReviewRequestHelper.MapOperationsToDescriptions(ref sut, operations);

        Assert.That(sut.RecruitApprenticesText, Is.EqualTo(ManageRequests.Yes));
    }

    [Test]
    public void MapOperationsToDescriptions_WhenRecruitmentRequiresReviewOperationExists_SetsRecruitApprenticesTextToYesWithEmployerReview()
    {
        var sut = new ReviewAddAccountRequestViewModel() { ProviderName = "Provider Name", ViewYourTrainingProvidersLink = "link" };
        var operations = new[] { Operation.RecruitmentRequiresReview };

        ReviewRequestHelper.MapOperationsToDescriptions(ref sut, operations);

        Assert.That(sut.RecruitApprenticesText, Is.EqualTo(ManageRequests.YesWithEmployerAdvertReview));
    }

    [Test]
    public void MapOperationsToDescriptions_WhenNoRelevantOperationExists_SetsRecruitApprenticesTextToNo()
    {
        var sut = new ReviewAddAccountRequestViewModel() { ProviderName = "Provider Name", ViewYourTrainingProvidersLink = "link" };

        ReviewRequestHelper.MapOperationsToDescriptions(ref sut, []);

        Assert.That(sut.RecruitApprenticesText, Is.EqualTo(ManageRequests.No));
    }
}
