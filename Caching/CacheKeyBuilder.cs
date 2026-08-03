#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.Caching;

/// <summary>
/// Builder for consistent cache key generation.
/// Prevents cache key collisions and simplifies cache invalidation.
/// Use this instead of hardcoded strings for cache keys.
/// </summary>
public static class CacheKeyBuilder
{
    private const string Separator = ":";

    // Cache key patterns
    /// <summary>
    /// Gets the cache key for a user by their ID.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <returns>A cache key string for the user.</returns>
    public static string UserById(int userId) => $"user{Separator}id{Separator}{userId}";
    /// <summary>
    /// Gets the cache key for a user by their email.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <returns>A cache key string for the user by email.</returns>
    public static string UserByEmail(string email) => $"user{Separator}email{Separator}{email.ToLowerInvariant()}";
    /// <summary>
    /// Gets the cache key for a user session.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <returns>A cache key string for the user session.</returns>
    public static string UserSession(string sessionId) => $"user{Separator}session{Separator}{sessionId}";

    /// <summary>
    /// Gets the cache key for a product by its ID.
    /// </summary>
    /// <param name="productId">The product's ID.</param>
    /// <returns>A cache key string for the product.</returns>
    public static string ProductById(int productId) => $"product{Separator}id{Separator}{productId}";
    /// <summary>
    /// Gets the cache key for products by category.
    /// </summary>
    /// <param name="category">The product category.</param>
    /// <returns>A cache key string for products by category.</returns>
    public static string ProductCategory(string category) => $"product{Separator}category{Separator}{category}";
    /// <summary>
    /// Gets the cache key for featured products.
    /// </summary>
    /// <returns>A cache key string for featured products.</returns>
    public static string ProductFeatured => $"product{Separator}featured";
    /// <summary>
    /// Gets the cache key for product search by term.
    /// </summary>
    /// <param name="term">The search term.</param>
    /// <returns>A cache key string for product search.</returns>
    public static string ProductSearch(string term) => $"product{Separator}search{Separator}{term.ToLowerInvariant()}";

    /// <summary>
    /// Gets the cache key for an order by its ID.
    /// </summary>
    /// <param name="orderId">The order's ID.</param>
    /// <returns>A cache key string for the order.</returns>
    public static string OrderById(int orderId) => $"order{Separator}id{Separator}{orderId}";
    /// <summary>
    /// Gets the cache key for orders by user ID.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <returns>A cache key string for orders by user.</returns>
    public static string OrdersByUserId(int userId) => $"order{Separator}user{Separator}{userId}";
    /// <summary>
    /// Gets the cache key for orders by status.
    /// </summary>
    /// <param name="status">The order status.</param>
    /// <returns>A cache key string for orders by status.</returns>
    public static string OrdersByStatus(string status) => $"order{Separator}status{Separator}{status}";

    /// <summary>
    /// Gets the cache key for reviews by product ID.
    /// </summary>
    /// <param name="productId">The product's ID.</param>
    /// <returns>A cache key string for reviews by product.</returns>
    public static string ReviewsByProductId(int productId) => $"review{Separator}product{Separator}{productId}";
    /// <summary>
    /// Gets the cache key for reviews by user ID.
    /// </summary>
    /// <param name="userId">The user's ID.</param>
    /// <returns>A cache key string for reviews by user.</returns>
    public static string ReviewsByUserId(int userId) => $"review{Separator}user{Separator}{userId}";

    public static string Config(string configKey) => $"config{Separator}{configKey}";
    /// <summary>
    /// Gets the cache key for application settings.
    /// </summary>
    /// <returns>A cache key string for application settings.</returns>
    public static string Settings => "settings";

    // Rate limiting and counters
    /// <summary>
    /// Gets the cache key for rate limiting by client ID.
    /// </summary>
    /// <param name="clientId">The client identifier.</param>
    /// <returns>A cache key string for rate limiting.</returns>
    public static string RateLimitKey(string clientId) => $"ratelimit{Separator}{clientId}";
    /// <summary>
    /// Gets the cache key for request count by endpoint.
    /// </summary>
    /// <param name="endpoint">The endpoint name.</param>
    /// <returns>A cache key string for request count.</returns>
    public static string RequestCount(string endpoint) => $"requests{Separator}{endpoint}";

    // Session data
    /// <summary>
    /// Gets the cache key for session data.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="key">The session data key.</param>
    /// <returns>A cache key string for session data.</returns>
    public static string SessionData(string sessionId, string key) => $"session{Separator}{sessionId}{Separator}{key}";

