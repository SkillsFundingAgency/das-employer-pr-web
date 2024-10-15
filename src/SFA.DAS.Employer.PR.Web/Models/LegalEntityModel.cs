using SFA.DAS.Employer.PR.Domain.Models;

namespace SFA.DAS.Employer.PR.Web.Models;

public class LegalEntityModel
{
    public long LegalEntityId { get; set; }
    public string LegalEntityPublicHashedId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public long AccountId { get; set; }
    public bool IsSelected { get; set; }
    public List<PermissionDetailsModel> PermissionDetails { get; set; } = [];

    public static implicit operator LegalEntityModel(LegalEntity legalEntity)
    {
        var model = new LegalEntityModel
        {
            AccountId = legalEntity.AccountId,
            LegalEntityPublicHashedId = legalEntity.PublicHashedId,
            LegalEntityId = legalEntity.Id,
            Name = legalEntity.Name
        };

        return model;
    }
}