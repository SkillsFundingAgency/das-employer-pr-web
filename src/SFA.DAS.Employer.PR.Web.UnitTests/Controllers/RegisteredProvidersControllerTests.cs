using AutoFixture.NUnit3;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Employer.PR.Domain.Interfaces;
using SFA.DAS.Employer.PR.Domain.Models;
using SFA.DAS.Employer.PR.Domain.OuterApi.Responses;
using SFA.DAS.Employer.PR.Web.Controllers;
using SFA.DAS.Employer.PR.Web.Infrastructure;
using SFA.DAS.Employer.PR.Web.Services;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Employer.PR.Web.UnitTests.Controllers;
public class RegisteredProvidersControllerTests
{
    [Test, MoqAutoData]
    public async Task GetRegisteredProviders_Cached_ReturnsProvidersWithoutCallingOuterApi(
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        Mock<ICacheStorageService> cacheStorageServiceMock,
        GetRegisteredProvidersResponse expectedResult,
        CancellationToken cancellationToken)
    {
        string query = expectedResult.Providers.FirstOrDefault()!.Name;

        outerApiMock.Setup(o => o.GetRegisteredProviders(cancellationToken)).ReturnsAsync(expectedResult);

        cacheStorageServiceMock.Setup(g => g.RetrieveFromCache<List<RegisteredProvider>>(CacheStorageValues.Providers.Key)).ReturnsAsync(expectedResult.Providers);

        RegisteredProvidersController sut = new RegisteredProvidersController(outerApiMock.Object, cacheStorageServiceMock.Object);

        var result = await sut.GetRegisteredProviders(query, cancellationToken);
        cacheStorageServiceMock.Verify(g => g.RetrieveFromCache<List<RegisteredProvider>>(CacheStorageValues.Providers.Key), Times.Once());
        cacheStorageServiceMock.Verify(g => g.SaveToCache(It.IsAny<string>(), It.IsAny<List<RegisteredProvider>>(), It.IsAny<int>()), Times.Never());
        outerApiMock.Verify(x => x.GetRegisteredProviders(cancellationToken), Times.Never);

        OkObjectResult? okObjectResult = result.As<OkObjectResult>();
        var returnedProviders = new List<RegisteredProvider>(((IEnumerable<RegisteredProvider>)okObjectResult.Value!)!);

        returnedProviders.First().Should().BeEquivalentTo(expectedResult.Providers.First(x => x.Name == query));
    }

    [Test, MoqAutoData]
    public async Task GetRegisteredProviders_NotCached_ReturnsProvidersCallingOuterApi(
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        Mock<ICacheStorageService> cacheStorageServiceMock,
        GetRegisteredProvidersResponse expectedResult,
        CancellationToken cancellationToken)
    {
        string query = expectedResult.Providers.MinBy(p => p.Name)!.Name;

        outerApiMock.Setup(o => o.GetRegisteredProviders(cancellationToken)).ReturnsAsync(expectedResult);

        cacheStorageServiceMock.Setup(g => g.RetrieveFromCache<List<RegisteredProvider>>(CacheStorageValues.Providers.Key))!.ReturnsAsync((List<RegisteredProvider>)null!);

        RegisteredProvidersController sut = new RegisteredProvidersController(outerApiMock.Object, cacheStorageServiceMock.Object);

        var result = await sut.GetRegisteredProviders(query, cancellationToken);
        cacheStorageServiceMock.Verify(g => g.RetrieveFromCache<List<RegisteredProvider>>(CacheStorageValues.Providers.Key), Times.Once());
        cacheStorageServiceMock.Verify(g => g.SaveToCache(CacheStorageValues.Providers.Key, It.IsAny<List<RegisteredProvider>>(), CacheStorageValues.Providers.HoursToCache), Times.Once());
        outerApiMock.Verify(x => x.GetRegisteredProviders(cancellationToken), Times.Once);

        OkObjectResult? okObjectResult = result.As<OkObjectResult>();
        var returnedProviders = new List<RegisteredProvider>(((IEnumerable<RegisteredProvider>)okObjectResult.Value!)!);

        returnedProviders.First().Should().BeEquivalentTo(expectedResult.Providers.First(x => x.Name == query));
    }

