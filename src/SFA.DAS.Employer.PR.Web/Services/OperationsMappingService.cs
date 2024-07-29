using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Models;

namespace SFA.DAS.Employer.PR.Web.Services;

public static class OperationsMappingService
{
    public static PermissionDescriptionsModel MapOperationsToDescriptions(List<Operation> operations)
    {
        var permissionToAddCohorts = operations.Exists(o => o == Operation.CreateCohort)
            ? SetPermissions.AddRecords.Yes
            : SetPermissions.AddRecords.No;

        var permissionToRecruit = SetPermissions.RecruitApprentices.No;

        if (operations.Exists(o => o == Operation.Recruitment))
        {
            permissionToRecruit = SetPermissions.RecruitApprentices.Yes;
        }
        else if (operations.Exists(o => o == Operation.RecruitmentRequiresReview))
        {
            permissionToRecruit = SetPermissions.RecruitApprentices.YesWithReview;
        }

        return new PermissionDescriptionsModel { PermissionToAddCohorts = permissionToAddCohorts, PermissionToRecruit = permissionToRecruit };
    }

    public static List<Operation> MapDescriptionsToOperations(PermissionDescriptionsModel permissionDescriptionsModel)
    {
        var operations = new List<Operation>();

        if (permissionDescriptionsModel.PermissionToAddCohorts == SetPermissions.AddRecords.Yes)
        {
            operations.Add(Operation.CreateCohort);
        }

        if (permissionDescriptionsModel.PermissionToRecruit == SetPermissions.RecruitApprentices.Yes)
        {
            operations.Add(Operation.Recruitment);
        }
        else if (permissionDescriptionsModel.PermissionToRecruit == SetPermissions.RecruitApprentices.YesWithReview)
        {
            operations.Add(Operation.RecruitmentRequiresReview);
        }

        return operations;
    }
}
