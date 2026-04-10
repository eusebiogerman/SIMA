using System;
using SIMA.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace SIMA.Infrastructure.Repositories
{
    public class MemoryCacheService : ICacheService
    {
        private static readonly MemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        public T? Get<T>(string key)
            => _cache.TryGetValue(key, out T value) ? value : default;

        public void Set<T>(string key, T value, TimeSpan expiration)
            => _cache.Set(key, value, expiration);

        public void Remove(string key)
            => _cache.Remove(key);
    }

}