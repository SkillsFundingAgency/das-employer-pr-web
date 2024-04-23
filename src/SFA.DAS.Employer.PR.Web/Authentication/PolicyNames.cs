namespace SFA.DAS.Employer.PR.Web.Authentication;

//MFCMFC no idea if the 'HasEmployerAccount' should be here
public static class PolicyNames
{
    public static string IsAuthenticated => nameof(IsAuthenticated);
    public static string HasEmployerAccount => nameof(HasEmployerAccount);
}
