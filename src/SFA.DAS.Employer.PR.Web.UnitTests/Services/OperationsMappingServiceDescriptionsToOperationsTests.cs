using AutoFixture.NUnit3;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Employer.PR.Web.Services;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Services;
public class OperationsMappingServiceDescriptionsToOperationsTests
{
    [Test]
    [InlineAutoData(Operation.CreateCohort, SetPermissions.AddRecords.Yes)]
    [InlineAutoData(null, SetPermissions.AddRecords.No)]
    public void PermissionToAddRecords_FromOperations(Operation? expectedOperation, string permissionText)
    {
        var permissionDescriptions = new PermissionDescriptionsViewModel { PermissionToAddCohorts = permissionText };

        var operations = OperationsMappingService.MapDescriptionsToOperations(permissionDescriptions);

        if (permissionDescriptions.PermissionToAddCohorts == SetPermissions.AddRecords.No)
        {
            operations.Count.Should().Be(0);
        }

        if (permissionDescriptions.PermissionToAddCohorts == SetPermissions.AddRecords.Yes)
        {
            operations.First().Should().Be(expectedOperation);
        }
    }

    [Test]
    [InlineAutoData(Operation.Recruitment, SetPermissions.RecruitApprentices.Yes)]
    [InlineAutoData(Operation.RecruitmentRequiresReview, SetPermissions.RecruitApprentices.YesWithReview)]
    [InlineAutoData(null, SetPermissions.RecruitApprentices.No)]
    public void PermissionToAddRecruit_FromOperations(Operation? expectedOperation, string permissionText)
    {
        var permissionDescriptions = new PermissionDescriptionsViewModel { PermissionToRecruit = permissionText };

        var operations = OperationsMappingService.MapDescriptionsToOperations(permissionDescriptions);

        if (permissionDescriptions.PermissionToRecruit == SetPermissions.RecruitApprentices.No)
        {
            operations.Count.Should().Be(0);
        }

        if (permissionDescriptions.PermissionToRecruit == SetPermissions.RecruitApprentices.Yes)
        {
            operations.First().Should().Be(expectedOperation);
        }
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

        var permissionDescriptions = new PermissionDescriptionsViewModel { PermissionToAddCohorts = addRecord, PermissionToRecruit = addRecruit };

        var operations = OperationsMappingService.MapDescriptionsToOperations(permissionDescriptions);

        if (permissionDescriptions.PermissionToRecruit == SetPermissions.RecruitApprentices.No
            && permissionDescriptions.PermissionToAddCohorts == SetPermissions.AddRecords.No)
        {
            operations.Count.Should().Be(0);
            return;
        }

        if (permissionDescriptions.PermissionToRecruit == SetPermissions.RecruitApprentices.No
            && permissionDescriptions.PermissionToAddCohorts != SetPermissions.AddRecords.No)
        {
            operations.First().Should().Be(addRecordsOperation);
            return;
        }

        if (permissionDescriptions.PermissionToRecruit != SetPermissions.RecruitApprentices.No
            && permissionDescriptions.PermissionToAddCohorts == SetPermissions.AddRecords.No)
        {
            operations.First().Should().Be(addRecruitmentOperation);
            return;
        }

        operations.Count.Should().Be(2);
        operations.Contains(addRecordsOperation!.Value).Should().BeTrue();
        operations.Contains(addRecruitmentOperation!.Value).Should().BeTrue();
    }
}
