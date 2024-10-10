using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Web.Models;

namespace SFA.DAS.Employer.PR.Web.Services;

public static class OperationsMappingService
{
    public const string Yes = "Yes";
    public const string No = "No";
    public const string YesWithReview = "YesWithReview";

    public static PermissionDescriptionsViewModel MapOperationsToDescriptions(List<Operation> operations)
    public static PermissionDescriptionsModel MapOperationsToDescriptions(List<Operation> operations)
    {
        var permissionToAddCohorts = operations.Exists(o => o == Operation.CreateCohort)
            ? Yes
            : No;

        var permissionToRecruit = No;

        if (operations.Exists(o => o == Operation.Recruitment))
        {
            permissionToRecruit = Yes;
        }
        else if (operations.Exists(o => o == Operation.RecruitmentRequiresReview))
        {
            permissionToRecruit = YesWithReview;
        }

        return new PermissionDescriptionsModel { PermissionToAddCohorts = permissionToAddCohorts, PermissionToRecruit = permissionToRecruit };
    }

    public static List<Operation> MapDescriptionsToOperations(PermissionDescriptionsModel permissionDescriptionsViewModel)
    {
        var operations = new List<Operation>();

        if (permissionDescriptionsViewModel.PermissionToAddCohorts == Yes)
        {
            operations.Add(Operation.CreateCohort);
        }

        if (permissionDescriptionsViewModel.PermissionToRecruit == Yes)
        {
            operations.Add(Operation.Recruitment);
        }
        else if (permissionDescriptionsViewModel.PermissionToRecruit == YesWithReview)
        {
            operations.Add(Operation.RecruitmentRequiresReview);
        }

        return operations;
    }
}
