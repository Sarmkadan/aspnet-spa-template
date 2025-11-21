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
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    // Store request counts per client: key = "ip:timestamp", value = count
    private static readonly Dictionary<string, (int Count, DateTime ResetTime)> RequestLog = new();
    private static readonly object RequestLogLock = new();

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

        if (!IsRateLimitExceeded(clientId))
        {
            // Add rate limit headers to response
            context.Response.OnStarting(() =>
            {
                var remaining = GetRemainingRequests(clientId);
                context.Response.Headers.Add("X-RateLimit-Limit", RequestsPerMinute.ToString());
                context.Response.Headers.Add("X-RateLimit-Remaining", remaining.ToString());
                context.Response.Headers.Add("X-RateLimit-Reset", GetResetTime(clientId).ToString("O"));
                return Task.CompletedTask;
            });

            await _next(context);
        }
        else
        {
            _logger.LogWarning($"Rate limit exceeded for client {clientId}");
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers.Add("Retry-After", "60");
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
            var token = auth.ToString().Replace("Bearer ", "");
            if (!string.IsNullOrEmpty(token))
                return $"key:{token}";
        }

        // Fall back to client IP
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip:{ip}";
    }

    /// <summary>
    /// Checks if client has exceeded rate limit.
    /// Returns true if limit exceeded (request should be rejected).
    /// </summary>
    private static bool IsRateLimitExceeded(string clientId)
    {
        lock (RequestLogLock)
        {
            var now = DateTime.UtcNow;
            var minuteAgo = now.AddMinutes(-1);

            // Clean old entries
            var keysToRemove = RequestLog
                .Where(x => x.Value.ResetTime < now)
                .Select(x => x.Key)
                .ToList();

            foreach (var key in keysToRemove)
                RequestLog.Remove(key);

            // Get or create entry for this client
            var key1MinuteBucket = $"{clientId}:minute";
            if (!RequestLog.TryGetValue(key1MinuteBucket, out var entry))
            {
                RequestLog[key1MinuteBucket] = (1, now.AddMinutes(1));
                return false;
            }

            // Check if exceeded
            if (entry.Count >= RequestsPerMinute)
                return true;

            // Update count
            RequestLog[key1MinuteBucket] = (entry.Count + 1, entry.ResetTime);
            return false;
        }
    }

    /// <summary>
    /// Gets number of remaining requests for client before hitting limit.
    /// </summary>
    private static int GetRemainingRequests(string clientId)
    {
        lock (RequestLogLock)
        {
            var key = $"{clientId}:minute";
            if (RequestLog.TryGetValue(key, out var entry))
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
            if (RequestLog.TryGetValue(key, out var entry))
                return entry.ResetTime;

            return DateTime.UtcNow.AddMinutes(1);
        }
    }
}

/// <summary>
/// Configuration for rate limiting behavior.
/// Can be extended to support per-endpoint and per-user limits.
/// </summary>
public class RateLimitConfig
{
    public int RequestsPerMinute { get; set; } = 60;
    public int RequestsPerHour { get; set; } = 1000;
    public List<string> ExemptPaths { get; set; } = new();
    public bool EnableByIpAddress { get; set; } = true;
    public bool EnableByApiKey { get; set; } = true;
}
