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
            AddRecords = addRecordsSelection,
            RecruitApprentices = recruitApprenticesSelection
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
            RecruitApprentices = recruitApprenticesSelection
        };

        var sut = new SetPermissionsSubmitViewModelValidator();
        var result = sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(c => c.AddRecords)
            .WithErrorMessage(SetPermissionsSubmitViewModelValidator.AddRecordsNotSelectedErrorMessage);
    }

    [TestCase(SetPermissions.AddRecords.Yes)]
    [TestCase(SetPermissions.AddRecords.No)]
    public void RecruitApprenticesNotSet_InvalidWithExpectedMessage(string addRecordsSelection)
    {
        var model = new SetPermissionsSubmitViewModel()
        {
            AddRecords = addRecordsSelection
        };

        var sut = new SetPermissionsSubmitViewModelValidator();
        var result = sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(c => c.RecruitApprentices)
            .WithErrorMessage(SetPermissionsSubmitViewModelValidator.RecruitApprenticesNotSelectedErrorMessage);
    }

    [Test]
    public void AddRecordsNo_RecruitApprenticesNo_InvalidWithExpectedMessage()
    {
        var model = new SetPermissionsSubmitViewModel
        {
            AddRecords = SetPermissions.AddRecords.No,
            RecruitApprentices = SetPermissions.RecruitApprentices.No
        };

        var sut = new SetPermissionsSubmitViewModelValidator();
        var result = sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(c => c.AddRecords)
            .WithErrorMessage(SetPermissionsSubmitViewModelValidator.BothSelectionsAreNoErrorMessage);
    }
}
