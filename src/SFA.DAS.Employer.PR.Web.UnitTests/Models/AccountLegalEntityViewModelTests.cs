using AutoFixture.NUnit3;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Models;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Models;

public class AccountLegalEntityViewModelTests
{
    [Test, AutoData]
    public void Operator_ConvertsFrom_AccountLegalEntity(AccountLegalEntity source)
    {
        /// Action
        AccountLegalEntityViewModel sut = source;

        sut.Name.Should().Be(source.AccountLegalEntityName);
        sut.Id.Should().Be(source.AccountLegalEntityPublicHashedId);
    }
}
