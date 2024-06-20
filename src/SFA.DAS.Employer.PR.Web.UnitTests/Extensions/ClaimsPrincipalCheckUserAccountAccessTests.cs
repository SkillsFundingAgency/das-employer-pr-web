using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Web.Authentication;
using SFA.DAS.Employer.PR.Web.Extensions;
using SFA.DAS.Testing.AutoFixture;
using System.Text.Json;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Extensions;
public class ClaimsPrincipalCheckUserAccountAccessTests
{
    [Test]
    [MoqInlineAutoData(EmployerUserRole.Owner, true)]
    [MoqInlineAutoData(EmployerUserRole.Transactor, false)]
    [MoqInlineAutoData(EmployerUserRole.Viewer, false)]
    public void CheckUserAccountRole_ReturnTrueIfValid(EmployerUserRole userRoleRequired,
        bool expectedResponse,
        EmployerIdentifier employerIdentifier)
    {
        var employerIdentifierToUse = new EmployerIdentifier(employerIdentifier.AccountId.ToUpper(), "name", userRoleRequired.ToString());
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifierToUse } };
        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonSerializer.Serialize(employerAccounts));
        var claimsPrincipal = new ClaimsPrincipal(new List<ClaimsIdentity> { new(new[] { claim }) });

        var result = claimsPrincipal.IsOwner(employerIdentifier.AccountId);

        result.Should().Be(expectedResponse);
    }

    [Test, MoqAutoData]
    public void CheckUserAccountAccess_NoEmployerAccount_ReturnFalse(
        string employerAccountId)
    {
        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonSerializer.Serialize(new object()));
        var claimsPrincipal = new ClaimsPrincipal(new List<ClaimsIdentity> { new(new[] { claim }) });

        var actual = claimsPrincipal.IsOwner(employerAccountId);

        actual.Should().BeFalse();
    }

    [Test, MoqAutoData]
    public void CheckUserAccountAccess_NoAccountClaims_ReturnsFalse(
        string employerAccountId)
    {
        var employerAccounts = new Dictionary<string, EmployerIdentifier>();
        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonSerializer.Serialize(employerAccounts));
        var claimsPrincipal = new ClaimsPrincipal(new List<ClaimsIdentity> { new(new[] { claim }) });

        var actual = claimsPrincipal.IsOwner(employerAccountId);

        actual.Should().BeFalse();
    }

    [Test, MoqAutoData]
    public void CheckUserAccountAccess_NoAccountClaimsMatching_ReturnsFalse(
        string employerAccountId,
        EmployerIdentifier employerIdentifier)
    {
        var employerIdentifierToUse = new EmployerIdentifier(employerIdentifier.AccountId.ToUpper(), "name", "Owner");
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifierToUse } };
        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonSerializer.Serialize(employerAccounts));
        var claimsPrincipal = new ClaimsPrincipal(new List<ClaimsIdentity> { new(new[] { claim }) });

        var actual = claimsPrincipal.IsOwner(employerAccountId);

        actual.Should().BeFalse();
    }

    [Test, MoqAutoData]
    public void CheckUserAccountAccess_IncorrectRoleType_ReturnsFalse(
        string employerAccountId,
        EmployerIdentifier employerIdentifier)
    {
        var employerIdentifierToUse = new EmployerIdentifier(employerIdentifier.AccountId.ToUpper(), "name", "Non-Role");
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifierToUse } };
        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonSerializer.Serialize(employerAccounts));
        var claimsPrincipal = new ClaimsPrincipal(new List<ClaimsIdentity> { new(new[] { claim }) });

        var actual = claimsPrincipal.IsOwner(employerAccountId);

        actual.Should().BeFalse();
    }
}
