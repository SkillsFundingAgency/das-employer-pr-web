using SFA.DAS.GovUK.Auth.Models;

namespace SFA.DAS.Employer.PR.Web.Models.StubAuth;

public class StubAuthenticationViewModel : StubAuthUserDetails
{
    public string ReturnUrl { get; set; } = null!;
}
