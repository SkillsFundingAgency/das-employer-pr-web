using AutoFixture.NUnit3;
using SFA.DAS.Employer.PR.Web.Authentication;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.PR.Web.UnitTests.AppStart;
public class EmployerAccountAllRoleAuthorizationHandlerTests
{
    [Test, MoqAutoData]
    public async Task Then_Returns_Succeeded_If_Employer_Is_Authorized_For_Any_Role(
        EmployerAccountAllRolesRequirement requirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        [Frozen] Mock<IEmployerAccountAuthorisationHandler> handler,
        EmployerAccountAllRolesAuthorizationHandler sut)
    {
        //Arrange
        var context = new AuthorizationHandlerContext(new[] { requirement }, new ClaimsPrincipal(), null);
        var httpContext = new DefaultHttpContext(new FeatureCollection());
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        handler.Setup(x => x.IsEmployerAuthorised(context, true)).ReturnsAsync(true);

        //Act
        await sut.HandleAsync(context);

        //Assert
        context.HasSucceeded.Should().BeTrue();
    }
    [Test, MoqAutoData]
    public async Task Then_Returns_Failed_If_Employer_Is_Not_Authorized_For_Any_Role(
        EmployerAccountAllRolesRequirement requirement,
        [Frozen] Mock<IHttpContextAccessor> httpContextAccessor,
        [Frozen] Mock<IEmployerAccountAuthorisationHandler> handler,
        EmployerAccountAllRolesAuthorizationHandler sut)
    {
        //Arrange
        var context = new AuthorizationHandlerContext(new[] { requirement }, new ClaimsPrincipal(), null);
        var httpContext = new DefaultHttpContext(new FeatureCollection());
        httpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        handler.Setup(x => x.IsEmployerAuthorised(context, true)).ReturnsAsync(false);

        //Act
        await sut.HandleAsync(context);

        //Assert
        context.HasSucceeded.Should().BeFalse();
    }
}
