using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Infrastructure.Services;
using SFA.DAS.Employer.PR.Web.Models;
using SFA.DAS.Employer.PR.Web.Models.Session;
using SFA.DAS.Employer.PR.Web.Services;

namespace SFA.DAS.Employer.PR.Web.Controllers.Requests;


[Route("requests")]
public class DeclineCreateAccountConfirmationController(ISessionService _sessionService, IAccountsLinkService accountsLinkService) : Controller
{
    [Authorize]
    [HttpGet]
    [Route("{requestId:guid}/createaccount/decline/confirmed", Name = RouteNames.DeclineCreateAccountConfirmation)]
    public IActionResult Index(Guid requestId, CancellationToken cancellationToken)
    {
        Request.HttpContext.Items.Add(SessionKeys.AccountTasksKey, true);
        var sessionModel = _sessionService.Get<AccountCreationSessionModel>();
        if (string.IsNullOrEmpty(sessionModel?.ProviderName))
        {
            return RedirectToRoute(RouteNames.CreateAccountCheckDetails, new { requestId });
        }

        DeclineCreateAccountConfirmationViewModel model = new()
        {
            ProviderName = sessionModel!.ProviderName!.ToUpper(),
            HelpLink = accountsLinkService.GetAccountsLink(EmployerAccountRoutes.Help)
        };

        _sessionService.Delete<AccountCreationSessionModel>();

        return View(model);
    }
}