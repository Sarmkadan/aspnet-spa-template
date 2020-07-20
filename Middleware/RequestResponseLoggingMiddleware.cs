// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using AspNetSpaTemplate.Utilities;

namespace AspNetSpaTemplate.Middleware;

/// <summary>
/// Middleware for detailed request/response logging.
/// Logs HTTP method, path, status code, and response time.
/// Sensitive data (passwords, tokens, PII) is automatically masked.
/// </summary>
public class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    // Endpoints to exclude from detailed logging (logs only method, path, status)
    private static readonly string[] ExcludedPaths = new[]
    {
        "/health",
        "/metrics",
        "/swagger",
        "/favicon"
    };

    // Parameters that contain sensitive data
    private static readonly string[] SensitiveParameters = new[]
    {
        "password",
        "token",
        "apikey",
        "secret",
        "creditcard",
        "ssn",
        "authorization"
    };

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Log incoming request
        LogRequest(context);

        // Capture response by replacing response stream
        var originalResponseBody = context.Response.Body;
        using (var memoryStream = new MemoryStream())
        {
            context.Response.Body = memoryStream;

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                // Log response
                LogResponse(context, stopwatch.ElapsedMilliseconds);

                // Copy captured response to original stream
                await memoryStream.CopyToAsync(originalResponseBody);
            }
        }
    }

    private void LogRequest(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "";

        // Skip logging excluded paths at detail level
        if (IsExcludedPath(path))
            return;

        var sb = new StringBuilder();
        sb.AppendLine($"[REQUEST] {context.Request.Method} {path}");

        // Log headers (excluding sensitive ones)
        if (context.Request.Headers.Count > 0)
        {
            sb.AppendLine("Headers:");
            foreach (var header in context.Request.Headers)
            {
                if (IsSensitiveHeader(header.Key))
                {
                    sb.AppendLine($"  {header.Key}: [REDACTED]");
                }
                else
                {
                    sb.AppendLine($"  {header.Key}: {header.Value}");
                }
            }
        }

        // Log query parameters (mask sensitive ones)
        if (context.Request.QueryString.HasValue)
        {
            sb.AppendLine($"Query: {MaskSensitiveData(context.Request.QueryString.Value)}");
        }

        _logger.LogInformation(sb.ToString());
    }

    private void LogResponse(HttpContext context, long elapsedMilliseconds)
    {
        var path = context.Request.Path.Value ?? "";

        // Always log response status
        var statusCode = context.Response.StatusCode;
        var correlationId = context.GetCorrelationId();

        _logger.LogInformation(
            $"[RESPONSE] {context.Request.Method} {path} => {statusCode} ({elapsedMilliseconds}ms) | CorrelationId: {correlationId}");

        // Alert on slow requests (> 1 second)
        if (elapsedMilliseconds > 1000)
        {
            _logger.LogWarning($"Slow request detected: {path} took {elapsedMilliseconds}ms");
        }

        // Alert on errors (5xx)
        if (statusCode >= 500)
        {
            _logger.LogError($"Server error in {path}: {statusCode}");
        }
    }

    /// <summary>
    /// Masks sensitive parameter values from query strings and post data.
    /// </summary>
    private static string MaskSensitiveData(string data)
    {
        if (string.IsNullOrEmpty(data))
            return data;

        // Simple masking: replace values of sensitive params with [REDACTED]
        foreach (var param in SensitiveParameters)
        {
            var pattern = $@"{param}=([^&]+)";
            data = System.Text.RegularExpressions.Regex.Replace(
                data, pattern, $"{param}=[REDACTED]", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        return data;
    }

    /// <summary>
    /// Checks if a header contains sensitive information.
    /// </summary>
    private static bool IsSensitiveHeader(string headerName)
    {
        return SensitiveParameters.Any(sp =>
            headerName.Equals(sp, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if path should be excluded from detailed logging.
    /// </summary>
    private static bool IsExcludedPath(string path)
    {
        return ExcludedPaths.Any(ep => path.Contains(ep, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// Request/Response logging configuration.
/// </summary>
public class LoggingMiddlewareOptions
{
    public bool LogRequestHeaders { get; set; } = true;
    public bool LogResponseHeaders { get; set; } = false; // Be careful with sensitive response headers
    public bool LogRequestBody { get; set; } = false; // Can be expensive for large payloads
    public bool LogResponseBody { get; set; } = false;
    public int SlowRequestThresholdMs { get; set; } = 1000;
    public List<string> ExcludedPaths { get; set; } = new();
}
