using SFA.DAS.Employer.PR.Domain.Interfaces;
using System.Reflection;

namespace SFA.DAS.Employer.PR.Web.Models;

public sealed class ReviewAddAccountRequestViewModel : ReviewAddAccountRequestSubmitViewModel, IReviewRequest
{
    public required string ProviderName { get; set; }
    public string? AddApprenticeRecordsText { get; set; }
    public string? RecruitApprenticesText { get; set; }
    public required string ViewYourTrainingProvidersLink { get; set; }
}

public class ReviewAddAccountRequestSubmitViewModel : PermissionDescriptionsModel
{
    public bool? AcceptAddAccountRequest { get; set; }

    public string AcceptAddAccountRequestYesRadioCheck
    {
        get
        {
            return AcceptAddAccountRequest.HasValue && AcceptAddAccountRequest.Value ? "checked" : "";
        }
    }

    public string AcceptAddAccountRequestNoRadioCheck
    {
        get
        {
            return AcceptAddAccountRequest.HasValue && !AcceptAddAccountRequest.Value ? "checked" : "";
        }
    }
}
