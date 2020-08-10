// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using AspNetSpaTemplate.Services;

namespace AspNetSpaTemplate.Middleware;

/// <summary>
/// Intercepts requests for <c>/sw.js</c>, <c>/__asset-manifest.json</c>, and (in development)
/// <c>/__hmr</c>, handling service-worker headers, asset versioning responses, and the
/// Server-Sent Events stream used for hot module replacement.
/// </summary>
public sealed class HotReloadMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = false };

    private readonly RequestDelegate _next;
    private readonly IAssetVersioningService _versioning;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<HotReloadMiddleware> _logger;

    /// <summary>Initializes the middleware with all required dependencies.</summary>
    public HotReloadMiddleware(
        RequestDelegate next,
        IAssetVersioningService versioning,
        IWebHostEnvironment environment,
        ILogger<HotReloadMiddleware> logger)
    {
        _next = next;
        _versioning = versioning;
        _environment = environment;
        _logger = logger;
    }

    /// <summary>
    /// Routes asset-manifest and HMR requests; for <c>sw.js</c> sets required service-worker
    /// headers; all other requests are forwarded to the next middleware.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        switch (path)
        {
            case "/__asset-manifest.json":
                await ServeManifestAsync(context);
                return;

            case "/__hmr" when _environment.IsDevelopment():
                await ServeHmrStreamAsync(context);
                return;

            case "/sw.js":
                // Permit the worker to intercept the entire origin, not just /sw.js.
                context.Response.Headers["Service-Worker-Allowed"] = "/";
                context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
                break;
        }

        await _next(context);
    }

    private async Task ServeManifestAsync(HttpContext context)
    {
        var manifest = await _versioning.GetAssetManifestAsync(context.RequestAborted);

        var payload = new
        {
            version = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            generated = DateTimeOffset.UtcNow,
            assets = manifest
        };

        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.Headers["Cache-Control"] = "no-cache";

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(payload, JsonOptions),
            context.RequestAborted);
    }

    private async Task ServeHmrStreamAsync(HttpContext context)
    {
        context.Response.ContentType = "text/event-stream; charset=utf-8";
        context.Response.Headers["Cache-Control"] = "no-cache";
        context.Response.Headers["X-Accel-Buffering"] = "no"; // disable nginx proxy buffering

        var remote = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        _logger.LogDebug("HMR client connected from {Remote}", remote);

        await WriteSseAsync(context.Response, "connected", "{\"ok\":true}", context.RequestAborted);
        await context.Response.Body.FlushAsync(context.RequestAborted);

        try
        {
            await foreach (var changedPath in _versioning.WatchForChangesAsync(context.RequestAborted))
            {
                var data = JsonSerializer.Serialize(
                    new { path = changedPath, t = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() },
                    JsonOptions);

                await WriteSseAsync(context.Response, "asset-changed", data, context.RequestAborted);
                await context.Response.Body.FlushAsync(context.RequestAborted);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("HMR client {Remote} disconnected", remote);
        }
    }

    private static Task WriteSseAsync(
        HttpResponse response, string eventName, string data, CancellationToken ct = default)
        => response.WriteAsync($"event: {eventName}\ndata: {data}\n\n", ct);
}
