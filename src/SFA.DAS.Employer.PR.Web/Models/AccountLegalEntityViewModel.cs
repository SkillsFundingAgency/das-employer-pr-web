using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;

namespace SFA.DAS.Employer.PR.Web.Models;

public record AccountLegalEntityViewModel(string Id, string Name)
{
    public bool IsSelected { get; set; }
    public static implicit operator AccountLegalEntityViewModel(AccountLegalEntity source)
        => new(source.AccountLegalEntityPublicHashedId, source.AccountLegalEntityName);
}
