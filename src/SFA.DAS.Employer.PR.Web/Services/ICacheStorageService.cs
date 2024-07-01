namespace SFA.DAS.Employer.PR.Web.Services;

public interface ICacheStorageService
{
    Task<T> RetrieveFromCache<T>(string key);
    Task SaveToCache<T>(string key, T item, int expirationInHours);
    Task DeleteFromCache(string key);
}