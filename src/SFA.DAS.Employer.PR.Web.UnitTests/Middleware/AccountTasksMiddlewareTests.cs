using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Infrastructure.Services;
using SFA.DAS.Employer.PR.Web.Middleware;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Middleware;
public class AccountTasksMiddlewareTests
{
    [Test]
    public async Task InvokeAsync_RequestHasAccountTaskQueryParam_SetsAccountTasksKey()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(
            new ClaimsIdentity(new[] { new Claim("name", "test") }, "testAuthType")
        );

        Mock<ISessionService> sessionServiceMock = new();
        sessionServiceMock.Setup(s => s.Contains(SessionKeys.AccountTasksKey)).Returns(false);

        context.Request.QueryString = new QueryString("?AccountTasks=true");

        var requestDelegateMock = new Mock<RequestDelegate>();
        var sut = new AccountTasksMiddleware(requestDelegateMock.Object, sessionServiceMock.Object);

        // Act
        await sut.InvokeAsync(context);

        // Assert
        sessionServiceMock.Verify(s => s.Set(SessionKeys.AccountTasksKey, true.ToString()), Times.Once);
        context.Items.ContainsKey(SessionKeys.AccountTasksKey).Should().BeTrue();
    }

    [Test]
    public async Task InvokeAsync_RequestHasAccountTaskInSession_DoesNotSetAccountTasksKey()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(
            new ClaimsIdentity(new[] { new Claim("name", "test") }, "testAuthType")
        );

        Mock<ISessionService> sessionServiceMock = new();
        sessionServiceMock.Setup(s => s.Contains(SessionKeys.AccountTasksKey)).Returns(true);

        var requestDelegateMock = new Mock<RequestDelegate>();
        var sut = new AccountTasksMiddleware(requestDelegateMock.Object, sessionServiceMock.Object);

        // Act
        await sut.InvokeAsync(context);

        // Assert
        sessionServiceMock.Verify(s => s.Set(SessionKeys.AccountTasksKey, true.ToString()), Times.Never);
        context.Items.ContainsKey(SessionKeys.AccountTasksKey).Should().BeTrue();
    }

    [Test]
    public async Task InvokeAsync_UserNotAuthenticated_DoesNotSetAccountTasksKey()
    {
        // Arrange
        var context = new DefaultHttpContext();

        Mock<ISessionService> sessionServiceMock = new();

        var requestDelegateMock = new Mock<RequestDelegate>();
        var sut = new AccountTasksMiddleware(requestDelegateMock.Object, sessionServiceMock.Object);

        // Act
        await sut.InvokeAsync(context);

        // Assert
        sessionServiceMock.Verify(s => s.Contains(SessionKeys.AccountTasksKey), Times.Never);
        context.Items.ContainsKey(SessionKeys.AccountTasksKey).Should().BeFalse();
    }
}
