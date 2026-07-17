# CacheKeyBuilder

`CacheKeyBuilder` is a centralized utility that generates consistent, collision-resistant string keys for caching operations across the application. It encapsulates all key patterns for user, product, order, review, session, rate limiting, and distributed locking scenarios, ensuring that cache access remains uniform and maintainable. The class exposes only static members and is designed to be used without instantiation.

## API

### `UserById`
```csharp
public static string UserById(int userId)
```
Generates a cache key for a user identified by their unique integer ID.  
**Returns** a formatted string key.  
**Throws** `ArgumentException` if `userId` is less than or equal to zero.

### `UserByEmail`
```csharp
public static string UserByEmail(string email)
```
Generates a cache key for a user lookup by email address.  
**Returns** a normalized string key incorporating the email.  
**Throws** `ArgumentNullException` if `email` is null, or `ArgumentException` if `email` is empty or whitespace.

### `UserSession`
```csharp
public static string UserSession(int userId, string sessionId)
```
Generates a cache key for a specific user session.  
**Returns** a composite key that uniquely identifies the session for the given user.  
**Throws** `ArgumentException` if `userId` is less than or equal to zero, or if `sessionId` is null, empty, or whitespace.

### `ProductById`
```csharp
public static string ProductById(int productId)
```
Generates a cache key for a product identified by its integer ID.  
**Returns** a formatted string key.  
**Throws** `ArgumentException` if `productId` is less than or equal to zero.

### `ProductCategory`
```csharp
public static string ProductCategory(int categoryId)
```
Generates a cache key for a product category lookup.  
**Returns** a string key scoped to the given category ID.  
**Throws** `ArgumentException` if `categoryId` is less than or equal to zero.

### `ProductSearch`
```csharp
public static string ProductSearch(string searchTerm, int page, int pageSize)
```
Generates a cache key for a paginated product search query.  
**Returns** a deterministic key that encodes the search term, page number, and page size.  
**Throws** `ArgumentNullException` if `searchTerm` is null; `ArgumentException` if `searchTerm` is empty or whitespace, or if `page` or `pageSize` are less than or equal to zero.

### `OrderById`
```csharp
public static string OrderById(int orderId)
```
Generates a cache key for an order identified by its integer ID.  
**Returns** a formatted string key.  
**Throws** `ArgumentException` if `orderId` is less than or equal to zero.

### `OrdersByUserId`
```csharp
public static string OrdersByUserId(int userId)
```
Generates a cache key for the collection of orders belonging to a specific user.  
**Returns** a string key scoped to the user.  
**Throws** `ArgumentException` if `userId` is less than or equal to zero.

### `OrdersByStatus`
```csharp
public static string OrdersByStatus(string status)
```
Generates a cache key for orders filtered by a status value.  
**Returns** a normalized key incorporating the status string.  
**Throws** `ArgumentNullException` if `status` is null; `ArgumentException` if `status` is empty or whitespace.

### `ReviewsByProductId`
```csharp
public static string ReviewsByProductId(int productId)
```
Generates a cache key for reviews associated with a specific product.  
**Returns** a string key scoped to the product ID.  
**Throws** `ArgumentException` if `productId` is less than or equal to zero.

### `ReviewsByUserId`
```csharp
public static string ReviewsByUserId(int userId)
```
Generates a cache key for reviews written by a specific user.  
**Returns** a string key scoped to the user ID.  
**Throws** `ArgumentException` if `userId` is less than or equal to zero.

### `Config`
```csharp
public static string Config(string configKey)
```
Generates a cache key for a configuration value identified by its key name.  
**Returns** a string key for the configuration entry.  
**Throws** `ArgumentNullException` if `configKey` is null; `ArgumentException` if `configKey` is empty or whitespace.

### `RateLimitKey`
```csharp
public static string RateLimitKey(string clientIdentifier, string resource)
```
Generates a cache key used for rate limiting a specific client on a specific resource.  
**Returns** a composite key that uniquely identifies the client-resource pair.  
**Throws** `ArgumentNullException` if either parameter is null; `ArgumentException` if either is empty or whitespace.

### `RequestCount`
```csharp
public static string RequestCount(string clientIdentifier, DateTime windowStart)
```
Generates a cache key for tracking the request count of a client within a time window.  
**Returns** a key that encodes the client identifier and the start of the window, truncated to a consistent granularity.  
**Throws** `ArgumentNullException` if `clientIdentifier` is null; `ArgumentException` if `clientIdentifier` is empty or whitespace.

