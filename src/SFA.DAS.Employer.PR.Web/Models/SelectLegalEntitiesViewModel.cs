namespace SFA.DAS.Employer.PR.Web.Models;

public class SelectLegalEntitiesViewModel : SelectLegalEntitiesSubmitViewModel
{
    public List<AccountLegalEntityViewModel> LegalEntities { get; set; } = new();
    public string BackLink { get; set; }

    public SelectLegalEntitiesViewModel(string cancelUrl, string backLink)
    {
        BackLink = backLink;
    }
}

public class SelectLegalEntitiesSubmitViewModel
{
    public string? LegalEntityPublicHashedId { get; set; }
}