    [Test]
    [MoqInlineAutoData(true)]
    [MoqInlineAutoData(false)]
    [MoqInlineAutoData(null)]
    public async Task GetRegisteredProviders_ReturnsProviders_CaseInsensitive(
        bool? isUpperCase,
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        Mock<ICacheStorageService> cacheStorageServiceMock,
        GetRegisteredProvidersResponse expectedResult,
        CancellationToken cancellationToken)
    {
        string query = expectedResult.Providers.FirstOrDefault()!.Name;

        if (isUpperCase is true)
        {
            query = query.ToUpper();
        }

        if (isUpperCase is false)
        {
            query = query.ToLower();
        }

        outerApiMock.Setup(o => o.GetRegisteredProviders(cancellationToken)).ReturnsAsync(expectedResult);

        cacheStorageServiceMock.Setup(g => g.RetrieveFromCache<List<RegisteredProvider>>(CacheStorageValues.Providers.Key)).ReturnsAsync(expectedResult.Providers);

        RegisteredProvidersController sut = new RegisteredProvidersController(outerApiMock.Object, cacheStorageServiceMock.Object);

        var result = await sut.GetRegisteredProviders(query, cancellationToken);
        cacheStorageServiceMock.Verify(g => g.RetrieveFromCache<List<RegisteredProvider>>(CacheStorageValues.Providers.Key), Times.Once());
        cacheStorageServiceMock.Verify(g => g.SaveToCache(It.IsAny<string>(), It.IsAny<List<RegisteredProvider>>(), It.IsAny<int>()), Times.Never());
        outerApiMock.Verify(x => x.GetRegisteredProviders(cancellationToken), Times.Never);

        OkObjectResult? okObjectResult = result.As<OkObjectResult>();
        var returnedProviders = new List<RegisteredProvider>(((IEnumerable<RegisteredProvider>)okObjectResult.Value!)!);

        returnedProviders.First().Should().BeEquivalentTo(expectedResult.Providers.First(x =>
            string.Equals(x.Name, query, StringComparison.CurrentCultureIgnoreCase)));
    }

    [Test, MoqAutoData]
    public async Task GetRegisteredProviders_NoCache_ReturnsProvidersInNameOrder(
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        Mock<ICacheStorageService> cacheStorageServiceMock,
        GetRegisteredProvidersResponse expectedResult,
        CancellationToken cancellationToken)
    {
        string query = expectedResult.Providers.FirstOrDefault()!.Name;

        outerApiMock.Setup(o => o.GetRegisteredProviders(cancellationToken)).ReturnsAsync(expectedResult);

        cacheStorageServiceMock.Setup(g => g.RetrieveFromCache<List<RegisteredProvider>>(CacheStorageValues.Providers.Key)).ReturnsAsync((List<RegisteredProvider>)null!);

        RegisteredProvidersController sut = new RegisteredProvidersController(outerApiMock.Object, cacheStorageServiceMock.Object);

        var result = await sut.GetRegisteredProviders(query, cancellationToken);
        cacheStorageServiceMock.Verify(g => g.RetrieveFromCache<List<RegisteredProvider>>(CacheStorageValues.Providers.Key), Times.Once());

        var orderedProviders = expectedResult.Providers.OrderBy(p => p.Name).ToList();
        cacheStorageServiceMock.Verify(g => g.SaveToCache(CacheStorageValues.Providers.Key, orderedProviders, CacheStorageValues.Providers.HoursToCache), Times.Once);

        outerApiMock.Verify(x => x.GetRegisteredProviders(cancellationToken), Times.Once);

        OkObjectResult? okObjectResult = result.As<OkObjectResult>();
        var returnedProviders = new List<RegisteredProvider>(((IEnumerable<RegisteredProvider>)okObjectResult.Value!)!);

        returnedProviders.First().Should().BeEquivalentTo(expectedResult.Providers.First(x =>
            string.Equals(x.Name, query, StringComparison.CurrentCultureIgnoreCase)));
    }

