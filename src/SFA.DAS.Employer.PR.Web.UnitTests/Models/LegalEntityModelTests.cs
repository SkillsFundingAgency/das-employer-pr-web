using AutoFixture.NUnit3;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Web.Models;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Models;
public class LegalEntityModelTests
{
    [Test, AutoData]
    public void Operator_ConvertsTo_LegalEntityModel(LegalEntity sut)
    {
        LegalEntityModel model = sut;
        sut.Should().BeEquivalentTo(model, options => options.ExcludingMissingMembers());
    }
}
