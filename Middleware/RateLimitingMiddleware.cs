#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.Middleware;

/// <summary>
/// Middleware for rate limiting API requests per client IP or API key.
/// Prevents DOS attacks and resource exhaustion.
/// Uses in-memory sliding window algorithm for simplicity.
/// </summary>
public sealed class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    // Store request counts per client bucket: key = "clientId:window", value = count + window reset time
    private static readonly Dictionary<string, (int Count, DateTime ResetTime)> RequestLog = new Dictionary<string, (int Count, DateTime ResetTime)>(StringComparer.OrdinalIgnoreCase);
    private static readonly object RequestLogLock = new();

    // Expired-bucket cleanup is amortized: scanning the whole dictionary on
    // every request under the lock is O(n) per request and serializes all
    // traffic behind the slowest scan.
    private static DateTime _nextCleanupAt = DateTime.MinValue;
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromSeconds(30);

    // Configuration
    private const int RequestsPerMinute = 60;
    private const int RequestsPerHour = 1000;
    private const int BurstLimit = 10; // Max requests in 1 second

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientId = GetClientIdentifier(context);

        if (!IsRateLimitExceeded(clientId, out var retryAfter))
        {
            // Add rate limit headers to response
            context.Response.OnStarting(() =>
            {
                var remaining = GetRemainingRequests(clientId);
                context.Response.Headers["X-RateLimit-Limit"] = RequestsPerMinute.ToString(System.Globalization.CultureInfo.InvariantCulture);
                context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString(System.Globalization.CultureInfo.InvariantCulture);
                context.Response.Headers["X-RateLimit-Reset"] = GetResetTime(clientId).ToString("O", System.Globalization.CultureInfo.InvariantCulture);
                return Task.CompletedTask;
            });

            _logger.LogDebug("InvokeAsync forwarding to next middleware for {ClientId}", clientId);
            await _next(context);
        }
        else
        {
            _logger.LogWarning("Rate limit exceeded for client {ClientId}", clientId);
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            var retryAfterSeconds = Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds));
            context.Response.Headers["Retry-After"] = retryAfterSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture);
            await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
        }
    }

    /// <summary>
    /// Gets unique identifier for client (IP or API key).
    /// Uses API key if present (more reliable than IP), falls back to IP.
    /// </summary>
    private static string GetClientIdentifier(HttpContext context)
    {
        // Prefer API key as identifier
        if (context.Request.Headers.TryGetValue("Authorization", out var auth))
        {
            var token = auth.ToString().Trim();
            const string bearerPrefix = "Bearer ";
            if (token.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
                token = token[bearerPrefix.Length..].Trim();

            if (!string.IsNullOrEmpty(token))
            {
                var tokenHash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token));
                return $"key:{Convert.ToHexString(tokenHash)[..16]}";
            }
        }

        // Fall back to client IP
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip:{ip}";
    }

    /// <summary>
    /// Checks if client has exceeded rate limit.
    /// Returns true if limit exceeded (request should be rejected).
    /// </summary>
    private static bool IsRateLimitExceeded(string clientId, out TimeSpan retryAfter)
    {
        lock (RequestLogLock)
        {
            var now = DateTime.UtcNow;

            // Amortized cleanup of expired buckets (all clients)
            if (now >= _nextCleanupAt)
            {
                var keysToRemove = RequestLog
                    .Where(x => x.Value.ResetTime < now)
                    .Select(x => x.Key)
                    .ToList();

                foreach (var key in keysToRemove)
                    RequestLog.Remove(key);

                _nextCleanupAt = now.Add(CleanupInterval);
            }

            var secondKey = $"{clientId}:second";
            var minuteKey = $"{clientId}:minute";
            var hourKey = $"{clientId}:hour";

            // Check every window before recording the request so a rejection in
            // one window does not consume quota in any of the other windows.
            if (IsBucketExceeded(secondKey, BurstLimit, now, out retryAfter))
                return true;
            if (IsBucketExceeded(minuteKey, RequestsPerMinute, now, out retryAfter))
                return true;
            if (IsBucketExceeded(hourKey, RequestsPerHour, now, out retryAfter))
                return true;

            IncrementBucket(secondKey, TimeSpan.FromSeconds(1), now);
            IncrementBucket(minuteKey, TimeSpan.FromMinutes(1), now);
            IncrementBucket(hourKey, TimeSpan.FromHours(1), now);

            retryAfter = TimeSpan.Zero;
            return false;
        }
    }

    /// <summary>
    /// Checks one fixed window bucket without consuming a request.
    /// Must be called under <see cref="RequestLogLock"/>.
    /// </summary>
    private static bool IsBucketExceeded(string bucketKey, int limit, DateTime now, out TimeSpan retryAfter)
    {
        if (!RequestLog.TryGetValue(bucketKey, out var entry) || entry.ResetTime < now)
        {
            retryAfter = TimeSpan.Zero;
            return false;
        }

        if (entry.Count >= limit)
        {
            retryAfter = entry.ResetTime - now;
            return true;
        }

        retryAfter = TimeSpan.Zero;
        return false;
    }

    /// <summary>
    /// Counts a request against one fixed window bucket.
    /// Must be called under <see cref="RequestLogLock"/>.
    /// </summary>
    private static void IncrementBucket(string bucketKey, TimeSpan window, DateTime now)
    {
        if (!RequestLog.TryGetValue(bucketKey, out var entry) || entry.ResetTime < now)
            RequestLog[bucketKey] = (1, now.Add(window));
        else
            RequestLog[bucketKey] = (entry.Count + 1, entry.ResetTime);
    }

    /// <summary>
    /// Gets number of remaining requests for client before hitting limit.
    /// </summary>
    private static int GetRemainingRequests(string clientId)
    {
        lock (RequestLogLock)
        {
            var key = $"{clientId}:minute";
            if (RequestLog.TryGetValue(key, out var entry) && entry.ResetTime >= DateTime.UtcNow)
                return Math.Max(0, RequestsPerMinute - entry.Count);

            return RequestsPerMinute;
        }
    }

    /// <summary>
    /// Gets the time when rate limit resets for this client.
    /// </summary>
    private static DateTime GetResetTime(string clientId)
    {
        lock (RequestLogLock)
        {
            var key = $"{clientId}:minute";
            if (RequestLog.TryGetValue(key, out var entry) && entry.ResetTime >= DateTime.UtcNow)
                return entry.ResetTime;

            return DateTime.UtcNow.AddMinutes(1);
        }
    }
}

/// <summary>
/// Configuration for rate limiting behavior.
/// Can be extended to support per-endpoint and per-user limits.
/// </summary>
public sealed class RateLimitConfig
{
    public int RequestsPerMinute { get; set; } = 60;
    public int RequestsPerHour { get; set; } = 1000;
    public List<string> ExemptPaths { get; set; } = new();
    public bool EnableByIpAddress { get; set; } = true;
    public bool EnableByApiKey { get; set; } = true;
}
