using SFA.DAS.Employer.PR.Domain.Models;

namespace SFA.DAS.Employer.PR.Web.Models;

public class PermissionModel
{
    public const string PermissionToAddRecordsText = "Yes, employer will review records";
    public const string PermissionToRecruitReviewAdvertsText = "Yes, employer will review adverts";
    public const string PermissionToRecruitText = "Yes";
    public const string NoPermissionToAddRecordsText = "No";
    public const string NoPermissionToRecruitText = "No";
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
            PermissionToAddRecords = NoPermissionToAddRecordsText,
            PermissionToRecruitApprentices = NoPermissionToRecruitText,
            ActionLink = "#"
        };

        if (permission.Operations.Exists(x => x == Operation.CreateCohort))
        {
            model.PermissionToAddRecords = PermissionToAddRecordsText;
        }

        if (permission.Operations.Exists(x => x == Operation.RecruitmentRequiresReview))
        {
            model.PermissionToRecruitApprentices = PermissionToRecruitReviewAdvertsText;
        }

        if (permission.Operations.Exists(x => x == Operation.Recruitment))
        {
            model.PermissionToRecruitApprentices = PermissionToRecruitText;
        }

        return model;
    }
}