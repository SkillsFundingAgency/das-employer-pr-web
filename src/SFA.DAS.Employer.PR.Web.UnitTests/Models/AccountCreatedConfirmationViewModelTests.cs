using AutoFixture.NUnit4;
using SFA.DAS.Employer.PR.Web.Models.Requests;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Models;

public class AccountCreatedConfirmationViewModelTests
{
    [Test, AutoData]
    public void ProviderNameInUpperCase(AccountCreatedConfirmationViewModel sut) => sut.ProviderNameInUpperCase.Should().Be(sut.ProviderName.ToUpper());
}
