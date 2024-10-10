using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Web.Constants;

namespace SFA.DAS.Employer.PR.Web.Models;

public class PermissionModel
{
    public long Ukprn { get; set; }
    public string ActionLink { get; set; } = null!;
    public string ActionLinkText { get; set; } = null!;
    public string ProviderName { get; set; } = null!;
    public string PermissionToAddRecords { get; set; } = null!;
    public string PermissionToRecruitApprentices { get; set; } = null!;

    public static implicit operator PermissionModel(ProviderPermission permission)
    {
        var model = new PermissionModel
        {
            Ukprn = permission.Ukprn,
            ProviderName = permission.ProviderName,
            PermissionToAddRecords = ManageRequests.No,
            PermissionToRecruitApprentices = ManageRequests.No,
            ActionLink = "#"
        };

        if (permission.Operations.Exists(x => x == Operation.CreateCohort))
        {
            model.PermissionToAddRecords = ManageRequests.YesWithEmployerRecordReview;
        }

        if (permission.Operations.Exists(x => x == Operation.RecruitmentRequiresReview))
        {
            model.PermissionToRecruitApprentices = ManageRequests.YesWithEmployerAdvertReview;
        }

        if (permission.Operations.Exists(x => x == Operation.Recruitment))
        {
            model.PermissionToRecruitApprentices = ManageRequests.Yes;
        }

        return model;
    }
}