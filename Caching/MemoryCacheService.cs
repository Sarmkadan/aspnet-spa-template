// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace AspNetSpaTemplate.Caching;

/// <summary>
/// In-memory cache service implementation.
/// Suitable for single-server deployments. For distributed setups, use RedisCacheService.
/// Uses distributed lock approach for GetOrSet to prevent cache stampedes.
/// </summary>
public class MemoryCacheService : ICacheService
{
    private readonly ConcurrentDictionary<string, CacheEntry> _cache;
    private readonly ConcurrentDictionary<string, object> _locks;
    private readonly CacheStatistics _stats;
    private readonly ILogger<MemoryCacheService> _logger;

    public MemoryCacheService(ILogger<MemoryCacheService> logger)
    {
        _cache = new ConcurrentDictionary<string, CacheEntry>();
        _locks = new ConcurrentDictionary<string, object>();
        _stats = new CacheStatistics();
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        _stats.TotalRequests++;

        if (_cache.TryGetValue(key, out var entry))
        {
            // Check if expired
            if (entry.ExpiresAt.HasValue && entry.ExpiresAt < DateTime.UtcNow)
            {
                _cache.TryRemove(key, out _);
                _stats.CacheMisses++;
                return null;
            }

            _stats.CacheHits++;
            return entry.Value as T;
        }

        _stats.CacheMisses++;
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
        if (existing != null)
            return existing;

        // Use lock to prevent cache stampede (multiple concurrent factory calls)
        var lockObj = _locks.GetOrAdd(key, _ => new object());
        lock (lockObj)
        {
            // Double-check after acquiring lock
            existing = GetAsync<T>(key).Result;
            if (existing != null)
                return existing;

            // Generate value
            var value = factory().Result;
            if (value != null)
            {
                SetAsync(key, value, ttl).Wait();
            }

            return value;
        }
    }

    public async Task<long> IncrementAsync(string key, long increment = 1)
    {
        if (_cache.TryGetValue(key, out var entry) && entry.Value is long)
        {
            var newValue = (long)entry.Value + increment;
            _cache[key] = new CacheEntry { Value = newValue, ExpiresAt = entry.ExpiresAt };
            return newValue;
        }

        _cache[key] = new CacheEntry { Value = increment, ExpiresAt = null };
        return increment;
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
        _locks.Clear();
        _logger.LogWarning("Cache FLUSH_ALL executed");
    }

    public async Task<CacheStatistics> GetStatisticsAsync()
    {
        return new CacheStatistics
        {
            TotalRequests = _stats.TotalRequests,
            CacheHits = _stats.CacheHits,
            CacheMisses = _stats.CacheMisses,
            ItemCount = _cache.Count
        };
    }

    /// <summary>
    /// Internal cache entry wrapper with expiration metadata.
    /// </summary>
    private class CacheEntry
    {
        public object Value { get; set; } = null!;
        public DateTime? ExpiresAt { get; set; }
    }
}
