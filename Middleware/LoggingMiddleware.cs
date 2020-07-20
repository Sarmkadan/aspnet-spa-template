// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;

namespace AspNetSpaTemplate.Middleware;

/// <summary>
/// Middleware for logging HTTP requests and responses.
/// </summary>
public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var request = context.Request;

        _logger.LogInformation(
            "HTTP Request: {Method} {Path} - User: {User}",
            request.Method,
            request.Path.Value,
            context.User?.Identity?.Name ?? "Anonymous"
        );

        var originalBodyStream = context.Response.Body;
        using (var responseBody = new MemoryStream())
        {
            context.Response.Body = responseBody;

            try
            {
                await _next(context);

                stopwatch.Stop();

                _logger.LogInformation(
                    "HTTP Response: {Method} {Path} - Status: {StatusCode} - Duration: {ElapsedMilliseconds}ms",
                    request.Method,
                    request.Path.Value,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds
                );

                if (context.Response.StatusCode >= 400)
                {
                    _logger.LogWarning(
                        "HTTP Error Response: {Method} {Path} - Status: {StatusCode}",
                        request.Method,
                        request.Path.Value,
                        context.Response.StatusCode
                    );
                }

                await responseBody.CopyToAsync(originalBodyStream);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(
                    ex,
                    "HTTP Request Exception: {Method} {Path} - Duration: {ElapsedMilliseconds}ms - Error: {Message}",
                    request.Method,
                    request.Path.Value,
                    stopwatch.ElapsedMilliseconds,
                    ex.Message
                );
                throw;
            }
            finally
            {
                context.Response.Body = originalBodyStream;
            }
        }
    }
}