    [Test, MoqAutoData]
    public async Task GetRegisteredProviders_ReturnsProvidersUsingUkprn(
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        Mock<ICacheStorageService> cacheStorageServiceMock,
        GetRegisteredProvidersResponse expectedResult,
        CancellationToken cancellationToken)
    {
        expectedResult.Providers.FirstOrDefault()!.Ukprn = 12345678;
        string query = expectedResult.Providers.FirstOrDefault()!.Ukprn.ToString();

        outerApiMock.Setup(o => o.GetRegisteredProviders(cancellationToken)).ReturnsAsync(expectedResult);

        cacheStorageServiceMock.Setup(g => g.RetrieveFromCache<List<RegisteredProvider>>(CacheStorageValues.Providers.Key)).ReturnsAsync(expectedResult.Providers);

        RegisteredProvidersController sut = new RegisteredProvidersController(outerApiMock.Object, cacheStorageServiceMock.Object);

        var result = await sut.GetRegisteredProviders(query, cancellationToken);
        cacheStorageServiceMock.Verify(g => g.RetrieveFromCache<List<RegisteredProvider>>(CacheStorageValues.Providers.Key), Times.Once());
        cacheStorageServiceMock.Verify(g => g.SaveToCache(It.IsAny<string>(), It.IsAny<List<RegisteredProvider>>(), It.IsAny<int>()), Times.Never());
        outerApiMock.Verify(x => x.GetRegisteredProviders(cancellationToken), Times.Never);

        OkObjectResult? okObjectResult = result.As<OkObjectResult>();
        var returnedProviders = new List<RegisteredProvider>(((IEnumerable<RegisteredProvider>)okObjectResult.Value!)!);

        returnedProviders.First().Should().BeEquivalentTo(expectedResult.Providers.First(x =>
           x.Ukprn.ToString() == query));
    }

    [Test, MoqAutoData]
    public async Task GetRegisteredProviders_NotCached_ReturnsProvidersUsingUkprn(
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        Mock<ICacheStorageService> cacheStorageServiceMock,
        GetRegisteredProvidersResponse expectedResult,
        CancellationToken cancellationToken)
    {
        expectedResult.Providers.FirstOrDefault()!.Ukprn = 12345678;
        string query = expectedResult.Providers.FirstOrDefault()!.Ukprn.ToString();

        outerApiMock.Setup(o => o.GetRegisteredProviders(cancellationToken)).ReturnsAsync(expectedResult);

        cacheStorageServiceMock.Setup(g => g.RetrieveFromCache<List<RegisteredProvider>>(CacheStorageValues.Providers.Key))!.ReturnsAsync((List<RegisteredProvider>)null!);

        RegisteredProvidersController sut = new RegisteredProvidersController(outerApiMock.Object, cacheStorageServiceMock.Object);

        var result = await sut.GetRegisteredProviders(query, cancellationToken);
        cacheStorageServiceMock.Verify(g => g.RetrieveFromCache<List<RegisteredProvider>>(CacheStorageValues.Providers.Key), Times.Once());
        cacheStorageServiceMock.Verify(g => g.SaveToCache(It.IsAny<string>(), It.IsAny<List<RegisteredProvider>>(), It.IsAny<int>()), Times.Once);
        outerApiMock.Verify(x => x.GetRegisteredProviders(cancellationToken), Times.Once);

        OkObjectResult? okObjectResult = result.As<OkObjectResult>();
        var returnedProviders = new List<RegisteredProvider>(((IEnumerable<RegisteredProvider>)okObjectResult.Value!)!);

        returnedProviders.First().Should().BeEquivalentTo(expectedResult.Providers.First(x =>
            x.Ukprn.ToString() == query));
    }

    [Test, MoqAutoData]
    public async Task GetRegisteredProviders_ReturnsProviders_LessThan3CharactersLong(
        [Frozen] Mock<IOuterApiClient> outerApiMock,
        Mock<ICacheStorageService> cacheStorageServiceMock,
        GetRegisteredProvidersResponse expectedResult,
        CancellationToken cancellationToken)
    {
        string query = "AB";

        expectedResult.Providers.FirstOrDefault()!.Name = query;

        outerApiMock.Setup(o => o.GetRegisteredProviders(cancellationToken)).ReturnsAsync(expectedResult);

        cacheStorageServiceMock.Setup(g => g.RetrieveFromCache<List<RegisteredProvider>>(CacheStorageValues.Providers.Key)).ReturnsAsync(expectedResult.Providers);

        RegisteredProvidersController sut = new RegisteredProvidersController(outerApiMock.Object, cacheStorageServiceMock.Object);

        var result = await sut.GetRegisteredProviders(query, cancellationToken);
        cacheStorageServiceMock.Verify(g => g.RetrieveFromCache<List<RegisteredProvider>>(CacheStorageValues.Providers.Key), Times.Never());
        cacheStorageServiceMock.Verify(g => g.SaveToCache(It.IsAny<string>(), It.IsAny<List<RegisteredProvider>>(), It.IsAny<int>()), Times.Never());
        outerApiMock.Verify(x => x.GetRegisteredProviders(cancellationToken), Times.Never);

        OkObjectResult? okObjectResult = result.As<OkObjectResult>();
        var returnedProviders = new List<RegisteredProvider>(((IEnumerable<RegisteredProvider>)okObjectResult.Value!)!);

        returnedProviders.Count.Should().Be(0);
    }
}