### `SessionData`
```csharp
public static string SessionData(string sessionId)
```
Generates a cache key for arbitrary session-scoped data.  
**Returns** a string key for the session.  
**Throws** `ArgumentNullException` if `sessionId` is null; `ArgumentException` if `sessionId` is empty or whitespace.

### `LockKey`
```csharp
public static string LockKey(string resourceId)
```
Generates a cache key for a distributed lock on a given resource.  
**Returns** a string key suitable for use with distributed locking mechanisms.  
**Throws** `ArgumentNullException` if `resourceId` is null; `ArgumentException` if `resourceId` is empty or whitespace.

### `ProcessingKey`
```csharp
public static string ProcessingKey(string operationId)
```
Generates a cache key that indicates an operation is currently being processed (idempotency or deduplication marker).  
**Returns** a string key for the in-flight operation.  
**Throws** `ArgumentNullException` if `operationId` is null; `ArgumentException` if `operationId` is empty or whitespace.

### `Pattern`
```csharp
public static string Pattern(string basePattern, params object[] args)
```
Generates a cache key from a composite format pattern and its arguments.  
**Returns** a formatted string key built from the pattern and arguments.  
**Throws** `ArgumentNullException` if `basePattern` is null; `ArgumentException` if `basePattern` is empty or whitespace; `FormatException` if the pattern is invalid for the supplied arguments.

### `ValidateKey`
```csharp
public static void ValidateKey(string key)
```
Validates that a cache key string meets the expected format and length constraints.  
**Returns** nothing; throws on invalid input.  
**Throws** `ArgumentNullException` if `key` is null; `ArgumentException` if `key` is empty, whitespace, exceeds the maximum allowed length, or contains characters reserved by the underlying cache provider.

### `TemporaryKey`
```csharp
public static string TemporaryKey(string purpose)
```
Generates a short-lived, purpose-specific cache key, typically for ephemeral data such as email verification tokens or password reset codes.  
**Returns** a string key that includes the purpose and a random or time-based component to ensure uniqueness.  
**Throws** `ArgumentNullException` if `purpose` is null; `ArgumentException` if `purpose` is empty or whitespace.

## Usage

### Example 1: Retrieving a cached product and falling back to the database
```csharp
public async Task<Product?> GetProductAsync(int productId, IDistributedCache cache, IProductRepository repository)
{
    string key = CacheKeyBuilder.ProductById(productId);
    byte[]? cached = await cache.GetAsync(key);
    if (cached is not null)
    {
        return Deserialize<Product>(cached);
    }

    Product? product = await repository.FindByIdAsync(productId);
    if (product is not null)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
        };
        await cache.SetAsync(key, Serialize(product), options);
    }
    return product;
}
```

### Example 2: Rate limiting a client on a specific API endpoint
```csharp
public async Task<bool> IsRateLimitedAsync(
    string clientIp, string endpoint, IDistributedCache cache, int maxRequests, TimeSpan window)
{
    string countKey = CacheKeyBuilder.RequestCount(clientIp, DateTime.UtcNow);
    string lockKey = CacheKeyBuilder.RateLimitKey(clientIp, endpoint);

    int currentCount = await GetOrCreateCounterAsync(cache, countKey, window);
    if (currentCount >= maxRequests)
    {
        // Mark the client as rate-limited for the remainder of the window
        await cache.SetStringAsync(lockKey, "blocked", new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = DateTime.UtcNow.Add(window)
        });
        return true;
    }
    return false;
}
```

## Notes

- All methods that accept integer IDs enforce a positive-value constraint; zero and negative IDs are treated as invalid and will throw `ArgumentException`.
- String parameters are validated for null, empty, and whitespace. Methods that accept email addresses or other free-text inputs do not perform semantic validation beyond basic presence checks.
- `ValidateKey` is intended as a guard before manually constructed keys are passed to the cache store. It does not automatically sanitize input—only rejects keys that violate length or character-set rules.
- `TemporaryKey` incorporates a uniqueness component (e.g., a GUID fragment or timestamp ticks) to prevent accidental collisions across different purposes or invocations. It is not suitable for long-term caching scenarios.
- All members are static and stateless; the class is inherently thread-safe. No internal mutable state is shared across invocations, and key generation relies only on the supplied arguments and deterministic formatting logic.
- The `Pattern` method uses `string.Format` semantics internally. Callers must ensure the number and types of `args` match the placeholders in `basePattern` to avoid `FormatException`.
- Keys produced by this class are provider-agnostic strings. When using Redis or similar backends, ensure that the configured maximum key length and allowed character set align with the constraints enforced by `ValidateKey`.
