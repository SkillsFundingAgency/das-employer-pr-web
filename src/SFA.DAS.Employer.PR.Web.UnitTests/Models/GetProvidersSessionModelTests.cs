using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Web.Models.Cache;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Models;
public class GetProvidersSessionModelTests
{
    [Test, MoqAutoData]
    public void Model_HasEmptyProvidersList()
    {
        var sut = new RegisteredProvidersCache();
        sut.RegisteredProviders.Should().BeEquivalentTo(new List<RegisteredProvider>());
    }

    [Test, MoqAutoData]
    public void Model_ProvidersSetUpAsExpected(List<RegisteredProvider> providers)
    {
        var sut = new RegisteredProvidersCache
        {
            RegisteredProviders = providers
        };
        sut.RegisteredProviders.Should().BeEquivalentTo(providers);
    }
}
