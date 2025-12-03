using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace MiniAppGIBA.Base.Dependencies.Cache
{
    public class CacheService(IMemoryCache memCache) : ICacheService
    {
        private readonly ConcurrentDictionary<string, string> cacheEntries = new ConcurrentDictionary<string, string>();

        // New method to get all keys in the cache
        public IEnumerable<KeyValuePair<string, string>> GetAllKeyValues()
        {
            return cacheEntries;
        }

        public IEnumerable<KeyValuePair<string, string>> GetAllPrefixValue(string prefix)
        {
            return cacheEntries.Where(entry => entry.Key.StartsWith(prefix)).ToList();
        }

        public Task DeleteValueAsync(string key)
        {
            memCache.Remove(key);
            lock (cacheEntries)
            {
                cacheEntries.TryRemove(key, out _);
            }
            return Task.CompletedTask; // Hoàn thành ngay lập tức
        }

        public async Task<string?> GetValueAsync(string key)
        {
            memCache.TryGetValue(key, out string? value);
            return await Task.FromResult(value);
        }

        public Task<T?> GetValueAsync<T>(string key)
        {
            if (memCache.TryGetValue(key, out string stringValue))
            {
                return Task.FromResult(JsonConvert.DeserializeObject<T>(stringValue));
            }
            return Task.FromResult<T?>(default);
        }

        public Task RefreshValueAsync(string key, string value)
        {
            var options = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(2));
            memCache.Set(key, value, options);
            lock (cacheEntries)
            {
                cacheEntries[key] = value;// Update the cache entry in our manual tracking
            }
            return Task.CompletedTask; // Hoàn thành ngay lập tức
        }

        public Task SetValueAsync(string key, string value)
        {
            var options = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromHours(2));
            memCache.Set(key, value, options);
            lock (cacheEntries)
            {
                cacheEntries[key] = value;  // Add the key-value pair to manual tracking
            }
            return Task.CompletedTask; // Hoàn thành ngay lập tức
        }

        public Task SetValueAsync(string key, string value, TimeSpan? slidingExpiration = null, DateTime? absoluteExpiration = null)
        {
            var options = new MemoryCacheEntryOptions();

            if (slidingExpiration.HasValue)
                options.SetSlidingExpiration(slidingExpiration.Value);

            if (absoluteExpiration.HasValue)
                options.AbsoluteExpiration = absoluteExpiration;

            // Đăng ký PostEvictionCallback để theo dõi khi cache entry bị xóa
            options.RegisterPostEvictionCallback((k, v, reason, state) =>
            {
                try
                {
                    if (reason == EvictionReason.Expired)
                    {
                        Console.WriteLine("\n\n\n Hết hạn nên xoá nha" + k.ToString());
                        cacheEntries.TryRemove(k.ToString(), out _);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });

            memCache.Set(key, value, options);
            cacheEntries[key] = value; // Thêm hoặc cập nhật thủ công
            return Task.CompletedTask;
        }

        public Task SetValueAsync(string key, string value, DateTime expiryDateTime)
        {
            var options = new MemoryCacheEntryOptions { AbsoluteExpiration = expiryDateTime };
            memCache.Set(key, value, options);
            lock (cacheEntries)
            {
                cacheEntries[key] = value;
            }
            return Task.CompletedTask; // Hoàn thành ngay lập tức
        }

        public Task SetValueAsync(string key, object value, TimeSpan? slidingExpiration = null, DateTime? absoluteExpiration = null)
        {
            var options = new MemoryCacheEntryOptions();
            var stringValue = JsonConvert.SerializeObject(value);
            if (slidingExpiration.HasValue)
                options.SetSlidingExpiration(slidingExpiration.Value);

            if (absoluteExpiration.HasValue)
                options.AbsoluteExpiration = absoluteExpiration;

            // Đăng ký PostEvictionCallback để theo dõi khi cache entry bị xóa
            options.RegisterPostEvictionCallback((k, v, reason, state) =>
            {
                try
                {
                    if (reason == EvictionReason.Expired)
                    {
                        Console.WriteLine("\n\n\n Hết hạn nên xoá nha" + k.ToString());
                        cacheEntries.TryRemove(k.ToString(), out _);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
            memCache.Set(key, stringValue, options);
            cacheEntries[key] = stringValue; // Thêm hoặc cập nhật thủ công
            return Task.CompletedTask;
        }
    }
}
