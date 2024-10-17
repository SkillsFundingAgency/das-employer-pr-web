namespace SFA.DAS.Employer.PR.Web.Models;

public class PermissionDetailsModel
{
    public long Ukprn { get; set; }
    public string ActionLink { get; set; } = null!;
    public string ActionLinkText { get; set; } = null!;
    public string ProviderName { get; set; } = null!;
    public string PermissionToAddRecords { get; set; } = null!;
    public string PermissionToRecruitApprentices { get; set; } = null!;
    public bool HasOutstandingRequest { get; set; } = false;
}
