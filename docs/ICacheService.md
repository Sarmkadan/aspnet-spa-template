# ICacheService

`ICacheService` is a monitoring interface for cache operations in ASP.NET Core applications, providing metrics about cache hits, misses, item count, and memory usage. It is designed to be implemented by cache providers to expose telemetry data for diagnostics and performance tuning.

## API

### `public long TotalRequests`

Gets the total number of cache request operations (both hits and misses) since the service was initialized.

- **Return value**: A `long` representing the total number of cache requests.
- **Thread safety**: This property is safe to read from multiple threads; however, the underlying counter may be updated concurrently.

---

### `public long CacheHits`

Gets the total number of successful cache lookups (hits) since initialization.

- **Return value**: A `long` representing the number of cache hits.
- **Thread safety**: This property is safe to read from multiple threads; the underlying counter may be updated concurrently.

---

### `public long CacheMisses`

Gets the total number of unsuccessful cache lookups (misses) since initialization.

- **Return value**: A `long` representing the number of cache misses.
- **Thread safety**: This property is safe to read from multiple threads; the underlying counter may be updated concurrently.

---
### `public long ItemCount`

Gets the current number of items stored in the cache.

- **Return value**: A `long` representing the number of cached items.
- **Thread safety**: This property is safe to read from multiple threads; however, the underlying count may be updated concurrently.

---
### `public long ApproximateMemoryUsage`

Gets an approximate measure of the memory consumed by the cache in bytes.

- **Return value**: A `long` representing the approximate memory usage in bytes.
- **Thread safety**: This property is safe to read from multiple threads; the underlying value may be updated concurrently.

---
### `public override string ToString()`

Returns a human-readable summary of the cache statistics, including total requests, hits, misses, item count, and approximate memory usage.

- **Return value**: A `string` formatted as a summary of cache metrics.
- **Thread safety**: This method is safe to call from multiple threads; it reads volatile or atomic state.

## Usage

### Example 1: Logging Cache Statistics Periodically
