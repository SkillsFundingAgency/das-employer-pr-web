namespace SFA.DAS.Employer.PR.Web.Models;

public class ChangePermissionsViewModel : ChangePermissionsSubmitViewModel, IBackLink
{
    public string BackLink { get; set; }
    public string CancelLink { get; set; }




    public ChangePermissionsViewModel(long legalEntityId, string legalName, string providerName, long ukprn, string backLink, string cancelLink)
    {
        LegalEntityId = legalEntityId;
        LegalName = legalName;
        ProviderName = providerName;
        Ukprn = ukprn;
        BackLink = backLink;
        CancelLink = cancelLink;
    }
}

public class ChangePermissionsSubmitViewModel
{
    public string AddRecords { get; set; } = null!;
    public string RecruitApprentices { get; set; } = null!;

    public string AddRecordsOriginal { get; set; } = null!;
    public string RecruitApprenticesOriginal { get; set; } = null!;
    public long LegalEntityId { get; set; }
    public long Ukprn { get; set; }
    public string LegalName { get; set; } = null!;
    public string ProviderName { get; set; } = null!;
}