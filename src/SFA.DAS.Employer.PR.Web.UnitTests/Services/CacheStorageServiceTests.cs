using AutoFixture.NUnit3;
using Microsoft.Extensions.Caching.Distributed;
using SFA.DAS.Employer.PR.Web.Services;
using SFA.DAS.Testing.AutoFixture;
using System.Text.Json;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Services;

[TestFixture]
public class CacheStorageServiceTests
{
    [Test, MoqAutoData]
    public async Task SaveToCache_CachingAsExpected(
        string key,
        TestObject objectToCache,
        [Frozen] Mock<IDistributedCache> mockCache,
        CacheStorageService sut)
    {
        var hoursToCache = 1;

        await sut.SaveToCache(key, objectToCache, hoursToCache);

        mockCache.Verify(x =>
                x.SetAsync(
                    key,
                    It.IsAny<byte[]>(),
                    It.Is<DistributedCacheEntryOptions>(c
                        => c.AbsoluteExpirationRelativeToNow!.Value.Minutes == TimeSpan.FromHours(hoursToCache).Minutes),
                    It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test, MoqAutoData]
    public async Task RetrieveFromCache_ReturnsExpectedObject(
        string keyName,
        TestObject test,
        [Frozen] Mock<IDistributedCache> distributedCache,
        CacheStorageService service)
    {
        distributedCache.Setup(x => x.GetAsync(keyName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(test)));

        var item = await service.RetrieveFromCache<TestObject>(keyName);

        item.Should().NotBeNull();
        item.Should().BeEquivalentTo(test);
    }

    [Test, MoqAutoData]
    public async Task RetrieveFromCache_ReturnsNullIfNoCachedItem(
        string keyName,
        [Frozen] Mock<IDistributedCache> distributedCache,
        CacheStorageService service)
    {
        distributedCache.Setup(x => x.GetAsync(keyName, It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[])null!);

        var item = await service.RetrieveFromCache<TestObject>(keyName);

        item.Should().BeNull();
    }

    [Test, MoqAutoData]
    public async Task DeleteFromCache_RemovedIsCalled(
        string keyName,
        [Frozen] Mock<IDistributedCache> distributedCache,
        CacheStorageService service)
    {
        await service.DeleteFromCache(keyName);

        distributedCache.Verify(x => x.RemoveAsync(keyName, It.IsAny<CancellationToken>()), Times.Once);
    }


    public class TestObject
    {
        public string TestValue { get; set; } = String.Empty;
    }
}
