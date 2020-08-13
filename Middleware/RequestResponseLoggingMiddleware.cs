#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using AspNetSpaTemplate.Utilities;
using Microsoft.Extensions.Options;

namespace AspNetSpaTemplate.Middleware;

/// <summary>
/// Configures what the <see cref="RequestResponseLoggingMiddleware"/> emits.
/// Bind this class to the "RequestLogging" section of appsettings.json.
/// </summary>
public sealed class LoggingMiddlewareOptions
{
    public const string SectionName = "RequestLogging";

    /// <summary>Set to false to disable the middleware entirely.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Controls how much detail is logged.
    /// Minimal  – method, path, status, duration only.
    /// Standard – adds query string (sensitive values redacted).
    /// Detailed – adds request headers (sensitive headers redacted).
    /// </summary>
    public string VerbosityLevel { get; set; } = "Standard";

    public bool LogRequestHeaders { get; set; } = false;
    public bool LogResponseHeaders { get; set; } = false;
    public bool LogRequestBody { get; set; } = false;
    public bool LogResponseBody { get; set; } = false;
    public int SlowRequestThresholdMs { get; set; } = 1000;
    public List<string> ExcludedPaths { get; set; } = new();
}

/// <summary>
/// Middleware for structured HTTP request/response logging.
/// Logs method, path, status code, and duration via <see cref="ILogger"/>.
/// Verbosity and opt-in behaviour are driven by <see cref="LoggingMiddlewareOptions"/>
/// which can be configured in appsettings.json under the "RequestLogging" key.
/// Sensitive data (passwords, tokens, PII) is automatically masked.
/// </summary>
public sealed class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
    private readonly LoggingMiddlewareOptions _options;

    private static readonly string[] SensitiveParameters =
    {
        "password", "token", "apikey", "secret",
        "creditcard", "ssn", "authorization"
    };

    private static readonly string[] DefaultExcludedPaths =
    {
        "/health", "/metrics", "/swagger", "/favicon"
    };

    public RequestResponseLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestResponseLoggingMiddleware> logger,
        IOptions<LoggingMiddlewareOptions> options)
    {
        _next = next;
        _logger = logger;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_options.Enabled)
        {
            await _next(context);
            return;
        }

        var path = context.Request.Path.Value ?? string.Empty;
        if (IsExcludedPath(path))
        {
            await _next(context);
            return;
        }

        LogRequest(context, path);

        var originalResponseBody = context.Response.Body;
        using var memoryStream = new MemoryStream();
        context.Response.Body = memoryStream;

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            memoryStream.Seek(0, SeekOrigin.Begin);
            LogResponse(context, path, stopwatch.ElapsedMilliseconds);
            memoryStream.Seek(0, SeekOrigin.Begin);
            await memoryStream.CopyToAsync(originalResponseBody);
            context.Response.Body = originalResponseBody;
        }
    }

    private void LogRequest(HttpContext context, string path)
    {
        var verbosity = _options.VerbosityLevel;

        if (verbosity == "Minimal")
        {
            _logger.LogInformation("[REQUEST] {Method} {Path}", context.Request.Method, path);
            return;
        }

        var sb = new StringBuilder();
        sb.Append($"[REQUEST] {context.Request.Method} {path}");

        if (context.Request.QueryString.HasValue)
            sb.Append($" | Query: {MaskSensitiveData(context.Request.QueryString.Value ?? string.Empty)}");

        if ((verbosity == "Detailed" || _options.LogRequestHeaders) && context.Request.Headers.Count > 0)
        {
            sb.AppendLine();
            sb.Append("Headers:");
            foreach (var header in context.Request.Headers)
            {
                var value = IsSensitiveHeader(header.Key) ? "[REDACTED]" : header.Value.ToString();
                sb.Append($"\n  {header.Key}: {value}");
            }
        }

        _logger.LogInformation("{RequestLog}", sb.ToString());
    }

    private void LogResponse(HttpContext context, string path, long elapsedMs)
    {
        var statusCode = context.Response.StatusCode;
        var correlationId = context.GetCorrelationId();

        _logger.LogInformation(
            "[RESPONSE] {Method} {Path} => {StatusCode} ({Elapsed}ms) | CorrelationId: {CorrelationId}",
            context.Request.Method, path, statusCode, elapsedMs, correlationId);

        if (elapsedMs > _options.SlowRequestThresholdMs)
            _logger.LogWarning("Slow request detected: {Path} took {Elapsed}ms", path, elapsedMs);

        if (statusCode >= 500)
            _logger.LogError("Server error {StatusCode} on {Method} {Path}", statusCode, context.Request.Method, path);
    }

    private bool IsExcludedPath(string path)
    {
        var allExcluded = DefaultExcludedPaths.Concat(_options.ExcludedPaths);
        return allExcluded.Any(ep => path.Contains(ep, StringComparison.OrdinalIgnoreCase));
    }

    private static string MaskSensitiveData(string data)
    {
        if (string.IsNullOrEmpty(data))
            return data;

        foreach (var param in SensitiveParameters)
        {
            var pattern = $@"{param}=([^&]+)";
            data = System.Text.RegularExpressions.Regex.Replace(
                data, pattern, $"{param}=[REDACTED]", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        return data;
    }

    private static bool IsSensitiveHeader(string headerName) =>
        SensitiveParameters.Any(sp => headerName.Equals(sp, StringComparison.OrdinalIgnoreCase));
}