    // Temporary locks (for distributed coordination)
    /// <summary>
    /// Gets the cache key for a distributed lock.
    /// </summary>
    /// <param name="resource">The resource to lock.</param>
    /// <returns>A cache key string for the lock.</returns>
    public static string LockKey(string resource) => $"lock{Separator}{resource}";
    /// <summary>
    /// Gets the cache key for a processing job.
    /// </summary>
    /// <param name="jobId">The job identifier.</param>
    /// <returns>A cache key string for the processing job.</returns>
    public static string ProcessingKey(string jobId) => $"processing{Separator}{jobId}";

    /// <summary>
    /// Creates pattern for cache invalidation (e.g., "product:*" for all products).
    /// </summary>
    public static string Pattern(string prefix) => $"{prefix}*";

    /// <summary>
    /// Invalidation patterns for cascading cache clears.
    /// </summary>
    public static class InvalidationPatterns
    {
        /// <summary>
        /// Pattern to invalidate all product-related cache entries.
        /// </summary>
        public const string AllProducts = "product:*";
        /// <summary>
        /// Pattern to invalidate all order-related cache entries.
        /// </summary>
        public const string AllOrders = "order:*";
        /// <summary>
        /// Pattern to invalidate all review-related cache entries.
        /// </summary>
        public const string AllReviews = "review:*";
        /// <summary>
        /// Pattern to invalidate all user-related cache entries.
        /// </summary>
        public const string AllUsers = "user:*";
        /// <summary>
        /// Pattern to invalidate all user session-related cache entries.
        /// </summary>
        public const string AllSessions = "user:session:*";
        /// <summary>
        /// Pattern to invalidate all rate limit-related cache entries.
        /// </summary>
        public const string AllRateLimits = "ratelimit:*";
    }

    /// <summary>
    /// Validates cache key format (no illegal characters).
    /// Redis keys should not contain spaces or certain special characters.
    /// </summary>
    public static void ValidateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Cache key cannot be empty");

        if (key.Length > 1024)
            throw new ArgumentException("Cache key exceeds maximum length of 1024 characters");

        if (key.Contains(' '))
            throw new ArgumentException("Cache key cannot contain spaces");
    }

    /// <summary>
    /// Generates a unique key for temporary operations (with timestamp).
    /// Useful for one-time operations that should auto-expire.
    /// </summary>
    public static string TemporaryKey(string prefix)
    {
        var timestamp = DateTime.UtcNow.Ticks.ToString("x");
        var random = Guid.NewGuid().ToString("N")[..8];
        return $"temp{Separator}{prefix}{Separator}{timestamp}{Separator}{random}";
    }
}

/// <summary>
/// Extension methods for cache operations with type-safe key builders.
/// </summary>
public static class CacheServiceExtensions
{
    /// <summary>
    /// Gets a user from cache by ID.
    /// </summary>
    public static async Task<T?> GetUserAsync<T>(this ICacheService cache, int userId) where T : class
    {
        return await cache.GetAsync<T>(CacheKeyBuilder.UserById(userId));
    }

    /// <summary>
    /// Caches a user by ID.
    /// </summary>
    public static async Task CacheUserAsync<T>(this ICacheService cache, int userId, T user, TimeSpan? ttl = null) where T : class
    {
        await cache.SetAsync(CacheKeyBuilder.UserById(userId), user, ttl);
    }

    /// <summary>
    /// Gets a product from cache by ID.
    /// </summary>
    public static async Task<T?> GetProductAsync<T>(this ICacheService cache, int productId) where T : class
    {
        return await cache.GetAsync<T>(CacheKeyBuilder.ProductById(productId));
    }

    /// <summary>
    /// Caches a product by ID.
    /// </summary>
    public static async Task CacheProductAsync<T>(this ICacheService cache, int productId, T product, TimeSpan? ttl = null) where T : class
    {
        await cache.SetAsync(CacheKeyBuilder.ProductById(productId), product, ttl);
    }

    /// <summary>
    /// Invalidates all cached data for a user (orders, reviews, session, etc.).
    /// </summary>
    public static async Task InvalidateUserCacheAsync(this ICacheService cache, int userId)
    {
        await cache.RemoveAsync(CacheKeyBuilder.UserById(userId));
        await cache.RemoveByPatternAsync(CacheKeyBuilder.Pattern($"order:user:{userId}"));
        await cache.RemoveByPatternAsync(CacheKeyBuilder.Pattern($"review:user:{userId}"));
    }

    /// <summary>
    /// Invalidates all cached product data (listings, searches, featured, etc.).
    /// </summary>
    public static async Task InvalidateProductCacheAsync(this ICacheService cache)
    {
        await cache.RemoveByPatternAsync(CacheKeyBuilder.InvalidationPatterns.AllProducts);
    }
}
