// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Utilities;

namespace AspNetSpaTemplate.Middleware;

/// <summary>
/// Middleware that adds correlation ID to all requests and responses.
/// Enables request tracing across multiple services and log aggregation.
/// Each request gets a unique ID that flows through the entire request pipeline.
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    private const string CorrelationIdHeader = "X-Correlation-Id";

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Try to extract correlation ID from request header
        var correlationId = ExtractOrCreateCorrelationId(context);

        // Store in HttpContext for access throughout the request
        context.Items["CorrelationId"] = correlationId;

        // Add to response headers
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId;
            return Task.CompletedTask;
        });

        // Log request start with correlation ID
        _logger.LogInformation(
            "Request started | CorrelationId: {CorrelationId} | Method: {Method} | Path: {Path}",
            correlationId, context.Request.Method, context.Request.Path);

        // Continue pipeline
        await _next(context);

        // Log request completion
        _logger.LogInformation(
            "Request completed | CorrelationId: {CorrelationId} | StatusCode: {StatusCode}",
            correlationId, context.Response.StatusCode);
    }

    /// <summary>
    /// Extracts correlation ID from request header or creates new one.
    /// Allows distributed tracing when correlation ID is passed between services.
    /// </summary>
    private static string ExtractOrCreateCorrelationId(HttpContext context)
    {
        // Check if client provided correlation ID
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationIdHeader))
        {
            var providedId = correlationIdHeader.ToString();
            if (!string.IsNullOrWhiteSpace(providedId))
                return providedId;
        }

        // Generate new correlation ID: timestamp-random
        var timestamp = DateTime.UtcNow.Ticks.ToString("x");
        var random = EncryptionHelper.GenerateRandomString(8);
        return $"{timestamp}-{random}";
    }
}

/// <summary>
/// Helper to access correlation ID from HttpContext.
/// Makes it easy to pass correlation ID to logging and dependent services.
/// </summary>
public static class CorrelationIdExtensions
{
    /// <summary>
    /// Gets the correlation ID from the current HttpContext.
    /// Returns empty string if not found.
    /// </summary>
    public static string GetCorrelationId(this HttpContext context)
    {
        return context?.Items.TryGetValue("CorrelationId", out var id) == true
            ? id?.ToString() ?? string.Empty
            : string.Empty;
    }

    /// <summary>
    /// Sets the correlation ID in the HttpContext.
    /// Useful when passing control between services.
    /// </summary>
    public static void SetCorrelationId(this HttpContext context, string correlationId)
    {
        if (context != null)
            context.Items["CorrelationId"] = correlationId;
    }
}

/// <summary>
/// Structured logging context that includes correlation ID automatically.
/// Simplifies logging by including context without repetition.
/// </summary>
public class CorrelationContext
{
    public string CorrelationId { get; set; } = string.Empty;
    public string ClientIp { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static CorrelationContext FromHttpContext(HttpContext context)
    {
        return new CorrelationContext
        {
            CorrelationId = context.GetCorrelationId(),
            ClientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            UserAgent = context.Request.Headers["User-Agent"].ToString(),
            Timestamp = DateTime.UtcNow
        };
    }
}
