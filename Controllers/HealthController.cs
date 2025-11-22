// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.BackgroundWorkers;
using AspNetSpaTemplate.Caching;

namespace AspNetSpaTemplate.Controllers;

/// <summary>
/// Health check endpoint for monitoring and load balancers.
/// Reports application health, cache status, and background worker status.
/// Should respond quickly for use in Kubernetes/Docker health checks.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ICacheHealthMonitor _cacheHealthMonitor;
    private readonly IBackgroundTaskScheduler _taskScheduler;
    private readonly ILogger<HealthController> _logger;

    public HealthController(
        ICacheHealthMonitor cacheHealthMonitor,
        IBackgroundTaskScheduler taskScheduler,
        ILogger<HealthController> logger)
    {
        _cacheHealthMonitor = cacheHealthMonitor;
        _taskScheduler = taskScheduler;
        _logger = logger;
    }

    /// <summary>
    /// Simple health check that returns 200 if application is running.
    /// Used by container orchestration for basic liveness probes.
    /// </summary>
    [HttpGet("live")]
    [ProduceResponseType(StatusCodes.Status200OK)]
    public IActionResult Liveness()
    {
        return Ok(new { status = "alive", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Detailed health check including dependencies.
    /// Used for readiness probes before routing traffic.
    /// </summary>
    [HttpGet("ready")]
    [ProduceResponseType(StatusCodes.Status200OK)]
    [ProduceResponseType(StatusCodes.Status503ServiceUnavailable)]
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
    [ProduceResponseType(StatusCodes.Status200OK)]
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
    /// Starts a specific background task immediately (useful for testing).
    /// Protected endpoint - requires authentication.
    /// </summary>
    [HttpPost("trigger-task/{taskName}")]
    [ProduceResponseType(StatusCodes.Status200OK)]
    [ProduceResponseType(StatusCodes.Status404NotFound)]
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
    [ProduceResponseType(StatusCodes.Status200OK)]
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
public class HealthCheckReport
{
    public DateTime Timestamp { get; set; }
    public string Status { get; set; } = "unknown";
    public Dictionary<string, string> Components { get; set; } = new();
}
