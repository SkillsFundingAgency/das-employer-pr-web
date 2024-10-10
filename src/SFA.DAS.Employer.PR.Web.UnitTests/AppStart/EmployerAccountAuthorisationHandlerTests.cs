using System.Text.Json;
using AutoFixture.NUnit3;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Web.Authentication;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.PR.Web.UnitTests.AppStart;

public class EmployerAccountAuthorisationHandlerTests
{
    [Test]
    [MoqInlineAutoData(EmployerUserRole.Owner, "Owner", true)]
    [MoqInlineAutoData(EmployerUserRole.Owner, "Transactor", false)]
    [MoqInlineAutoData(EmployerUserRole.Owner, "Viewer", false)]
    [MoqInlineAutoData(EmployerUserRole.Transactor, "Owner", true)]
    [MoqInlineAutoData(EmployerUserRole.Transactor, "Transactor", true)]
    [MoqInlineAutoData(EmployerUserRole.Transactor, "Viewer", false)]
    [MoqInlineAutoData(EmployerUserRole.Viewer, "Owner", true)]
    [MoqInlineAutoData(EmployerUserRole.Viewer, "Transactor", true)]
    [MoqInlineAutoData(EmployerUserRole.Viewer, "Viewer", true)]
    public void Handler_ReturnsResultBasedOnRole(
        EmployerUserRole userRoleRequired,
        string roleInClaim,
        bool expectedResponse,
        EmployerIdentifier employerIdentifier,
        EmployerAccountOwnerRequirement ownerRequirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerAccountAuthorisationHandler sut)
    {
        employerIdentifier.Role = roleInClaim;
        employerIdentifier.AccountId = employerIdentifier.AccountId.ToUpper();
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifier } };
        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonSerializer.Serialize(employerAccounts));
        var claimsPrinciple = new ClaimsPrincipal(new[] { new ClaimsIdentity(new[] { claim }) });

        var httpContext = new DefaultHttpContext(new FeatureCollection());
        httpContext.Request.RouteValues.Add(RouteValueKeys.EncodedAccountId, employerIdentifier.AccountId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        //Act
        var actual = sut.CheckUserAccountAccess(claimsPrinciple, userRoleRequired);

        //Assert
        actual.Should().Be(expectedResponse);
    }

    [Test, MoqAutoData]
    public async Task EmployerNotAuthorised_DidNotSucceed(
        string accountId,
        EmployerIdentifier employerIdentifier,
        EmployerAccountAllRolesRequirement requirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerAccountAuthorisationHandler sut)
    {
        var employerIdentifierToUse = new EmployerIdentifier { AccountId = employerIdentifier.AccountId.ToUpper(), EmployerName = "name", Role = "Owner" };
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifierToUse } };
        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonSerializer.Serialize(employerAccounts));
        var claimsPrincipal = new ClaimsPrincipal(new List<ClaimsIdentity> { new(new[] { claim }) });
        var context = new AuthorizationHandlerContext([requirement], claimsPrincipal, null);
        var responseMock = new FeatureCollection();
        var httpContext = new DefaultHttpContext(responseMock);
        httpContext.Request.RouteValues.Add(RouteValueKeys.EncodedAccountId, accountId.ToUpper());
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var response = await sut.IsEmployerAuthorised(context, false);

        response.Should().BeFalse();
    }

    [Test, MoqAutoData]
    public async Task EmployerAuthorisedButInvalidRole_DidNotSucceed(
        EmployerIdentifier employerIdentifier,
        EmployerAccountAllRolesRequirement requirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerAccountAuthorisationHandler sut)
    {
        var employerIdentifierToUse = new EmployerIdentifier { AccountId = employerIdentifier.AccountId.ToUpper(), EmployerName = "name", Role = "Viewer-Owner-Transactor" };
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifierToUse } };
        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonSerializer.Serialize(employerAccounts));
        var claimsPrincipal = new ClaimsPrincipal(new List<ClaimsIdentity> { new(new[] { claim }) });
        var context = new AuthorizationHandlerContext(new[] { requirement }, claimsPrincipal, null);
        var responseMock = new FeatureCollection();
        var httpContext = new DefaultHttpContext(responseMock);
        httpContext.Request.RouteValues.Add(RouteValueKeys.EncodedAccountId, employerIdentifier.AccountId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var response = await sut.IsEmployerAuthorised(context, false);

        response.Should().BeFalse();
    }

    [Test, MoqAutoData]
    public async Task AccountIdNotInUrl_DidNotSucceed(
        EmployerIdentifier employerIdentifier,
        EmployerAccountAllRolesRequirement requirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerAccountAuthorisationHandler sut)
    {
        var employerIdentifierToUse = new EmployerIdentifier { AccountId = employerIdentifier.AccountId.ToUpper(), EmployerName = "name", Role = "Owner" };
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifierToUse } };

        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonSerializer.Serialize(employerAccounts));
        var claimsPrincipal = new ClaimsPrincipal(new List<ClaimsIdentity> { new(new[] { claim }) });
        var context = new AuthorizationHandlerContext(new[] { requirement }, claimsPrincipal, null);
        var responseMock = new FeatureCollection();
        var httpContext = new DefaultHttpContext(responseMock);
        httpContext.Request.RouteValues.Clear();
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var response = await sut.IsEmployerAuthorised(context, false);

        response.Should().BeFalse();
    }

    [Test, MoqAutoData]
    public async Task NoMatchingAccountIdentifier_DidNotSucceed(
        EmployerIdentifier employerIdentifier,
        EmployerAccountAllRolesRequirement requirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerAccountAuthorisationHandler sut)
    {
        var employerIdentifierToUse = new EmployerIdentifier { AccountId = employerIdentifier.AccountId.ToUpper(), EmployerName = "name", Role = "Viewer-Owner-Transactor" };
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifierToUse } };

        var claim = new Claim("SomeOtherClaim", JsonSerializer.Serialize(employerAccounts));
        var claimsPrincipal = new ClaimsPrincipal(new List<ClaimsIdentity> { new(new[] { claim }) });
        var context = new AuthorizationHandlerContext(new[] { requirement }, claimsPrincipal, null);
        var responseMock = new FeatureCollection();
        var httpContext = new DefaultHttpContext(responseMock);
        httpContext.Request.RouteValues.Add(RouteValueKeys.EncodedAccountId, employerIdentifier.AccountId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var response = await sut.IsEmployerAuthorised(context, false);

        response.Should().BeFalse();
    }

    [Test, MoqAutoData]
    public async Task ClaimCannotBeDeserialized_DidNotSucceed(
        EmployerIdentifier employerIdentifier,
        EmployerAccountAllRolesRequirement requirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerAccountAuthorisationHandler sut)
    {
        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonSerializer.Serialize(employerIdentifier));

        var claimsPrincipal = new ClaimsPrincipal(new List<ClaimsIdentity> { new(new[] { claim }) });
        var context = new AuthorizationHandlerContext(new[] { requirement }, claimsPrincipal, null);
        var responseMock = new FeatureCollection();
        var httpContext = new DefaultHttpContext(responseMock);
        httpContext.Request.RouteValues.Add(RouteValueKeys.EncodedAccountId, employerIdentifier.AccountId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var response = await sut.IsEmployerAuthorised(context, false);

        response.Should().BeFalse();
    }
}
