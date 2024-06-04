namespace SFA.DAS.Employer.PR.Web.Authentication;

public static class PolicyNames
{
    public static string IsAuthenticated => nameof(IsAuthenticated);
    public static string HasEmployerAccount => nameof(HasEmployerAccount);
    public const string HasEmployerOwnerAccount = nameof(HasEmployerOwnerAccount);
}
