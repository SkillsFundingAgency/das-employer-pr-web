namespace SFA.DAS.Employer.PR.Web.Models;

public sealed class ReviewPermissionRequestViewModel : ReviewPermissionRequestSubmitViewModel
{
    public required string ProviderName { get; set; }
    public string? AddApprenticeRecordsText { get; set; }
    public string? RecruitApprenticesText { get; set; }
}

public class ReviewPermissionRequestSubmitViewModel : PermissionDescriptionsViewModel
{
    public bool? AcceptPermissions { get; set; }
}
