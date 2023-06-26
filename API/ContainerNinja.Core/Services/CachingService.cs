using ContainerNinja.Contracts.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace ContainerNinja.Core.Services
{
    public class CachingService : ICachingService
    {
        private readonly IMemoryCache _memoryCache;
        private MemoryCacheEntryOptions _memoryCacheEntryOptions;
        private CancellationTokenSource _resetCacheToken;

        public CachingService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
            _memoryCacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24),
                SlidingExpiration = TimeSpan.FromMinutes(60)
            };
            _resetCacheToken = new CancellationTokenSource();
            _memoryCacheEntryOptions.ExpirationTokens.Add(new CancellationChangeToken(_resetCacheToken.Token));
        }

        public T? GetItem<T>(string cacheKey)
        {
            if (_memoryCache.TryGetValue(cacheKey, out T item))
            {
                return item;
            }
            return default;
        }

        public T SetItem<T>(string cacheKey, T item)
        {
            return _memoryCache.Set(cacheKey, item, _memoryCacheEntryOptions);
        }

        public void RemoveItem(string cacheKey)
        {
            _memoryCache.Remove(cacheKey);
        }

        public void Clear()
        {
            _resetCacheToken.Cancel(); // this triggers the CancellationChangeToken to expire every item from cache
            //_memoryCache.Dispose(); // dispose the current cancellation token source and create a new one
            _memoryCacheEntryOptions.ExpirationTokens.Clear();
            _resetCacheToken = new CancellationTokenSource();
            _memoryCacheEntryOptions.ExpirationTokens.Add(new CancellationChangeToken(_resetCacheToken.Token));
        }
    }
}
