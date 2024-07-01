using AutoFixture.NUnit3;
using SFA.DAS.Employer.PR.Web.Models;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Models;
public class SetPermissionsViewModelTests
{
    [Test, AutoData]
    public void Constructor_BuildsViewModel(long legalEntityId, string legalName, string providerName, long ukprn, string backLink, string cancelLink)
    {
        var sut = new SetPermissionsViewModel(legalEntityId, legalName, providerName, ukprn, backLink, cancelLink);
        sut.AdjustPermissionsViewModel.LegalEntityId.Should().Be(legalEntityId);
        sut.AdjustPermissionsViewModel.LegalName.Should().Be(legalName);
        sut.AdjustPermissionsViewModel.ProviderName.Should().Be(providerName);
        sut.AdjustPermissionsViewModel.Ukprn.Should().Be(ukprn);
        sut.BackLink.Should().Be(backLink);
        sut.AdjustPermissionsViewModel.CancelLink.Should().Be(cancelLink);
    }
}
