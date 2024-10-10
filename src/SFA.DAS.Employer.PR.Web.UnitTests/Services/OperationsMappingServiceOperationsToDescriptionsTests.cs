using AutoFixture.NUnit3;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Services;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Services;
public class OperationsMappingServiceOperationsToDescriptionsTests
{
    [Test]
    [InlineAutoData(Operation.CreateCohort, OperationsMappingService.Yes)]
    [InlineAutoData(null, OperationsMappingService.No)]
    public void PermissionToAddRecords_FromOperations(Operation? operation, string expectedPermissionText)
    {
        var operationsToCheck = new List<Operation>();
        if (operation != null)
        {
            operationsToCheck.Add(operation.Value);
        }
        var permissions = OperationsMappingService.MapOperationsToDescriptions(operationsToCheck);
        permissions.PermissionToAddCohorts.Should().Be(expectedPermissionText);
        permissions.PermissionToRecruit.Should().Be(OperationsMappingService.No);
    }

    [Test]
    [InlineAutoData(Operation.Recruitment, OperationsMappingService.Yes)]
    [InlineAutoData(Operation.RecruitmentRequiresReview, OperationsMappingService.YesWithReview)]
    [InlineAutoData(null, OperationsMappingService.No)]
    public void PermissionToAddRecruit_FromOperations(Operation? operation, string expectedPermissionText)
    {
        var operationsToCheck = new List<Operation>();
        if (operation != null)
        {
            operationsToCheck.Add(operation.Value);
        }

        var permissions = OperationsMappingService.MapOperationsToDescriptions(operationsToCheck);
        permissions.PermissionToRecruit.Should().Be(expectedPermissionText);
        permissions.PermissionToAddCohorts.Should().Be(OperationsMappingService.No);
    }

    [Test]
    [InlineAutoData(null, OperationsMappingService.No, Operation.Recruitment, OperationsMappingService.Yes)]
    [InlineAutoData(null, OperationsMappingService.No, Operation.RecruitmentRequiresReview, OperationsMappingService.YesWithReview)]
    [InlineAutoData(null, OperationsMappingService.No, null, OperationsMappingService.No)]
    [InlineAutoData(Operation.CreateCohort, OperationsMappingService.Yes, Operation.Recruitment, OperationsMappingService.Yes)]
    [InlineAutoData(Operation.CreateCohort, OperationsMappingService.Yes, Operation.RecruitmentRequiresReview, OperationsMappingService.YesWithReview)]
    [InlineAutoData(Operation.CreateCohort, OperationsMappingService.Yes, null, OperationsMappingService.No)]
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
