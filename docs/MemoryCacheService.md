# MemoryCacheService

A thread-safe, asynchronous wrapper around `IMemoryCache` that provides common cache operations with support for typed values, expiration, atomic increments, and pattern-based key removal. Designed for ASP.NET Core applications to simplify cache interaction while maintaining performance and consistency.

## API

### `MemoryCacheService`
Initializes a new instance of the `MemoryCacheService` with the specified `IMemoryCache` and optional `CacheKeyBuilder`.

- **Parameters**
  - `cache` (`IMemoryCache`): The underlying memory cache instance.
  - `keyBuilder` (`CacheKeyBuilder?`, optional): A builder for generating consistent cache keys. If `null`, keys are used directly.

### `async Task<T?> GetAsync<T>(string key)`
Retrieves a value from the cache by key with the specified type.

- **Parameters**
  - `key` (`string`): The cache key to retrieve.
- **Returns**
  - `T?`: The cached value if found and of type `T`; otherwise, `null`.
- **Exceptions**
  - Throws `ArgumentNullException` if `key` is `null`.

### `async Task SetAsync<T>(string key, T value, TimeSpan? absoluteExpirationRelativeToNow = null)`
Stores a value in the cache with an optional absolute expiration relative to the current time.

- **Parameters**
  - `key` (`string`): The cache key to store under.
  - `value` (`T`): The value to cache.
  - `absoluteExpirationRelativeToNow` (`TimeSpan?`, optional): The duration after which the item expires. If `null`, the item does not expire automatically.
- **Exceptions**
  - Throws `ArgumentNullException` if `key` is `null`.
  - Throws `ArgumentNullException` if `value` is `null` and `T` is a reference type.

### `async Task RemoveAsync(string key)`
Removes a single cache entry by key.

- **Parameters**
  - `key` (`string`): The cache key to remove.
- **Exceptions**
  - Throws `ArgumentNullException` if `key` is `null`.

### `async Task RemoveByPatternAsync(string pattern)`
Removes all cache entries whose keys match the specified pattern using a simple wildcard (`*`) expansion.

- **Parameters**
  - `pattern` (`string`): A pattern with `*` as a wildcard (e.g., `"user:*"`).
- **Exceptions**
  - Throws `ArgumentNullException` if `pattern` is `null`.

### `async Task<bool> ExistsAsync(string key)`
Checks whether a cache entry with the specified key exists.

- **Parameters**
  - `key` (`string`): The cache key to check.
- **Returns**
  - `bool`: `true` if the key exists; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `key` is `null`.

### `async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> valueFactory, TimeSpan? absoluteExpirationRelativeToNow = null)`
Retrieves a value from the cache or computes, stores, and returns it if absent.

- **Parameters**
  - `key` (`string`): The cache key to use.
  - `valueFactory` (`Func<Task<T>>`): A function to compute the value if the key is not found.
  - `absoluteExpirationRelativeToNow` (`TimeSpan?`, optional): The expiration duration for the cached value.
- **Returns**
  - `T`: The cached or newly computed value.
- **Exceptions**
  - Throws `ArgumentNullException` if `key` is `null`.
  - Throws `ArgumentNullException` if `valueFactory` is `null`.

### `async Task<long> IncrementAsync(string key, long defaultValue = 0, long incrementBy = 1)`
Atomically increments a cached numeric value by the specified amount, initializing it if not present.

- **Parameters**
  - `key` (`string`): The cache key for the numeric value.
  - `defaultValue` (`long`, optional): The initial value if the key does not exist.
  - `incrementBy` (`long`, optional): The amount to increment by (can be negative).
- **Returns**
  - `long`: The resulting value after incrementing.
- **Exceptions**
  - Throws `ArgumentNullException` if `key` is `null`.

### `async Task<bool> ExpireAsync(string key, TimeSpan timeToLive)`
Sets the expiration time for a cache entry relative to the current time.

- **Parameters**
  - `key` (`string`): The cache key to expire.
  - `timeToLive` (`TimeSpan`): The duration until the item expires.
- **Returns**
  - `bool`: `true` if the key existed and expiration was set; otherwise, `false`.
- **Exceptions**
  - Throws `ArgumentNullException` if `key` is `null`.
  - Throws `ArgumentOutOfRangeException` if `timeToLive` is negative.

### `async Task<IEnumerable<string>> GetKeysAsync()`
Retrieves all keys currently present in the cache.

- **Returns**
  - `IEnumerable<string>`: An enumerable of all cache keys.

### `async Task FlushAllAsync()`
Removes all entries from the cache.

### `async Task<CacheStatistics> GetStatisticsAsync()`
Retrieves runtime statistics about the cache, including hit/miss counts and memory usage.

- **Returns**
  - `CacheStatistics`: A snapshot of cache statistics.

### `object Value`
Gets the underlying cached value associated with the current instance (used internally for composite operations).

### `DateTime? ExpiresAt`
Gets the absolute expiration date/time of the cached value associated with the current instance, if any.

## Usage
