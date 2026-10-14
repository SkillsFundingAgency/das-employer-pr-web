using AutoFixture.NUnit4;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Domain.OuterApi.Requests;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Models;

public class PostPermissionsCommandTests
{
    [Test, AutoData]
    public void Constructor_BuildsCommand(Guid userRef, long ukprn, long accountLegalEntityId, List<Operation> operations, string email)
    {
        var sut = new PostPermissionsRequest(userRef, ukprn, accountLegalEntityId, operations, email);

        sut.UserRef.Should().Be(userRef);
        sut.Ukprn.Should().Be(ukprn);
        sut.AccountLegalEntityId.Should().Be(accountLegalEntityId);
        sut.Operations.Should().BeEquivalentTo(operations);
        sut.Email.Should().Be(email);
    }
}
