using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Interfaces;

namespace SFA.DAS.Employer.PR.Web.Controllers;

[Route("[controller]")]
public class RequestsController(IOuterApiClient _outerApiClient) : Controller
{
    public const string PageNotFoundViewPath = "~/Views/Error/PageNotFound.cshtml";
    [AllowAnonymous]
    [Route("{requestId:guid}")]
    public async Task<IActionResult> ValidateCreateAccountRequest(Guid requestId, CancellationToken cancellationToken)
    {
        var response = await _outerApiClient.ValidateCreateAccountRequest(requestId, cancellationToken);
        if (!response.IsRequestValid) return View(PageNotFoundViewPath);
        return Ok();
    }
}
