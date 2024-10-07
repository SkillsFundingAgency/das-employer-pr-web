namespace SFA.DAS.Employer.PR.Domain.OuterApi.Responses;

public class ValidateCreateAccountRequestResponse
{
    public bool IsRequestValid { get; set; }
    public string? Status { get; set; }
    public bool? HasEmployerAccount { get; set; }
    public bool? HasValidaPaye { get; set; }
}
