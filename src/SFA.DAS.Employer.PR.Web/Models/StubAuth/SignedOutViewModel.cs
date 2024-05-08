namespace SFA.DAS.Employer.PR.Web.Models.StubAuth;

public class SignedOutViewModel
{
    private readonly string _environmentPart;
    private readonly string _domainPart;

    public SignedOutViewModel(string environment)
    {
        _environmentPart = environment.Equals("prd", StringComparison.OrdinalIgnoreCase) ? "manage-apprenticeships" : $"{environment.ToLower()}-eas.apprenticeships";
        _domainPart = environment.Equals("prd", StringComparison.OrdinalIgnoreCase) ? "service" : "education";
    }
    public string ServiceLink => $"https://accounts.{_environmentPart}.{_domainPart}.gov.uk";
}
