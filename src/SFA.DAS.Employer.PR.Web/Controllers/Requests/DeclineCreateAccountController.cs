using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Models.Requests;

namespace SFA.DAS.Employer.PR.Web.Controllers.Requests;


[Route("requests")]
public class DeclineCreateAccountController(IOuterApiClient _outerApiClient) : Controller
{
    [Authorize]
    [HttpGet]
    [Route("{requestId:guid}/createaccount/decline", Name = RouteNames.DeclineCreateAccount)]
    public async Task<IActionResult> Index(Guid requestId, CancellationToken cancellationToken)
    {
        Request.HttpContext.Items.Add(SessionKeys.AccountTasksKey, true);
        GetPermissionRequestResponse permissionRequest = await _outerApiClient.GetPermissionRequest(requestId, cancellationToken);

        var backLink = Url.RouteUrl(RouteNames.CreateAccountCheckDetails, new { requestId });

        var vm = new DeclineCreateAccountViewModel
        {
            ProviderName = permissionRequest.ProviderName.ToUpper(),
            BackLink = backLink!
        };

        return View(vm);
    }
}
