﻿using FluentValidation.TestHelper;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Employer.PR.Web.Services;
using SFA.DAS.Employer.PR.Web.Validators;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Validators;
public class AddPermissionsValidatorTests
{
    [TestCase(OperationsMappingService.Yes, OperationsMappingService.Yes)]
    [TestCase(OperationsMappingService.No, OperationsMappingService.Yes)]
    [TestCase(OperationsMappingService.Yes, OperationsMappingService.YesWithReview)]
    [TestCase(OperationsMappingService.No, OperationsMappingService.YesWithReview)]
    [TestCase(OperationsMappingService.Yes, OperationsMappingService.No)]
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

    [TestCase(OperationsMappingService.Yes)]
    [TestCase(OperationsMappingService.Yes)]
    [TestCase(OperationsMappingService.YesWithReview)]
    [TestCase(OperationsMappingService.YesWithReview)]
    [TestCase(OperationsMappingService.No)]
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

    [TestCase(OperationsMappingService.Yes)]
    [TestCase(OperationsMappingService.No)]
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
            PermissionToAddCohorts = OperationsMappingService.No,
            PermissionToRecruit = OperationsMappingService.No
        };

        var sut = new AddPermissionsValidator();
        var result = sut.TestValidate(model);

        result.ShouldHaveValidationErrorFor(c => c.PermissionToAddCohorts)
            .WithErrorMessage(AddPermissionsValidator.BothSelectionsAreNoErrorMessage);
    }
}
