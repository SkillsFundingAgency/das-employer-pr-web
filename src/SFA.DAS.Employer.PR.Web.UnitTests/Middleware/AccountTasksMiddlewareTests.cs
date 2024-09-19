using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Middleware;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Middleware;
public class AccountTasksMiddlewareTests
{
    [Test]
    public async Task InvokeAsync_UserAuthenticated_SetsAccountTasksKey()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(
            new ClaimsIdentity(new[] { new Claim("name", "test") }, "testAuthType")
        );
        context.Session = new Mock<ISession>().Object;
        context.Request.QueryString = new QueryString("?AccountTasks=true");
        var sessionMock = Mock.Get(context.Session);
        sessionMock.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]?>.IsAny)).Returns(false);
        sessionMock.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()));

        var requestDelegateMock = new Mock<RequestDelegate>();
        var sut = new AccountTasksMiddleware(requestDelegateMock.Object);

        // Act
        await sut.InvokeAsync(context);

        // Assert
        sessionMock.Verify(s => s.Set(SessionKeys.AccountTasksKey, It.IsAny<byte[]>()), Times.Once);
        context.Items.ContainsKey(SessionKeys.AccountTasksKey).Should().BeTrue();
    }

    [Test]
    public async Task InvokeAsync_UserNotAuthenticated_DoesNotSetAccountTasksKey()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.User = new ClaimsPrincipal(new ClaimsIdentity());
        context.Session = new Mock<ISession>().Object;

        var requestDelegateMock = new Mock<RequestDelegate>();
        var sut = new AccountTasksMiddleware(requestDelegateMock.Object);

        // Act
        await sut.InvokeAsync(context);

        // Assert
        context.Items.ContainsKey(SessionKeys.AccountTasksKey).Should().BeFalse();
    }
}
