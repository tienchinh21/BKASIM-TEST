namespace MiniAppGIBA.Base.Dependencies.Cache
{
    public interface ICacheService
    {
        Task<T?> GetValueAsync<T>(string key);
        Task<string?> GetValueAsync(string key);

        Task SetValueAsync(string key, string value);
        Task SetValueAsync(string key, string value, DateTime expiryDateTime);

        Task RefreshValueAsync(string key, string value);
        Task SetValueAsync(string key, string value, TimeSpan? slidingExpiration = null, DateTime? absoluteExpiration = null);
        Task SetValueAsync(string key, object value, TimeSpan? slidingExpiration = null, DateTime? absoluteExpiration = null);

        Task DeleteValueAsync(string key);

        IEnumerable<KeyValuePair<string, string>> GetAllKeyValues();
        IEnumerable<KeyValuePair<string, string>> GetAllPrefixValue(string prefix);
    }
}
