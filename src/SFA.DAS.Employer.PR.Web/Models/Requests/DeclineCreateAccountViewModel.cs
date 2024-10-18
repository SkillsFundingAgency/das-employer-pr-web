namespace SFA.DAS.Employer.PR.Web.Models.Requests;

public class DeclineCreateAccountViewModel
{
    public Guid RequestId { get; set; }
    public required string ProviderName { get; set; }
}