using FluentValidation.TestHelper;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Employer.PR.Web.Validators;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Validators;
public class SetPermissionsSubmitViewModelValidatorTests
{
    [TestCase(SetPermissions.AddRecords.Yes, SetPermissions.RecruitApprentices.Yes)]
    [TestCase(SetPermissions.AddRecords.No, SetPermissions.RecruitApprentices.Yes)]
    [TestCase(SetPermissions.AddRecords.Yes, SetPermissions.RecruitApprentices.YesWithReview)]
    [TestCase(SetPermissions.AddRecords.No, SetPermissions.RecruitApprentices.YesWithReview)]
    [TestCase(SetPermissions.AddRecords.Yes, SetPermissions.RecruitApprentices.No)]
    public void AddRecordsAndRecruitApprenticesSet_Valid(string addRecordsSelection, string recruitApprenticesSelection)
    {
        var model = new SetPermissionsSubmitViewModel()
        {
            PermissionToAddCohorts = addRecordsSelection,
            PermissionToRecruit = recruitApprenticesSelection
        };

        var sut = new SetPermissionsSubmitViewModelValidator();
        var result = sut.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [TestCase(SetPermissions.RecruitApprentices.Yes)]
    [TestCase(SetPermissions.RecruitApprentices.Yes)]
    [TestCase(SetPermissions.RecruitApprentices.YesWithReview)]
    [TestCase(SetPermissions.RecruitApprentices.YesWithReview)]
    [TestCase(SetPermissions.RecruitApprentices.No)]
    public void AddRecordsNotSet_InvalidWithExpectedMessage(string recruitApprenticesSelection)
    {
        var model = new SetPermissionsSubmitViewModel()
        {
            PermissionToRecruit = recruitApprenticesSelection
        };

        var sut = new SetPermissionsSubmitViewModelValidator();
        var result = sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(c => c.PermissionToAddCohorts)
            .WithErrorMessage(SetPermissionsSubmitViewModelValidator.AddRecordsNotSelectedErrorMessage);
    }

    [TestCase(SetPermissions.AddRecords.Yes)]
    [TestCase(SetPermissions.AddRecords.No)]
    public void RecruitApprenticesNotSet_InvalidWithExpectedMessage(string addRecordsSelection)
    {
        var model = new SetPermissionsSubmitViewModel()
        {
            PermissionToAddCohorts = addRecordsSelection
        };

        var sut = new SetPermissionsSubmitViewModelValidator();
        var result = sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(c => c.PermissionToRecruit)
            .WithErrorMessage(SetPermissionsSubmitViewModelValidator.RecruitApprenticesNotSelectedErrorMessage);
    }

    [Test]
    public void AddRecordsNo_RecruitApprenticesNo_InvalidWithExpectedMessage()
    {
        var model = new SetPermissionsSubmitViewModel
        {
            PermissionToAddCohorts = SetPermissions.AddRecords.No,
            PermissionToRecruit = SetPermissions.RecruitApprentices.No
        };

        var sut = new SetPermissionsSubmitViewModelValidator();
        var result = sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(c => c.PermissionToAddCohorts)
            .WithErrorMessage(SetPermissionsSubmitViewModelValidator.BothSelectionsAreNoErrorMessage);
    }
}
