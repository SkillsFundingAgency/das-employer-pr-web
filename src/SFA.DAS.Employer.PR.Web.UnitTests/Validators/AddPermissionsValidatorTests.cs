using FluentValidation.TestHelper;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Employer.PR.Web.Validators;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Validators;
public class AddPermissionsValidatorTests
{
    [TestCase(SetPermissions.AddRecords.Yes, SetPermissions.RecruitApprentices.Yes)]
    [TestCase(SetPermissions.AddRecords.No, SetPermissions.RecruitApprentices.Yes)]
    [TestCase(SetPermissions.AddRecords.Yes, SetPermissions.RecruitApprentices.YesWithReview)]
    [TestCase(SetPermissions.AddRecords.No, SetPermissions.RecruitApprentices.YesWithReview)]
    [TestCase(SetPermissions.AddRecords.Yes, SetPermissions.RecruitApprentices.No)]
    public void AddRecordsAndRecruitApprenticesSet_Valid(string addRecordsSelection, string recruitApprenticesSelection)
    {
        var model = new AddPermissionsSubmitModel()
        {
            PermissionToAddCohorts = addRecordsSelection,
            PermissionToRecruit = recruitApprenticesSelection
        };

        var sut = new AddPermissionsValidator();
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
        var model = new AddPermissionsSubmitModel()
        {
            PermissionToRecruit = recruitApprenticesSelection
        };

        var sut = new AddPermissionsValidator();
        var result = sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(c => c.PermissionToAddCohorts)
            .WithErrorMessage(AddPermissionsValidator.AddRecordsNotSelectedErrorMessage);
    }

    [TestCase(SetPermissions.AddRecords.Yes)]
    [TestCase(SetPermissions.AddRecords.No)]
    public void RecruitApprenticesNotSet_InvalidWithExpectedMessage(string addRecordsSelection)
    {
        var model = new AddPermissionsSubmitModel()
        {
            PermissionToAddCohorts = addRecordsSelection
        };

        var sut = new AddPermissionsValidator();
        var result = sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(c => c.PermissionToRecruit)
            .WithErrorMessage(AddPermissionsValidator.RecruitApprenticesNotSelectedErrorMessage);
    }

    [Test]
    public void AddRecordsNo_RecruitApprenticesNo_InvalidWithExpectedMessage()
    {
        var model = new AddPermissionsSubmitModel
        {
            PermissionToAddCohorts = SetPermissions.AddRecords.No,
            PermissionToRecruit = SetPermissions.RecruitApprentices.No
        };

        var sut = new AddPermissionsValidator();
        var result = sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(c => c.PermissionToAddCohorts)
            .WithErrorMessage(AddPermissionsValidator.BothSelectionsAreNoErrorMessage);
    }
}
