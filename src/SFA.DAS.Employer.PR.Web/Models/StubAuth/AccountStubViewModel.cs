using SFA.DAS.Employer.PR.Domain.Models;

namespace SFA.DAS.Employer.PR.Web.Models.StubAuth;

public class AccountStubViewModel
{
    public string Id { get; set; } = null!;
    public string Email { get; set; } = null!;
    public List<EmployerIdentifier> Accounts { get; set; } = [];
    public string ReturnUrl { get; set; } = null!;
}
