using AutoFixture.NUnit3;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Web.Authentication;
using SFA.DAS.Employer.PR.Web.Constants;
using SFA.DAS.Testing.AutoFixture;
using System.Text.Json;

namespace SFA.DAS.Employer.PR.Web.UnitTests.AppStart;

public class EmployerAccountAuthorizationHandlerTests
{
    [Test, MoqAutoData]
    public void EmployerAuthorizedOwner_Succeeded(
        EmployerIdentifier employerIdentifier,
        EmployerAccountRequirement requirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerAccountAuthorizationHandler authorizationHandler)
    {
        var employerIdentifierToUse = new EmployerIdentifier(employerIdentifier.AccountId.ToUpper(), "name", "Owner");
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifierToUse } };
        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonSerializer.Serialize(employerAccounts));
        var claimsPrincipal = new ClaimsPrincipal(new List<ClaimsIdentity> { new(new[] { claim }) });

        var context = new AuthorizationHandlerContext(new[] { requirement }, claimsPrincipal, null);
        var httpContext = new DefaultHttpContext(new FeatureCollection());
        httpContext.Request.RouteValues.Add(RouteValueKeys.EncodedAccountId, employerIdentifier.AccountId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var response = authorizationHandler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Test]
    [MoqInlineAutoData("Transactor")]
    [MoqInlineAutoData("Viewer")]
    public void CheckViewerAndTransactorAgainstRoles_Succeeded(
        string role,
        EmployerIdentifier employerIdentifier,
        EmployerAccountRequirement requirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerAccountAuthorizationHandler authorizationHandler)
    {
        var employerIdentifierToUse = new EmployerIdentifier(employerIdentifier.AccountId.ToUpper(), "name", role);
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifierToUse } };
        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonSerializer.Serialize(employerAccounts));
        var claimsPrincipal = new ClaimsPrincipal(new List<ClaimsIdentity> { new(new[] { claim }) });
        var context = new AuthorizationHandlerContext(new[] { requirement }, claimsPrincipal, null);
        var httpContext = new DefaultHttpContext(new FeatureCollection());
        httpContext.Request.RouteValues.Add(RouteValueKeys.EncodedAccountId, employerIdentifier.AccountId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var response = authorizationHandler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Test, MoqAutoData]
    public void EmployerNotAuthorised_DidNotSucceed(
        string accountId,
        EmployerIdentifier employerIdentifier,
        EmployerAccountRequirement requirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerAccountAuthorizationHandler authorizationHandler)
    {
        var employerIdentifierToUse = new EmployerIdentifier(employerIdentifier.AccountId.ToUpper(), "name", "Owner");
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifierToUse } };
        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonSerializer.Serialize(employerAccounts));
        var claimsPrincipal = new ClaimsPrincipal(new List<ClaimsIdentity> { new(new[] { claim }) });
        var context = new AuthorizationHandlerContext(new[] { requirement }, claimsPrincipal, null);
        var responseMock = new FeatureCollection();
        var httpContext = new DefaultHttpContext(responseMock);
        httpContext.Request.RouteValues.Add(RouteValueKeys.EncodedAccountId, accountId.ToUpper());
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var response = authorizationHandler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Test, MoqAutoData]
    public void EmployerAuthorisedButInvalidRole_DidNotSucceed(
        EmployerIdentifier employerIdentifier,
        EmployerAccountRequirement requirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerAccountAuthorizationHandler authorizationHandler)
    {
        var employerIdentifierToUse = new EmployerIdentifier(employerIdentifier.AccountId.ToUpper(), "name", "Viewer-Owner-Transactor");
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifierToUse } };
        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonSerializer.Serialize(employerAccounts));
        var claimsPrincipal = new ClaimsPrincipal(new List<ClaimsIdentity> { new(new[] { claim }) });
        var context = new AuthorizationHandlerContext(new[] { requirement }, claimsPrincipal, null);
        var responseMock = new FeatureCollection();
        var httpContext = new DefaultHttpContext(responseMock);
        httpContext.Request.RouteValues.Add(RouteValueKeys.EncodedAccountId, employerIdentifier.AccountId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var response = authorizationHandler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Test, MoqAutoData]
    public void AccountIdNotInUrl_DidNotSucceed(
        EmployerIdentifier employerIdentifier,
        EmployerAccountRequirement requirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerAccountAuthorizationHandler authorizationHandler)
    {
        var employerIdentifierToUse = new EmployerIdentifier(employerIdentifier.AccountId.ToUpper(), "name", "Owner");
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifierToUse } };

        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonSerializer.Serialize(employerAccounts));
        var claimsPrincipal = new ClaimsPrincipal(new List<ClaimsIdentity> { new(new[] { claim }) });
        var context = new AuthorizationHandlerContext(new[] { requirement }, claimsPrincipal, null);
        var responseMock = new FeatureCollection();
        var httpContext = new DefaultHttpContext(responseMock);
        httpContext.Request.RouteValues.Clear();
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var response = authorizationHandler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Test, MoqAutoData]
    public void NoMatchingAccountIdentifier_DidNotSucceed(
        EmployerIdentifier employerIdentifier,
        EmployerAccountRequirement requirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerAccountAuthorizationHandler authorizationHandler)
    {
        var employerIdentifierToUse = new EmployerIdentifier(employerIdentifier.AccountId.ToUpper(), "name", "Viewer-Owner-Transactor");
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifierToUse } };

        var claim = new Claim("SomeOtherClaim", JsonSerializer.Serialize(employerAccounts));
        var claimsPrincipal = new ClaimsPrincipal(new List<ClaimsIdentity> { new(new[] { claim }) });
        var context = new AuthorizationHandlerContext(new[] { requirement }, claimsPrincipal, null);
        var responseMock = new FeatureCollection();
        var httpContext = new DefaultHttpContext(responseMock);
        httpContext.Request.RouteValues.Add(RouteValueKeys.EncodedAccountId, employerIdentifier.AccountId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var response = authorizationHandler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Test, MoqAutoData]
    public void ClaimCannotBeDeserialized_DidNotSucceed(
        EmployerIdentifier employerIdentifier,
        EmployerAccountRequirement requirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerAccountAuthorizationHandler authorizationHandler)
    {
        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonSerializer.Serialize(employerIdentifier));
        var employerIdentifierToUse = new EmployerIdentifier(employerIdentifier.AccountId.ToUpper(), "name", "Owner");
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifierToUse } };


        var claimsPrincipal = new ClaimsPrincipal(new List<ClaimsIdentity> { new(new[] { claim }) });
        var context = new AuthorizationHandlerContext(new[] { requirement }, claimsPrincipal, null);
        var responseMock = new FeatureCollection();
        var httpContext = new DefaultHttpContext(responseMock);
        httpContext.Request.RouteValues.Add(RouteValueKeys.EncodedAccountId, employerIdentifier.AccountId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var response = authorizationHandler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }
}