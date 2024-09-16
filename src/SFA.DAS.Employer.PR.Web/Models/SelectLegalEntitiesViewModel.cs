using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;

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

public record AccountLegalEntityViewModel(string Id, string Name)
{
    public bool IsSelected { get; set; }
    public static implicit operator AccountLegalEntityViewModel(AccountLegalEntity source)
        => new(source.AccountLegalEntityPublicHashedId, source.AccountLegalEntityName);
}

public class SelectLegalEntitiesSubmitViewModel
{
    public string? LegalEntityPublicHashedId { get; set; }
}
