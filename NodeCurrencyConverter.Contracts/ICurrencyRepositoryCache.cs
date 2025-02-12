namespace NodeCurrencyConverter.Contracts
{
    public interface ICurrencyRepositoryCache
    {
        T GetCache<T>(string key, T defaultValue);
        List<T> GetCacheList<T>(string key);
        void SetCache<T>(string key, T generic);
        void SetCacheList<T>(string key, List<T> generic);
    }
}
