using AutoFixture.NUnit3;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Models;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Models;

public class SelectLegalEntitiesViewModelTests
{

    [Test, AutoData]
    public void Operator_ConvertsTo_LegalEntityModel(List<AccountLegalEntity> legalEntities, string backLink, string cancelUrl)
    {
        SelectLegalEntitiesViewModel sut = new(cancelUrl, backLink);

        foreach (var legalEntity in legalEntities)
        {
            sut.LegalEntities.Add(legalEntity);
        }

        sut.BackLink.Should().BeEquivalentTo(backLink);
        sut.LegalEntities.Should().BeEquivalentTo(legalEntities, options => options.ExcludingMissingMembers());
    }
}
