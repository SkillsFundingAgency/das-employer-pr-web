namespace SFA.DAS.Employer.PR.Web.Models;

public class SelectLegalEntitiesViewModel : SelectLegalEntitiesSubmitViewModel, IBackLink
{
    public List<LegalEntityModel> LegalEntities { get; set; } = new();
    public string BackLink { get; set; }

    public SelectLegalEntitiesViewModel(string cancelUrl, string backLink)
    {
        BackLink = backLink;
    }
}

public class SelectLegalEntitiesSubmitViewModel
{
    public long? LegalEntityId { get; set; }
    public string? LegalName { get; set; }
}