using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MyCache;

public sealed class MemoryCacheService : ICacheService
{
    private static readonly ConcurrentDictionary<string, byte> KeyRegistry = new();

    private readonly IMemoryCache memoryCache;
    private readonly CacheOptions options;
    private readonly ILogger<MemoryCacheService> logger;

    public MemoryCacheService(
        IMemoryCache memoryCache,
        IOptions<CacheOptions> options,
        ILogger<MemoryCacheService> logger)
    {
        this.memoryCache = memoryCache;
        this.options = options.Value;
        this.logger = logger;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        memoryCache.TryGetValue(BuildKey(key), out T? value);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        var fullKey = BuildKey(key);
        memoryCache.Set(fullKey, value, expiry ?? DefaultExpiry());
        KeyRegistry[fullKey] = 0;
        return Task.CompletedTask;
    }

    public async Task<T> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiry = null,
        CancellationToken cancellationToken = default)
    {
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var value = await factory();
        await SetAsync(key, value, expiry, cancellationToken);
        return value;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        var fullKey = BuildKey(key);
        memoryCache.Remove(fullKey);
        KeyRegistry.TryRemove(fullKey, out _);
        logger.LogDebug("Cache removed: {Key}", fullKey);
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        var fullPrefix = BuildKey(prefix);
        var keys = KeyRegistry.Keys.Where(k => k.StartsWith(fullPrefix, StringComparison.Ordinal)).ToList();

        foreach (var key in keys)
        {
            memoryCache.Remove(key);
            KeyRegistry.TryRemove(key, out _);
        }

        logger.LogDebug("Cache removed by prefix: {Prefix}, count={Count}", fullPrefix, keys.Count);
        return Task.CompletedTask;
    }

    private string BuildKey(string key) => $"{options.KeyPrefix}{key}";

    private TimeSpan DefaultExpiry() => TimeSpan.FromMinutes(options.DefaultExpirationMinutes);
}
