using AutoFixture.NUnit3;
using SFA.DAS.Employer.PR.Web.Models;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Models;

public class SelectTrainingProviderViewModelTests
{
    [Test, AutoData]
    public void Operator_ConvertsTo_SelectTrainingProviderViewModel(string backLink, string? name, string? ukprn)
    {
        SelectTrainingProviderModel sut = new SelectTrainingProviderModel(backLink, name, ukprn);
        sut.BackLink.Should().Be(backLink);
        sut.Name.Should().Be(name);
        sut.Ukprn.Should().Be(ukprn);
    }
}