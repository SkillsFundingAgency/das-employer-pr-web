using AutoFixture.NUnit3;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Domain.OuterApi.Permissions;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Models;
public class PostPermissionsCommandTests
{
    [Test, AutoData]
    public void Constructor_BuildsCommand(string userRef, long ukprn, long accountLegalEntityId, List<Operation> operations)
    {
        var sut = new PostPermissionsCommand(userRef, ukprn, accountLegalEntityId, operations);

        sut.UserRef.Should().Be(userRef);
        sut.Ukprn.Should().Be(ukprn);
        sut.AccountLegalEntityId.Should().Be(accountLegalEntityId);
        sut.Operations.Should().BeEquivalentTo(operations);
    }
}
