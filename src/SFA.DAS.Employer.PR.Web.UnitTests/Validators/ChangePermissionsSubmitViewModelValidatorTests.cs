using FluentValidation.TestHelper;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Employer.PR.Web.Services;
using SFA.DAS.Employer.PR.Web.Validators;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Validators;
public class ChangePermissionsSubmitViewModelValidatorTests
{
    [TestCase(OperationsMappingService.Yes, OperationsMappingService.Yes, OperationsMappingService.Yes, OperationsMappingService.YesWithReview)]
    [TestCase(OperationsMappingService.Yes, OperationsMappingService.Yes, OperationsMappingService.Yes, OperationsMappingService.No)]
    [TestCase(OperationsMappingService.Yes, OperationsMappingService.Yes, OperationsMappingService.No, OperationsMappingService.YesWithReview)]
    [TestCase(OperationsMappingService.Yes, OperationsMappingService.Yes, OperationsMappingService.No, OperationsMappingService.Yes)]
    [TestCase(OperationsMappingService.Yes, OperationsMappingService.Yes, OperationsMappingService.YesWithReview, OperationsMappingService.Yes)]
    [TestCase(OperationsMappingService.Yes, OperationsMappingService.Yes, OperationsMappingService.YesWithReview, OperationsMappingService.No)]
    [TestCase(OperationsMappingService.No, OperationsMappingService.No, OperationsMappingService.Yes, OperationsMappingService.YesWithReview)]
    [TestCase(OperationsMappingService.No, OperationsMappingService.No, OperationsMappingService.Yes, OperationsMappingService.No)]
    [TestCase(OperationsMappingService.No, OperationsMappingService.No, OperationsMappingService.No, OperationsMappingService.YesWithReview)]
    [TestCase(OperationsMappingService.No, OperationsMappingService.No, OperationsMappingService.No, OperationsMappingService.Yes)]
    [TestCase(OperationsMappingService.No, OperationsMappingService.No, OperationsMappingService.YesWithReview, OperationsMappingService.Yes)]
    [TestCase(OperationsMappingService.No, OperationsMappingService.No, OperationsMappingService.YesWithReview, OperationsMappingService.No)]
    [TestCase(OperationsMappingService.Yes, OperationsMappingService.No, OperationsMappingService.Yes, OperationsMappingService.Yes)]
    [TestCase(OperationsMappingService.No, OperationsMappingService.Yes, OperationsMappingService.Yes, OperationsMappingService.Yes)]
    [TestCase(OperationsMappingService.Yes, OperationsMappingService.No, OperationsMappingService.No, OperationsMappingService.No)]
    [TestCase(OperationsMappingService.No, OperationsMappingService.Yes, OperationsMappingService.No, OperationsMappingService.No)]
    [TestCase(OperationsMappingService.Yes, OperationsMappingService.No, OperationsMappingService.YesWithReview, OperationsMappingService.YesWithReview)]
    [TestCase(OperationsMappingService.No, OperationsMappingService.Yes, OperationsMappingService.YesWithReview, OperationsMappingService.YesWithReview)]
    [TestCase(OperationsMappingService.Yes, OperationsMappingService.No, OperationsMappingService.Yes, OperationsMappingService.YesWithReview)]
    [TestCase(OperationsMappingService.Yes, OperationsMappingService.No, OperationsMappingService.Yes, OperationsMappingService.No)]
    [TestCase(OperationsMappingService.Yes, OperationsMappingService.No, OperationsMappingService.No, OperationsMappingService.YesWithReview)]
    [TestCase(OperationsMappingService.Yes, OperationsMappingService.No, OperationsMappingService.No, OperationsMappingService.Yes)]
    [TestCase(OperationsMappingService.Yes, OperationsMappingService.No, OperationsMappingService.YesWithReview, OperationsMappingService.Yes)]
    [TestCase(OperationsMappingService.Yes, OperationsMappingService.No, OperationsMappingService.YesWithReview, OperationsMappingService.No)]
    [TestCase(OperationsMappingService.No, OperationsMappingService.Yes, OperationsMappingService.Yes, OperationsMappingService.YesWithReview)]
    [TestCase(OperationsMappingService.No, OperationsMappingService.Yes, OperationsMappingService.Yes, OperationsMappingService.No)]
    [TestCase(OperationsMappingService.No, OperationsMappingService.Yes, OperationsMappingService.No, OperationsMappingService.YesWithReview)]
    [TestCase(OperationsMappingService.No, OperationsMappingService.Yes, OperationsMappingService.No, OperationsMappingService.Yes)]
    [TestCase(OperationsMappingService.No, OperationsMappingService.Yes, OperationsMappingService.YesWithReview, OperationsMappingService.Yes)]
    [TestCase(OperationsMappingService.No, OperationsMappingService.Yes, OperationsMappingService.YesWithReview, OperationsMappingService.No)]
    public void AddRecordsAndRecruitApprenticesChanged_Valid(string addRecordsSelected, string addRecordsOriginal, string recruitApprenticesSelected, string recruitApprenticesOriginal)
    {
        var model = new ChangePermissionsSubmitViewModel()
        {
            PermissionToAddCohorts = addRecordsSelected,
            PermissionToAddCohortsOriginal = addRecordsOriginal,
            PermissionToRecruit = recruitApprenticesSelected,
            PermissionToRecruitOriginal = recruitApprenticesOriginal
        };

        var sut = new ChangePermissionsSubmitViewModelValidator();
        var result = sut.TestValidate(model);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [TestCase(OperationsMappingService.Yes, OperationsMappingService.Yes)]
    [TestCase(OperationsMappingService.No, OperationsMappingService.Yes)]
    [TestCase(OperationsMappingService.Yes, OperationsMappingService.No)]
    [TestCase(OperationsMappingService.No, OperationsMappingService.No)]
    [TestCase(OperationsMappingService.Yes, OperationsMappingService.YesWithReview)]
    [TestCase(OperationsMappingService.No, OperationsMappingService.YesWithReview)]
    public void NoPermissionsChanged_InvalidWithExpectedMessage(string addRecordsSet, string recruitApprenticesSet)
    {
        var model = new ChangePermissionsSubmitViewModel()
        {
            PermissionToAddCohorts = addRecordsSet,
            PermissionToAddCohortsOriginal = addRecordsSet,
            PermissionToRecruit = recruitApprenticesSet,
            PermissionToRecruitOriginal = recruitApprenticesSet
        };

        var sut = new ChangePermissionsSubmitViewModelValidator();
        var result = sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(c => c.PermissionToAddCohorts)
            .WithErrorMessage(ChangePermissionsSubmitViewModelValidator.NotChangedPermissionsErrorMessage);
    }

}
