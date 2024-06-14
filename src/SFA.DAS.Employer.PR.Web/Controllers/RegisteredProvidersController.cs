using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Services;

namespace SFA.DAS.Employer.PR.Web.Controllers;

[Authorize]
[Route("registeredProviders", Name = RouteNames.GetRegisteredProviders)]
public class RegisteredProvidersController(IOuterApiClient outerApiClient, ICacheStorageService cacheStorageService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> GetRegisteredProviders([FromQuery] string query, CancellationToken cancellationToken)
    {
        query = query.Trim();
        if (query.Length < 3) return Ok(new List<RegisteredProvider>());

        List<RegisteredProvider> providers = (await GetRegisteredProviders(cancellationToken))!;
        var matchedProviders = providers
            .Where(provider => provider.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                               || provider.Ukprn.ToString().Contains(query, StringComparison.OrdinalIgnoreCase));

        return Ok(matchedProviders.Take(100));
    }

    private async Task<List<RegisteredProvider>?> GetRegisteredProviders(CancellationToken cancellationToken)
    {
        var providers = await cacheStorageService.RetrieveFromCache<List<RegisteredProvider>>(CacheStorageValues.Providers.Key);

        if (providers != null) return providers;

        var result = await outerApiClient.GetRegisteredProviders(cancellationToken);
        await cacheStorageService.SaveToCache(CacheStorageValues.Providers.Key, result.Providers.OrderBy(p => p.Name).ToList(), CacheStorageValues.Providers.HoursToCache);

        return result.Providers;
    }
}
