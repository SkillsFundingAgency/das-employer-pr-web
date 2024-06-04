using AutoFixture.NUnit3;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Web.Authentication;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Testing.AutoFixture;
using System.Text.Json;

namespace SFA.DAS.Employer.PR.Web.UnitTests.AppStart;
public class EmployerOwnerAuthorizationHandlerTests
{
    [Test, MoqAutoData]
    public void EmployerAuthorizedOwner_Succeeded(
     EmployerIdentifier employerIdentifier,
     EmployerOwnerRoleRequirement requirement,
     [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
     EmployerOwnerAuthorizationHandler authorizationHandler)
    {
        var employerIdentifierToUse = new EmployerIdentifier(employerIdentifier.AccountId.ToUpper(), "name", "Owner");

        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifierToUse } };
        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonSerializer.Serialize(employerAccounts));
        var claimsPrinciple = new ClaimsPrincipal(new[] { new ClaimsIdentity(new[] { claim }) });
        var context = new AuthorizationHandlerContext(new[] { requirement }, claimsPrinciple, null);
        var httpContext = new DefaultHttpContext(new FeatureCollection());
        httpContext.Request.RouteValues.Add(RouteValueKeys.EncodedAccountId, employerIdentifier.AccountId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var response = authorizationHandler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Test]
    [MoqInlineAutoData("Transactor")]
    [MoqInlineAutoData("Viewer")]
    public void CheckViewerAndTransactorAgainstOwner_DidNotSucceed(
        string role,
        EmployerIdentifier employerIdentifier,
        EmployerOwnerRoleRequirement requirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerOwnerAuthorizationHandler authorizationHandler)
    {
        var employerIdentifierToUse = new EmployerIdentifier(employerIdentifier.AccountId.ToUpper(), "name", role);
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifierToUse } };
        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonSerializer.Serialize(employerAccounts));
        var claimsPrinciple = new ClaimsPrincipal(new[] { new ClaimsIdentity(new[] { claim }) });
        var context = new AuthorizationHandlerContext(new[] { requirement }, claimsPrinciple, null);
        var httpContext = new DefaultHttpContext(new FeatureCollection());
        httpContext.Request.RouteValues.Add(RouteValueKeys.EncodedAccountId, employerIdentifier.AccountId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var response = authorizationHandler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Test, MoqAutoData]
    public void EmployerNotAuthorised_DifferentAccountInRoute_DidNotSucceed(
        string differentAccountId,
        EmployerIdentifier employerIdentifier,
        EmployerOwnerRoleRequirement requirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerOwnerAuthorizationHandler authorizationHandler)
    {
        var employerIdentifierToUse = new EmployerIdentifier(employerIdentifier.AccountId.ToUpper(), "name", "Owner");
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifierToUse } };
        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonSerializer.Serialize(employerAccounts));
        var claimsPrinciple = new ClaimsPrincipal(new[] { new ClaimsIdentity(new[] { claim }) });
        var context = new AuthorizationHandlerContext(new[] { requirement }, claimsPrinciple, null);
        var responseMock = new FeatureCollection();
        var httpContext = new DefaultHttpContext(responseMock);
        httpContext.Request.RouteValues.Add(RouteValueKeys.EncodedAccountId, differentAccountId.ToUpper());
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var response = authorizationHandler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Test, MoqAutoData]
    public void EmployerAuthorisedButInvalidRole_DidNotSucceed(
        EmployerIdentifier employerIdentifier,
        EmployerOwnerRoleRequirement requirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerOwnerAuthorizationHandler authorizationHandler)
    {

        var employerIdentifierToUse = new EmployerIdentifier(employerIdentifier.AccountId.ToUpper(), "name", "Viewer-Owner-Transactor");
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifierToUse } };
        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonSerializer.Serialize(employerAccounts));
        var claimsPrinciple = new ClaimsPrincipal(new[] { new ClaimsIdentity(new[] { claim }) });
        var context = new AuthorizationHandlerContext(new[] { requirement }, claimsPrinciple, null);
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
        EmployerOwnerRoleRequirement requirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerOwnerAuthorizationHandler authorizationHandler)
    {
        var employerIdentifierToUse = new EmployerIdentifier(employerIdentifier.AccountId.ToUpper(), "name", "Owner");
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifierToUse } };
        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonSerializer.Serialize(employerAccounts));
        var claimsPrinciple = new ClaimsPrincipal(new[] { new ClaimsIdentity(new[] { claim }) });
        var context = new AuthorizationHandlerContext(new[] { requirement }, claimsPrinciple, null);
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
        EmployerOwnerRoleRequirement requirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerOwnerAuthorizationHandler authorizationHandler)
    {
        var employerIdentifierToUse = new EmployerIdentifier(employerIdentifier.AccountId.ToUpper(), "name", "Viewer-Owner-Transactor");
        var employerAccounts = new Dictionary<string, EmployerIdentifier> { { employerIdentifier.AccountId, employerIdentifierToUse } };
        var claim = new Claim("SomeOtherClaim", JsonSerializer.Serialize(employerAccounts));
        var claimsPrinciple = new ClaimsPrincipal(new[] { new ClaimsIdentity(new[] { claim }) });
        var context = new AuthorizationHandlerContext(new[] { requirement }, claimsPrinciple, null);
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
        EmployerOwnerRoleRequirement requirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        EmployerOwnerAuthorizationHandler authorizationHandler)
    {
        var employerIdentifierToUse = new EmployerIdentifier(employerIdentifier.AccountId.ToUpper(), "name", "Owner");

        var claim = new Claim(EmployerClaims.AccountsClaimsTypeIdentifier, JsonSerializer.Serialize(employerIdentifierToUse));
        var claimsPrinciple = new ClaimsPrincipal(new[] { new ClaimsIdentity(new[] { claim }) });
        var context = new AuthorizationHandlerContext(new[] { requirement }, claimsPrinciple, null);
        var responseMock = new FeatureCollection();
        var httpContext = new DefaultHttpContext(responseMock);
        httpContext.Request.RouteValues.Add(RouteValueKeys.EncodedAccountId, employerIdentifier.AccountId);
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var response = authorizationHandler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }
}
