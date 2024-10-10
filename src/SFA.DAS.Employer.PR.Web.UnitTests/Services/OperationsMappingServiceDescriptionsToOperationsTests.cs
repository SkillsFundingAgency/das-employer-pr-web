using AutoFixture.NUnit3;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Employer.PR.Web.Services;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Services;
public class OperationsMappingServiceDescriptionsToOperationsTests
{
    [Test]
    [InlineAutoData(Operation.CreateCohort, OperationsMappingService.Yes)]
    [InlineAutoData(null, OperationsMappingService.No)]
    public void PermissionToAddRecords_FromOperations(Operation? expectedOperation, string permissionText)
    {
        var permissionDescriptions = new PermissionDescriptionsModel { PermissionToAddCohorts = permissionText };

        var operations = OperationsMappingService.MapDescriptionsToOperations(permissionDescriptions);

        if (permissionDescriptions.PermissionToAddCohorts == OperationsMappingService.No)
        {
            operations.Count.Should().Be(0);
        }

        if (permissionDescriptions.PermissionToAddCohorts == OperationsMappingService.Yes)
        {
            operations.First().Should().Be(expectedOperation);
        }
    }

    [Test]
    [InlineAutoData(Operation.Recruitment, OperationsMappingService.Yes)]
    [InlineAutoData(Operation.RecruitmentRequiresReview, OperationsMappingService.YesWithReview)]
    [InlineAutoData(null, OperationsMappingService.No)]
    public void PermissionToAddRecruit_FromOperations(Operation? expectedOperation, string permissionText)
    {
        var permissionDescriptions = new PermissionDescriptionsModel { PermissionToRecruit = permissionText };

        var operations = OperationsMappingService.MapDescriptionsToOperations(permissionDescriptions);

        if (permissionDescriptions.PermissionToRecruit == OperationsMappingService.No)
        {
            operations.Count.Should().Be(0);
        }

        if (permissionDescriptions.PermissionToRecruit == OperationsMappingService.Yes)
        {
            operations.First().Should().Be(expectedOperation);
        }
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
        string addRecord,
        Operation? addRecruitmentOperation,
        string addRecruit
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

        var permissionDescriptions = new PermissionDescriptionsModel { PermissionToAddCohorts = addRecord, PermissionToRecruit = addRecruit };

        var operations = OperationsMappingService.MapDescriptionsToOperations(permissionDescriptions);

        if (permissionDescriptions.PermissionToRecruit == OperationsMappingService.No
            && permissionDescriptions.PermissionToAddCohorts == OperationsMappingService.No)
        {
            operations.Count.Should().Be(0);
            return;
        }

        if (permissionDescriptions.PermissionToRecruit == OperationsMappingService.No
            && permissionDescriptions.PermissionToAddCohorts != OperationsMappingService.No)
        {
            operations.First().Should().Be(addRecordsOperation);
            return;
        }

        if (permissionDescriptions.PermissionToRecruit != OperationsMappingService.No
            && permissionDescriptions.PermissionToAddCohorts == OperationsMappingService.No)
        {
            operations.First().Should().Be(addRecruitmentOperation);
            return;
        }

        operations.Count.Should().Be(2);
        operations.Contains(addRecordsOperation!.Value).Should().BeTrue();
        operations.Contains(addRecruitmentOperation!.Value).Should().BeTrue();
    }
}
