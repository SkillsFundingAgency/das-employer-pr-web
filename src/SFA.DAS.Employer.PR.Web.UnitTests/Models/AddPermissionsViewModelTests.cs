using AutoFixture.NUnit3;
using SFA.DAS.Employer.PR.Web.Models;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Models;
public class AddPermissionsViewModelTests
{
    [Test, AutoData]
    public void Constructor_BuildsViewModel(long legalEntityId, string legalName, string providerName, long ukprn, string backLink, string cancelLink)
    {
        var sut = new AddPermissionsViewModel(legalEntityId, legalName, providerName, ukprn, cancelLink);
        sut.LegalEntityId.Should().Be(legalEntityId);
        sut.LegalName.Should().Be(legalName);
        sut.ProviderName.Should().Be(providerName);
        sut.Ukprn.Should().Be(ukprn);
        sut.CancelLink.Should().Be(cancelLink);
    }
}
