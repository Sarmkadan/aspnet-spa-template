// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.Utilities;

/// <summary>
/// Helper for accessing and managing request context information.
/// Provides consistent access to correlation IDs, user info, and request metadata.
/// </summary>
public static class RequestContextHelper
{
    /// <summary>
    /// Gets correlation ID from HTTP context.
    /// Creates new ID if not already set.
    /// </summary>
    public static string GetCorrelationId(this HttpContext context)
    {
        const string key = "CorrelationId";

        if (!context.Items.TryGetValue(key, out var id))
        {
            id = Guid.NewGuid().ToString("N")[..16]; // Short ID: 16 chars
            context.Items[key] = id;
        }

        return id.ToString()!;
    }

    /// <summary>
    /// Gets authenticated user ID from context.
    /// Returns 0 if user not authenticated.
    /// </summary>
    public static int GetUserId(this HttpContext context)
    {
        if (context.Items.TryGetValue("UserId", out var userId) && int.TryParse(userId.ToString(), out var id))
            return id;

        return 0;
    }

    /// <summary>
    /// Gets API key/token from request.
    /// Checks Authorization header and query string.
    /// </summary>
    public static string? GetApiToken(this HttpRequest request)
    {
        // Check Authorization header
        var authHeader = request.Headers["Authorization"].ToString();
        if (!string.IsNullOrEmpty(authHeader))
        {
            var parts = authHeader.Split(' ');
            if (parts.Length == 2 && parts[0].Equals("Bearer", StringComparison.OrdinalIgnoreCase))
                return parts[1];
        }

        // Check query string
        if (request.Query.TryGetValue("api_key", out var apiKey))
            return apiKey.ToString();

        return null;
    }

    /// <summary>
    /// Gets client IP address from request.
    /// Handles X-Forwarded-For header for proxied requests.
    /// </summary>
    public static string GetClientIpAddress(this HttpContext context)
    {
        var request = context.Request;

        // Check X-Forwarded-For header (for proxied requests)
        if (request.Headers.TryGetValue("X-Forwarded-For", out var forwarded))
        {
            var ips = forwarded.ToString().Split(',');
            if (ips.Length > 0 && !string.IsNullOrWhiteSpace(ips[0]))
                return ips[0].Trim();
        }

        // Check X-Real-IP header
        if (request.Headers.TryGetValue("X-Real-IP", out var realIp))
            return realIp.ToString();

        // Use direct connection IP
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    /// <summary>
    /// Gets request user agent.
    /// </summary>
    public static string GetUserAgent(this HttpRequest request)
    {
        return request.Headers["User-Agent"].ToString() ?? "unknown";
    }

    /// <summary>
    /// Gets request referer (HTTP Referer header).
    /// </summary>
    public static string? GetReferer(this HttpRequest request)
    {
        var referer = request.Headers["Referer"].ToString();
        return string.IsNullOrEmpty(referer) ? null : referer;
    }

    /// <summary>
    /// Creates request context from HTTP context.
    /// Useful for passing context to other services.
    /// </summary>
    public static RequestContext CreateContext(this HttpContext context)
    {
        return new RequestContext
        {
            CorrelationId = context.GetCorrelationId(),
            UserId = context.GetUserId(),
            ClientIp = context.GetClientIpAddress(),
            UserAgent = context.Request.GetUserAgent(),
            Method = context.Request.Method,
            Path = context.Request.Path.Value ?? "",
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Checks if request is AJAX (XMLHttpRequest).
    /// </summary>
    public static bool IsAjaxRequest(this HttpRequest request)
    {
        return request.Headers["X-Requested-With"].ToString().Equals("XMLHttpRequest", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if request is from a mobile device (simple check).
    /// </summary>
    public static bool IsMobileRequest(this HttpRequest request)
    {
        var userAgent = request.GetUserAgent().ToLowerInvariant();
        return userAgent.Contains("mobile") ||
               userAgent.Contains("android") ||
               userAgent.Contains("iphone") ||
               userAgent.Contains("ipad");
    }

    /// <summary>
    /// Gets requested content type.
    /// </summary>
    public static string GetContentType(this HttpRequest request)
    {
        return request.ContentType ?? "application/json";
    }

    /// <summary>
    /// Checks if request wants JSON response.
    /// </summary>
    public static bool WantsJson(this HttpRequest request)
    {
        var accept = request.Headers["Accept"].ToString().ToLowerInvariant();
        return accept.Contains("application/json") || accept.Contains("*/*");
    }

    /// <summary>
    /// Gets query parameter safely.
    /// </summary>
    public static string? GetQueryParameter(this HttpRequest request, string paramName)
    {
        if (request.Query.TryGetValue(paramName, out var value))
            return value.ToString();
        return null;
    }

    /// <summary>
    /// Gets header value safely.
    /// </summary>
    public static string? GetHeaderValue(this HttpRequest request, string headerName)
    {
        if (request.Headers.TryGetValue(headerName, out var value))
            return value.ToString();
        return null;
    }
}

/// <summary>
/// Request context information.
/// Passed between services and middleware.
/// </summary>
public class RequestContext
{
    public string CorrelationId { get; set; } = "";
    public int UserId { get; set; }
    public string ClientIp { get; set; } = "";
    public string UserAgent { get; set; } = "";
    public string Method { get; set; } = "";
    public string Path { get; set; } = "";
    public DateTime Timestamp { get; set; }

    public override string ToString()
    {
        return $"{Method} {Path} | CorrelationId: {CorrelationId} | UserId: {UserId} | IP: {ClientIp}";
    }
}
