using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Web.Authentication;
using SFA.DAS.Employer.PR.Web.Infrastructure;

namespace SFA.DAS.Employer.PR.Web.Controllers;

[Authorize(Policy = nameof(PolicyNames.HasEmployerAccount))]
[Route("accounts/{employerAccountId}", Name = RouteNames.Home)]

public class HomeController : Controller
{
    public IActionResult Index([FromRoute] string employerAccountId)
    {
        return View();
    }
}
