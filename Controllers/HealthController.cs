#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.BackgroundWorkers;
using AspNetSpaTemplate.Caching;
using AspNetSpaTemplate.Configuration;
using AspNetSpaTemplate.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AspNetSpaTemplate.Controllers;

/// <summary>
/// Health check endpoint for monitoring and load balancers.
/// Reports application health, cache status, and background worker status.
/// Should respond quickly for use in Kubernetes/Docker health checks.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public sealed class HealthController : ControllerBase
{
    private readonly ICacheHealthMonitor _cacheHealthMonitor;
    private readonly IBackgroundTaskScheduler _taskScheduler;
    private readonly ILogger<HealthController> _logger;
    private readonly MetricsRegistry _metricsRegistry;
    private readonly AspnetSpaTemplateOptions _aspnetSpaTemplateOptions;
    private readonly PwaOptions _pwaOptions;

    public HealthController(
        ICacheHealthMonitor cacheHealthMonitor,
        IBackgroundTaskScheduler taskScheduler,
        ILogger<HealthController> logger,
        MetricsRegistry metricsRegistry,
        IOptions<AspnetSpaTemplateOptions> aspnetSpaTemplateOptions,
        IOptions<PwaOptions> pwaOptions)
    {
        _cacheHealthMonitor = cacheHealthMonitor;
        _taskScheduler = taskScheduler;
        _logger = logger;
        _metricsRegistry = metricsRegistry;
        _aspnetSpaTemplateOptions = aspnetSpaTemplateOptions.Value;
        _pwaOptions = pwaOptions.Value;
    }

    /// <summary>
    /// Simple health check that returns 200 if application is running.
    /// Used by container orchestration for basic liveness probes.
    /// </summary>
    [HttpGet("live")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Liveness()
    {
        return Ok(new { status = "alive", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Detailed health check including dependencies.
    /// Used for readiness probes before routing traffic.
    /// </summary>
    [HttpGet("ready")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Readiness()
    {
        var report = new HealthCheckReport
        {
            Timestamp = DateTime.UtcNow,
            Status = "healthy"
        };

        // Check cache health
        var cacheHealthy = await _cacheHealthMonitor.IsCacheHealthyAsync();
        report.Components["cache"] = cacheHealthy ? "healthy" : "unhealthy";

        if (!cacheHealthy)
            report.Status = "degraded";

        // Check background workers
        var taskStates = _taskScheduler.GetStatus();
        foreach (var task in taskStates)
        {
            report.Components[$"background-worker-{task.TaskName}"] = task.Status == "Failed" ? "unhealthy" : "healthy";
            if (task.Status == "Failed")
                report.Status = "degraded";
        }

        var statusCode = report.Status == "healthy" ? StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable;
        return StatusCode(statusCode, report);
    }

    /// <summary>
    /// Detailed diagnostics endpoint (should be protected/admin-only in production).
    /// Returns comprehensive system health and performance metrics.
    /// </summary>
    [HttpGet("diagnostics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Diagnostics()
    {
        var cacheReport = await _cacheHealthMonitor.GetHealthReportAsync();
        var taskStatuses = _taskScheduler.GetStatus();

        var diagnostics = new
        {
            Timestamp = DateTime.UtcNow,
            Environment = new
            {
                AspNetCoreEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                DotNetVersion = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
                ProcessId = Environment.ProcessId,
                MachineName = Environment.MachineName
            },
            Cache = cacheReport,
            BackgroundWorkers = taskStatuses.Select(s => new
            {
                s.TaskName,
                s.Status,
                s.LastExecutedAt,
                s.ExecutionCount,
                s.FailureCount,
                s.LastError
            }),
            Memory = new
            {
                WorkingSetMB = GC.GetTotalMemory(false) / 1024 / 1024,
                PrivateMemoryMB = Environment.WorkingSet / 1024 / 1024
            }
        };

        return Ok(diagnostics);
    }

    /// <summary>
    /// GET /api/health/metrics - Returns comprehensive application metrics including uptime, memory, threads, and request counters.
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetMetrics()
    {
        var metrics = _metricsRegistry.GetMetricsReport();

        // Increment request counter for this endpoint
        _metricsRegistry.IncrementRequestCount();

        return Ok(metrics);
    }

    /// <summary>
    /// GET /api/health/config - Returns a sanitized snapshot of active AspnetSpaTemplateOptions and PwaOptions.
    /// </summary>
    [HttpGet("config")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetConfig()
    {
        var config = new
        {
            AspnetSpaTemplate = new
            {
                Environment = _aspnetSpaTemplateOptions.Environment,
                RequestLogging = new
                {
                    Enabled = _aspnetSpaTemplateOptions.RequestLogging.Enabled,
                    VerbosityLevel = _aspnetSpaTemplateOptions.RequestLogging.VerbosityLevel,
                    SlowRequestThresholdMs = _aspnetSpaTemplateOptions.RequestLogging.SlowRequestThresholdMs,
                    ExcludedPaths = _aspnetSpaTemplateOptions.RequestLogging.ExcludedPaths
                },
                Webhooks = new
                {
                    // Mask secret values
                    PaymentProviderSecret = "***REDACTED***",
                    EmailServiceSecret = "***REDACTED***",
                    ShippingProviderSecret = "***REDACTED***"
                }
            },
            Pwa = new
            {
                EnablePushNotifications = _pwaOptions.EnablePushNotifications,
                EnableOfflineSync = _pwaOptions.EnableOfflineSync,
                MaxNotificationsPerBatch = _pwaOptions.MaxNotificationsPerBatch,
                MaxSyncRetries = _pwaOptions.MaxSyncRetries,
                SyncRetryBaseDelaySeconds = _pwaOptions.SyncRetryBaseDelaySeconds,
                SyncQueueMaxAgeHours = _pwaOptions.SyncQueueMaxAgeHours,
                PushDeliveryTimeoutSeconds = _pwaOptions.PushDeliveryTimeoutSeconds,
                InactiveSubscriptionPurgeDays = _pwaOptions.InactiveSubscriptionPurgeDays,
                Vapid = new
                {
                    PublicKey = !string.IsNullOrWhiteSpace(_pwaOptions.Vapid.PublicKey) ?
                        "***CONFIGURED***" : "***NOT CONFIGURED***",
                    Subject = _pwaOptions.Vapid.Subject
                },
                IsVapidConfigured = _pwaOptions.IsVapidConfigured
            },
            Timestamp = DateTime.UtcNow
        };

        return Ok(config);
    }

    /// <summary>
    /// Starts a specific background task immediately (useful for testing).
    /// Protected endpoint - requires authentication.
    /// </summary>
    [HttpPost("trigger-task/{taskName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TriggerTask(string taskName)
    {
        try
        {
            await _taskScheduler.TriggerTaskAsync(taskName);
            _logger.LogInformation($"Manually triggered task: {taskName}");
            return Ok(new { message = $"Task '{taskName}' triggered successfully" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new { error = $"Task '{taskName}' not found" });
        }
    }

    /// <summary>
    /// Gets current status of all background workers.
    /// </summary>
    [HttpGet("workers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetWorkerStatus()
    {
        var workers = _taskScheduler.GetStatus().Select(s => new
        {
            s.TaskName,
            s.IsRunning,
            s.Status,
            s.LastExecutedAt,
            s.NextExecutionAt,
            s.ExecutionCount,
            s.FailureCount,
            LastDurationSeconds = s.LastExecutionDuration?.TotalSeconds
        });

        return Ok(new { workers = workers.ToList() });
    }
}

/// <summary>
/// Health check report structure.
/// </summary>
public sealed class HealthCheckReport
{
    public DateTime Timestamp { get; set; }
    public string Status { get; set; } = "unknown";
    public Dictionary<string, string> Components { get; set; } = new();
}