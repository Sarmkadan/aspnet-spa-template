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
    public static string UserById(int userId) => $"user{Separator}id{Separator}{userId}";
    public static string UserByEmail(string email) => $"user{Separator}email{Separator}{email.ToLowerInvariant()}";
    public static string UserSession(string sessionId) => $"user{Separator}session{Separator}{sessionId}";

    public static string ProductById(int productId) => $"product{Separator}id{Separator}{productId}";
    public static string ProductCategory(string category) => $"product{Separator}category{Separator}{category}";
    public static string ProductFeatured => $"product{Separator}featured";
    public static string ProductSearch(string term) => $"product{Separator}search{Separator}{term.ToLowerInvariant()}";

    public static string OrderById(int orderId) => $"order{Separator}id{Separator}{orderId}";
    public static string OrdersByUserId(int userId) => $"order{Separator}user{Separator}{userId}";
    public static string OrdersByStatus(string status) => $"order{Separator}status{Separator}{status}";

    public static string ReviewsByProductId(int productId) => $"review{Separator}product{Separator}{productId}";
    public static string ReviewsByUserId(int userId) => $"review{Separator}user{Separator}{userId}";

    public static string Config(string configKey) => $"config{Separator}{configKey}";
    public static string Settings => "settings";

    // Rate limiting and counters
    public static string RateLimitKey(string clientId) => $"ratelimit{Separator}{clientId}";
    public static string RequestCount(string endpoint) => $"requests{Separator}{endpoint}";

    // Session data
    public static string SessionData(string sessionId, string key) => $"session{Separator}{sessionId}{Separator}{key}";

    // Temporary locks (for distributed coordination)
    public static string LockKey(string resource) => $"lock{Separator}{resource}";
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
        public const string AllProducts = "product:*";
        public const string AllOrders = "order:*";
        public const string AllReviews = "review:*";
        public const string AllUsers = "user:*";
        public const string AllSessions = "user:session:*";
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
