#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Threading;

namespace AspNetSpaTemplate.Caching;

/// <summary>
/// In-memory cache service implementation.
/// Suitable for single-server deployments. For distributed setups, use RedisCacheService.
/// Uses distributed lock approach for GetOrSet to prevent cache stampedes.
/// </summary>
public sealed class MemoryCacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores;
    private readonly ILogger<MemoryCacheService> _logger;

    // Counters are updated with Interlocked because GetAsync runs concurrently.
    private long _totalRequests;
    private long _cacheHits;
    private long _cacheMisses;

    public MemoryCacheService(ILogger<MemoryCacheService> logger)
    {
        _cache = new ConcurrentDictionary<string, CacheEntry>();
        _semaphores = new ConcurrentDictionary<string, SemaphoreSlim>();
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        Interlocked.Increment(ref _totalRequests);

        if (_cache.TryGetValue(key, out var entry))
        {
            // Check if expired
            if (entry.ExpiresAt.HasValue && entry.ExpiresAt < DateTime.UtcNow)
            {
                _cache.TryRemove(key, out _);
                Interlocked.Increment(ref _cacheMisses);
                return null;
            }

            Interlocked.Increment(ref _cacheHits);
            return entry.Value as T;
        }

        Interlocked.Increment(ref _cacheMisses);
        return null;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null) where T : class
    {
        var expiresAt = ttl.HasValue ? DateTime.UtcNow.Add(ttl.Value) : (DateTime?)null;
        var entry = new CacheEntry { Value = value, ExpiresAt = expiresAt };

        _cache[key] = entry;
        _logger.LogDebug($"Cache SET: {key} (TTL: {ttl?.TotalSeconds}s)");
    }

    public async Task RemoveAsync(string key)
    {
        if (_cache.TryRemove(key, out _))
        {
            _logger.LogDebug($"Cache REMOVE: {key}");
        }
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        var regex = new Regex("^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$");
        var keysToRemove = _cache.Keys.Where(k => regex.IsMatch(k)).ToList();

        foreach (var key in keysToRemove)
        {
            _cache.TryRemove(key, out _);
        }

        _logger.LogDebug($"Cache REMOVE_PATTERN: {pattern} (removed {keysToRemove.Count} items)");
    }

    public async Task<bool> ExistsAsync(string key)
    {
        if (!_cache.TryGetValue(key, out var entry))
            return false;

        // Check expiration
        if (entry.ExpiresAt.HasValue && entry.ExpiresAt < DateTime.UtcNow)
        {
            _cache.TryRemove(key, out _);
            return false;
        }

        return true;
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? ttl = null) where T : class
    {
        // Fast path: check if exists
        var existing = await GetAsync<T>(key);
        if (existing is not null)
            return existing;

        // Use semaphore to prevent cache stampede (multiple concurrent factory calls)
        var semaphore = _semaphores.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync();
        try
        {
            // Double-check after acquiring semaphore
            existing = await GetAsync<T>(key);
            if (existing is not null)
                return existing;

            // Generate value
            var value = await factory();
            if (value is not null)
            {
                await SetAsync(key, value, ttl);
            }

            return value!;
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task<long> IncrementAsync(string key, long increment = 1)
    {
        // AddOrUpdate makes the read-modify-write atomic; the old TryGetValue/set
        // sequence could lose increments under concurrent callers.
        var entry = _cache.AddOrUpdate(
            key,
            _ => new CacheEntry { Value = increment, ExpiresAt = null },
            (_, existing) => existing.Value is long current
                ? new CacheEntry { Value = current + increment, ExpiresAt = existing.ExpiresAt }
                : new CacheEntry { Value = increment, ExpiresAt = null });

        return (long)entry.Value;
    }

    public async Task<bool> ExpireAsync(string key, TimeSpan ttl)
    {
        if (!_cache.TryGetValue(key, out var entry))
            return false;

        entry.ExpiresAt = DateTime.UtcNow.Add(ttl);
        return true;
    }

    public async Task<IEnumerable<string>> GetKeysAsync(string pattern)
    {
        var regex = new Regex("^" + Regex.Escape(pattern).Replace("\\*", ".*") + "$");
        return _cache.Keys.Where(k => regex.IsMatch(k)).ToList();
    }

    public async Task FlushAllAsync()
    {
        _cache.Clear();
        _semaphores.Clear();
        _logger.LogWarning("Cache FLUSH_ALL executed");
    }

    public async Task<CacheStatistics> GetStatisticsAsync()
    {
        return new CacheStatistics
        {
            TotalRequests = Interlocked.Read(ref _totalRequests),
            CacheHits = Interlocked.Read(ref _cacheHits),
            CacheMisses = Interlocked.Read(ref _cacheMisses),
            ItemCount = _cache.Count
        };
    }

    /// <summary>
    /// Internal cache entry wrapper with expiration metadata.
    /// </summary>
    private sealed class CacheEntry
    {
        public object Value { get; set; } = null!;
        public DateTime? ExpiresAt { get; set; }
    }
}
