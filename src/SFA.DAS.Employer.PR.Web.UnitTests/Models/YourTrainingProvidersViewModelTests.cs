using AutoFixture.NUnit3;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Web.Models;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Models;
public class YourTrainingProvidersViewModelTests
{
    [Test, AutoData]
    public void Operator_ConvertsTo_LegalEntityModel(List<AccountLegalEntity> legalEntities)
    {
        YourTrainingProvidersViewModel sut = legalEntities;
        sut.LegalEntities.Should().BeEquivalentTo(legalEntities, options => options.ExcludingMissingMembers());
    }
}
