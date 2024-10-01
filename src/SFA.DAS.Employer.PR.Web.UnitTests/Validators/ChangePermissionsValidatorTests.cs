using FluentValidation.TestHelper;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Employer.PR.Web.Validators;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Validators;
public class ChangePermissionsValidatorTests
{
    [TestCase(SetPermissions.AddRecords.Yes, SetPermissions.AddRecords.Yes, SetPermissions.RecruitApprentices.Yes, SetPermissions.RecruitApprentices.YesWithReview)]
    [TestCase(SetPermissions.AddRecords.Yes, SetPermissions.AddRecords.Yes, SetPermissions.RecruitApprentices.Yes, SetPermissions.RecruitApprentices.No)]
    [TestCase(SetPermissions.AddRecords.Yes, SetPermissions.AddRecords.Yes, SetPermissions.RecruitApprentices.No, SetPermissions.RecruitApprentices.YesWithReview)]
    [TestCase(SetPermissions.AddRecords.Yes, SetPermissions.AddRecords.Yes, SetPermissions.RecruitApprentices.No, SetPermissions.RecruitApprentices.Yes)]
    [TestCase(SetPermissions.AddRecords.Yes, SetPermissions.AddRecords.Yes, SetPermissions.RecruitApprentices.YesWithReview, SetPermissions.RecruitApprentices.Yes)]
    [TestCase(SetPermissions.AddRecords.Yes, SetPermissions.AddRecords.Yes, SetPermissions.RecruitApprentices.YesWithReview, SetPermissions.RecruitApprentices.No)]
    [TestCase(SetPermissions.AddRecords.No, SetPermissions.AddRecords.No, SetPermissions.RecruitApprentices.Yes, SetPermissions.RecruitApprentices.YesWithReview)]
    [TestCase(SetPermissions.AddRecords.No, SetPermissions.AddRecords.No, SetPermissions.RecruitApprentices.Yes, SetPermissions.RecruitApprentices.No)]
    [TestCase(SetPermissions.AddRecords.No, SetPermissions.AddRecords.No, SetPermissions.RecruitApprentices.No, SetPermissions.RecruitApprentices.YesWithReview)]
    [TestCase(SetPermissions.AddRecords.No, SetPermissions.AddRecords.No, SetPermissions.RecruitApprentices.No, SetPermissions.RecruitApprentices.Yes)]
    [TestCase(SetPermissions.AddRecords.No, SetPermissions.AddRecords.No, SetPermissions.RecruitApprentices.YesWithReview, SetPermissions.RecruitApprentices.Yes)]
    [TestCase(SetPermissions.AddRecords.No, SetPermissions.AddRecords.No, SetPermissions.RecruitApprentices.YesWithReview, SetPermissions.RecruitApprentices.No)]
    [TestCase(SetPermissions.AddRecords.Yes, SetPermissions.AddRecords.No, SetPermissions.RecruitApprentices.Yes, SetPermissions.RecruitApprentices.Yes)]
    [TestCase(SetPermissions.AddRecords.No, SetPermissions.AddRecords.Yes, SetPermissions.RecruitApprentices.Yes, SetPermissions.RecruitApprentices.Yes)]
    [TestCase(SetPermissions.AddRecords.Yes, SetPermissions.AddRecords.No, SetPermissions.RecruitApprentices.No, SetPermissions.RecruitApprentices.No)]
    [TestCase(SetPermissions.AddRecords.No, SetPermissions.AddRecords.Yes, SetPermissions.RecruitApprentices.No, SetPermissions.RecruitApprentices.No)]
    [TestCase(SetPermissions.AddRecords.Yes, SetPermissions.AddRecords.No, SetPermissions.RecruitApprentices.YesWithReview, SetPermissions.RecruitApprentices.YesWithReview)]
    [TestCase(SetPermissions.AddRecords.No, SetPermissions.AddRecords.Yes, SetPermissions.RecruitApprentices.YesWithReview, SetPermissions.RecruitApprentices.YesWithReview)]
    [TestCase(SetPermissions.AddRecords.Yes, SetPermissions.AddRecords.No, SetPermissions.RecruitApprentices.Yes, SetPermissions.RecruitApprentices.YesWithReview)]
    [TestCase(SetPermissions.AddRecords.Yes, SetPermissions.AddRecords.No, SetPermissions.RecruitApprentices.Yes, SetPermissions.RecruitApprentices.No)]
    [TestCase(SetPermissions.AddRecords.Yes, SetPermissions.AddRecords.No, SetPermissions.RecruitApprentices.No, SetPermissions.RecruitApprentices.YesWithReview)]
    [TestCase(SetPermissions.AddRecords.Yes, SetPermissions.AddRecords.No, SetPermissions.RecruitApprentices.No, SetPermissions.RecruitApprentices.Yes)]
    [TestCase(SetPermissions.AddRecords.Yes, SetPermissions.AddRecords.No, SetPermissions.RecruitApprentices.YesWithReview, SetPermissions.RecruitApprentices.Yes)]
    [TestCase(SetPermissions.AddRecords.Yes, SetPermissions.AddRecords.No, SetPermissions.RecruitApprentices.YesWithReview, SetPermissions.RecruitApprentices.No)]
    [TestCase(SetPermissions.AddRecords.No, SetPermissions.AddRecords.Yes, SetPermissions.RecruitApprentices.Yes, SetPermissions.RecruitApprentices.YesWithReview)]
    [TestCase(SetPermissions.AddRecords.No, SetPermissions.AddRecords.Yes, SetPermissions.RecruitApprentices.Yes, SetPermissions.RecruitApprentices.No)]
    [TestCase(SetPermissions.AddRecords.No, SetPermissions.AddRecords.Yes, SetPermissions.RecruitApprentices.No, SetPermissions.RecruitApprentices.YesWithReview)]
    [TestCase(SetPermissions.AddRecords.No, SetPermissions.AddRecords.Yes, SetPermissions.RecruitApprentices.No, SetPermissions.RecruitApprentices.Yes)]
    [TestCase(SetPermissions.AddRecords.No, SetPermissions.AddRecords.Yes, SetPermissions.RecruitApprentices.YesWithReview, SetPermissions.RecruitApprentices.Yes)]
    [TestCase(SetPermissions.AddRecords.No, SetPermissions.AddRecords.Yes, SetPermissions.RecruitApprentices.YesWithReview, SetPermissions.RecruitApprentices.No)]
    public void AddRecordsAndRecruitApprenticesChanged_Valid(string addRecordsSelected, string addRecordsOriginal, string recruitApprenticesSelected, string recruitApprenticesOriginal)
    {
        var model = new ChangePermissionsSubmitModel()
        {
            PermissionToAddCohorts = addRecordsSelected,
            PermissionToAddCohortsOriginal = addRecordsOriginal,
            PermissionToRecruit = recruitApprenticesSelected,
            PermissionToRecruitOriginal = recruitApprenticesOriginal
        };

        var sut = new ChangePermissionsValidator();
        var result = sut.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [TestCase(SetPermissions.AddRecords.Yes, SetPermissions.RecruitApprentices.Yes)]
    [TestCase(SetPermissions.AddRecords.No, SetPermissions.RecruitApprentices.Yes)]
    [TestCase(SetPermissions.AddRecords.Yes, SetPermissions.RecruitApprentices.No)]
    [TestCase(SetPermissions.AddRecords.No, SetPermissions.RecruitApprentices.No)]
    [TestCase(SetPermissions.AddRecords.Yes, SetPermissions.RecruitApprentices.YesWithReview)]
    [TestCase(SetPermissions.AddRecords.No, SetPermissions.RecruitApprentices.YesWithReview)]
    public void NoPermissionsChanged_InvalidWithExpectedMessage(string addRecordsSet, string recruitApprenticesSet)
    {
        var model = new ChangePermissionsSubmitModel()
        {
            PermissionToAddCohorts = addRecordsSet,
            PermissionToAddCohortsOriginal = addRecordsSet,
            PermissionToRecruit = recruitApprenticesSet,
            PermissionToRecruitOriginal = recruitApprenticesSet
        };

        var sut = new ChangePermissionsValidator();
        var result = sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(c => c.PermissionToAddCohorts)
            .WithErrorMessage(ChangePermissionsValidator.NotChangedPermissionsErrorMessage);
    }

}
