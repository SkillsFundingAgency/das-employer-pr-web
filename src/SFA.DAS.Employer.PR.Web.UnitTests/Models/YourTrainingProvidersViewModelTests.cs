using AutoFixture.NUnit3;
using SFA.DAS.Employer.PR.Web.Models;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Models;
public class YourTrainingProvidersViewModelTests
{
    [Test, AutoData]
    public void Constructor_Populates_YourTrainingProvidersViewModelLegalEntities(List<LegalEntityModel> legalEntityModels)
    {
        YourTrainingProvidersViewModel sut = new YourTrainingProvidersViewModel(legalEntityModels);
        sut.LegalEntities.Should().BeEquivalentTo(legalEntityModels, options => options.ExcludingMissingMembers());
    }
}
