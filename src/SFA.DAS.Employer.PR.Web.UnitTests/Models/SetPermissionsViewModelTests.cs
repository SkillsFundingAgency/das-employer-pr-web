using AutoFixture.NUnit3;
using SFA.DAS.Employer.PR.Web.Models;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Models;
public class SetPermissionsViewModelTests
{
    [Test, AutoData]
    public void Constructor_BuildsViewModel(long legalEntityId, string legalName, string providerName, long ukprn, string backLink, string cancelLink)
    {
        var sut = new SetPermissionsViewModel(legalEntityId, legalName, providerName, ukprn, backLink, cancelLink);
        sut.LegalEntityId.Should().Be(legalEntityId);
        sut.LegalName.Should().Be(legalName);
        sut.ProviderName.Should().Be(providerName);
        sut.Ukprn.Should().Be(ukprn);
        sut.BackLink.Should().Be(backLink);
        sut.CancelLink.Should().Be(cancelLink);
    }
}
