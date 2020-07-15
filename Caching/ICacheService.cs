// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.Caching;

/// <summary>
/// Interface for distributed caching service.
/// Abstracts cache implementation (Redis, memory, etc.) from consumers.
/// Supports TTL, cache invalidation, and bulk operations.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Gets a value from cache by key.
    /// Returns null if key not found or expired.
    /// </summary>
    Task<T?> GetAsync<T>(string key) where T : class;

    /// <summary>
    /// Sets a value in cache with optional TTL.
    /// Overwrites existing value if key already exists.
    /// </summary>
    Task SetAsync<T>(string key, T value, TimeSpan? ttl = null) where T : class;

    /// <summary>
    /// Removes a value from cache by key.
    /// </summary>
    Task RemoveAsync(string key);

    /// <summary>
    /// Removes all values matching a pattern (e.g., "user:*").
    /// Useful for cache invalidation by prefix.
    /// </summary>
    Task RemoveByPatternAsync(string pattern);

    /// <summary>
    /// Checks if a key exists in cache (without retrieving value).
    /// Useful for cache hit/miss decisions.
    /// </summary>
    Task<bool> ExistsAsync(string key);

    /// <summary>
    /// Gets or creates a cached value using factory function.
    /// If key exists, returns cached value. Otherwise, calls factory and caches result.
    /// Atomically ensures only one factory invocation for concurrent requests (distributed lock).
    /// </summary>
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? ttl = null) where T : class;

    /// <summary>
    /// Increments a numeric counter in cache (atomically).
    /// Creates key if it doesn't exist (starts at 0).
    /// </summary>
    Task<long> IncrementAsync(string key, long increment = 1);

    /// <summary>
    /// Sets cache expiration time for existing key.
    /// Returns false if key doesn't exist.
    /// </summary>
    Task<bool> ExpireAsync(string key, TimeSpan ttl);

    /// <summary>
    /// Gets all keys matching pattern (e.g., "user:*").
    /// Warning: Can be slow on large caches. Use pagination in production.
    /// </summary>
    Task<IEnumerable<string>> GetKeysAsync(string pattern);

    /// <summary>
    /// Clears all cached data.
    /// Use with caution - this affects all users.
    /// </summary>
    Task FlushAllAsync();

    /// <summary>
    /// Gets cache statistics (hits, misses, size).
    /// </summary>
    Task<CacheStatistics> GetStatisticsAsync();
}

/// <summary>
/// Cache statistics for monitoring and optimization.
/// </summary>
public class CacheStatistics
{
    public long TotalRequests { get; set; }
    public long CacheHits { get; set; }
    public long CacheMisses { get; set; }
    public double HitRate => TotalRequests > 0 ? (double)CacheHits / TotalRequests : 0;
    public long ItemCount { get; set; }
    public long ApproximateMemoryUsage { get; set; } // in bytes

    public override string ToString()
    {
        return $"CacheStats: {CacheHits}/{TotalRequests} hits ({HitRate:P}), {ItemCount} items, ~{ApproximateMemoryUsage} bytes";
    }
}
