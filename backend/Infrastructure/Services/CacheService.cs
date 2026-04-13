using Microsoft.Extensions.Caching.Distributed;
using Application.Interfaces;

namespace Infrastructure.Services;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;

    public CacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<string?> GetAsync(string key)
    {
        return await _cache.GetStringAsync(key);
    }

    public async Task SetAsync(string key, string value, TimeSpan expiry)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry
        };

        await _cache.SetStringAsync(key, value, options);
    }
}