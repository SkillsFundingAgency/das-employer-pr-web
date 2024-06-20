namespace SFA.DAS.Employer.PR.Domain.Models;

public class AccountLegalEntity
{
    public long Id { get; set; }
    public string PublicHashedId { get; set; } = null!;

    public string Name { get; set; } = null!;
    public long AccountId { get; set; }
    public List<Permission> Permissions { get; set; } = [];
}
