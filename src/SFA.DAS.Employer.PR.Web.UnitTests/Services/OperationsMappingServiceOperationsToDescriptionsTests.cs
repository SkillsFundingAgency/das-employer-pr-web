using AutoFixture.NUnit3;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Services;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Services;
public class OperationsMappingServiceOperationsToDescriptionsTests
{
    [Test]
    [InlineAutoData(Operation.CreateCohort, SetPermissions.AddRecords.Yes)]
    [InlineAutoData(null, SetPermissions.AddRecords.No)]
    public void PermissionToAddRecords_FromOperations(Operation? operation, string expectedPermissionText)
    {
        var operationsToCheck = new List<Operation>();
        if (operation != null)
        {
            operationsToCheck.Add(operation.Value);
        }
        var permissions = OperationsMappingService.MapOperationsToDescriptions(operationsToCheck);
        permissions.PermissionToAddCohorts.Should().Be(expectedPermissionText);
        permissions.PermissionToRecruit.Should().Be(SetPermissions.RecruitApprentices.No);
    }

    [Test]
    [InlineAutoData(Operation.Recruitment, SetPermissions.RecruitApprentices.Yes)]
    [InlineAutoData(Operation.RecruitmentRequiresReview, SetPermissions.RecruitApprentices.YesWithReview)]
    [InlineAutoData(null, SetPermissions.RecruitApprentices.No)]
    public void PermissionToAddRecruit_FromOperations(Operation? operation, string expectedPermissionText)
    {
        var operationsToCheck = new List<Operation>();
        if (operation != null)
        {
            operationsToCheck.Add(operation.Value);
        }

        var permissions = OperationsMappingService.MapOperationsToDescriptions(operationsToCheck);
        permissions.PermissionToRecruit.Should().Be(expectedPermissionText);
        permissions.PermissionToAddCohorts.Should().Be(SetPermissions.AddRecords.No);
    }

    [Test]
    [InlineAutoData(null, SetPermissions.AddRecords.No, Operation.Recruitment, SetPermissions.RecruitApprentices.Yes)]
    [InlineAutoData(null, SetPermissions.AddRecords.No, Operation.RecruitmentRequiresReview, SetPermissions.RecruitApprentices.YesWithReview)]
    [InlineAutoData(null, SetPermissions.AddRecords.No, null, SetPermissions.RecruitApprentices.No)]
    [InlineAutoData(Operation.CreateCohort, SetPermissions.AddRecords.Yes, Operation.Recruitment, SetPermissions.RecruitApprentices.Yes)]
    [InlineAutoData(Operation.CreateCohort, SetPermissions.AddRecords.Yes, Operation.RecruitmentRequiresReview, SetPermissions.RecruitApprentices.YesWithReview)]
    [InlineAutoData(Operation.CreateCohort, SetPermissions.AddRecords.Yes, null, SetPermissions.RecruitApprentices.No)]
    public void PermissionsForBothTypes_TwoOperationsChecked(
        Operation? addRecordsOperation,
        string expectedAddRecord,
        Operation? addRecruitmentOperation,
        string expectedRecruitApprentices
        )
    {
        var operationsToCheck = new List<Operation>();
        if (addRecordsOperation != null)
        {
            operationsToCheck.Add(addRecordsOperation.Value);
        }
        if (addRecruitmentOperation != null)
        {
            operationsToCheck.Add(addRecruitmentOperation.Value);
        }

        var permissions = OperationsMappingService.MapOperationsToDescriptions(operationsToCheck);

        permissions.PermissionToAddCohorts.Should().Be(expectedAddRecord);
        permissions.PermissionToRecruit.Should().Be(expectedRecruitApprentices);
    }
}
