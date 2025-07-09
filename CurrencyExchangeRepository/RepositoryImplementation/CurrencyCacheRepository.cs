using Microsoft.Extensions.Caching.Memory;
using NodeCurrencyConverter.Contracts;

namespace NodeCurrencyConverter.Infrastructure.RepositoryImplementation
{
    public class CurrencyCacheRepository : ICurrencyRepositoryCache
    {
        private readonly IMemoryCache _memoryCache;

        public CurrencyCacheRepository(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public T GetCache<T>(string key, T defaultValue)
        {
            var response = _memoryCache.Get(key);

            return response == null ? defaultValue : (T)response;
        }

        public List<T> GetCacheList<T>(string key)
        {
            var response = _memoryCache.Get(key);

            return (List<T>)response;
        }

        public void SetCache<T>(string key, T value, TimeSpan? expiration)
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            _memoryCache.Set(key, value, options);
        }

        public void SetCacheList<T>(string key, List<T> list, TimeSpan? expiration)
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            _memoryCache.Set(key, list, options);
        }
    }
}
