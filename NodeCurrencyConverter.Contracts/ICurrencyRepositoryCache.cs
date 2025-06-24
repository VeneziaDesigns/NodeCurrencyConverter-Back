namespace NodeCurrencyConverter.Contracts
{
    public interface ICurrencyRepositoryCache
    {
        T GetCache<T>(string key, T defaultValue);
        List<T> GetCacheList<T>(string key);
        void SetCache<T>(string key, T value, TimeSpan? expiration);
        public void SetCacheList<T>(string key, List<T> list, TimeSpan? expiration);
    }
}
