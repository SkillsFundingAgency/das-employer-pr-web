using FluentValidation.TestHelper;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Employer.PR.Web.Validators;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Validators;
public class AddPermissionsSubmitViewModelValidatorTests
{
    [TestCase(SetPermissions.AddRecords.Yes, SetPermissions.RecruitApprentices.Yes)]
    [TestCase(SetPermissions.AddRecords.No, SetPermissions.RecruitApprentices.Yes)]
    [TestCase(SetPermissions.AddRecords.Yes, SetPermissions.RecruitApprentices.YesWithReview)]
    [TestCase(SetPermissions.AddRecords.No, SetPermissions.RecruitApprentices.YesWithReview)]
    [TestCase(SetPermissions.AddRecords.Yes, SetPermissions.RecruitApprentices.No)]
    public void AddRecordsAndRecruitApprenticesSet_Valid(string addRecordsSelection, string recruitApprenticesSelection)
    {
        var model = new AddPermissionsSubmitViewModel()
        {
            PermissionToAddCohorts = addRecordsSelection,
            PermissionToRecruit = recruitApprenticesSelection
        };

        var sut = new AddPermissionsSubmitViewModelValidator();
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
        var model = new AddPermissionsSubmitViewModel()
        {
            PermissionToRecruit = recruitApprenticesSelection
        };

        var sut = new AddPermissionsSubmitViewModelValidator();
        var result = sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(c => c.PermissionToAddCohorts)
            .WithErrorMessage(AddPermissionsSubmitViewModelValidator.AddRecordsNotSelectedErrorMessage);
    }

    [TestCase(SetPermissions.AddRecords.Yes)]
    [TestCase(SetPermissions.AddRecords.No)]
    public void RecruitApprenticesNotSet_InvalidWithExpectedMessage(string addRecordsSelection)
    {
        var model = new AddPermissionsSubmitViewModel()
        {
            PermissionToAddCohorts = addRecordsSelection
        };

        var sut = new AddPermissionsSubmitViewModelValidator();
        var result = sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(c => c.PermissionToRecruit)
            .WithErrorMessage(AddPermissionsSubmitViewModelValidator.RecruitApprenticesNotSelectedErrorMessage);
    }

    [Test]
    public void AddRecordsNo_RecruitApprenticesNo_InvalidWithExpectedMessage()
    {
        var model = new AddPermissionsSubmitViewModel
        {
            PermissionToAddCohorts = SetPermissions.AddRecords.No,
            PermissionToRecruit = SetPermissions.RecruitApprentices.No
        };

        var sut = new AddPermissionsSubmitViewModelValidator();
        var result = sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(c => c.PermissionToAddCohorts)
            .WithErrorMessage(AddPermissionsSubmitViewModelValidator.BothSelectionsAreNoErrorMessage);
    }
}
