using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using SFA.DAS.Employer.PR.Web.Infrastructure.DataProtection;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Infrastructure.DataProtection;
public class DataProtectorServiceFactoryTests
{
    private Mock<IDataProtectionProvider> _providerMock = null!;
    private Mock<IDataProtector> _dataProtectorMock = null!;
    private DataProtectorServiceFactory _factory = null!;

    [SetUp]
    public void SetUp()
    {
        _providerMock = new Mock<IDataProtectionProvider>();
        _dataProtectorMock = new Mock<IDataProtector>();
        _providerMock.Setup(p => p.CreateProtector(It.IsAny<string>())).Returns(_dataProtectorMock.Object);
        _factory = new DataProtectorServiceFactory(_providerMock.Object);
    }

    [Test]
    public void Create_ShouldReturnDataProtectorServiceInstance()
    {
        var key = "test-key";
        var service = _factory.Create(key);

        service.Should().NotBeNull();
        service.Should().BeAssignableTo<IDataProtectorService>();
    }

    [Test]
    public void Protect_ShouldReturnEncodedProtectedString()
    {
        var plainText = "test";
        var protectedBytes = System.Text.Encoding.UTF8.GetBytes("protected");
        _dataProtectorMock.Setup(p => p.Protect(It.IsAny<byte[]>())).Returns(protectedBytes);
        var service = _factory.Create("test-key");

        var result = service.Protect(plainText);

        WebEncoders.Base64UrlEncode(protectedBytes).Should().Be(result);
    }

    [Test]
    public void Unprotect_ShouldReturnPlainTextString()
    {
        var cipherText = WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes("protected"));
        var unprotectedBytes = System.Text.Encoding.UTF8.GetBytes("test");
        _dataProtectorMock.Setup(p => p.Unprotect(It.IsAny<byte[]>())).Returns(unprotectedBytes);
        var service = _factory.Create("test-key");

        var result = service.Unprotect(cipherText);

        System.Text.Encoding.UTF8.GetString(unprotectedBytes).Should().Be(result);
    }

    [Test]
    public void Unprotect_ShouldReturnNull_WhenCipherTextIsNull()
    {
        var service = _factory.Create("test-key");

        var result = service.Unprotect(null);

        result.Should().BeNull();
    }
}
