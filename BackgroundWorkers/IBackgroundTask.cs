// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.BackgroundWorkers;

/// <summary>
/// Interface for background tasks that run periodically or on-demand.
/// Examples: cache cleanup, notification sending, report generation, data synchronization.
/// Implementations should handle their own error logging and recovery.
/// </summary>
public interface IBackgroundTask
{
    /// <summary>
    /// Gets unique name for this background task.
    /// Used for logging and monitoring.
    /// </summary>
    string TaskName { get; }

    /// <summary>
    /// Gets interval for periodic execution.
    /// Return null for one-time or on-demand tasks.
    /// </summary>
    TimeSpan? ExecutionInterval { get; }

    /// <summary>
    /// Executes the background task.
    /// Should complete within reasonable time (few seconds to minutes).
    /// Must handle and log its own exceptions.
    /// </summary>
    Task ExecuteAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets execution state and metrics for monitoring.
    /// </summary>
    BackgroundTaskStatus GetStatus();
}

/// <summary>
/// Status information for monitoring background tasks.
/// </summary>
public class BackgroundTaskStatus
{
    public string TaskName { get; set; } = "";
    public bool IsRunning { get; set; }
    public DateTime? LastExecutedAt { get; set; }
    public DateTime? NextExecutionAt { get; set; }
    public TimeSpan? LastExecutionDuration { get; set; }
    public int ExecutionCount { get; set; }
    public int FailureCount { get; set; }
    public string? LastError { get; set; }
    public string Status => IsRunning ? "Running" : LastError != null ? "Failed" : "Idle";
}

/// <summary>
/// Scheduler for managing background tasks.
/// Runs tasks on schedule and handles lifecycle.
/// </summary>
public interface IBackgroundTaskScheduler
{
    /// <summary>
    /// Registers a background task for execution.
    /// </summary>
    void RegisterTask(IBackgroundTask task);

    /// <summary>
    /// Starts the scheduler and begins executing registered tasks.
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Stops the scheduler and cancels running tasks.
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// Gets status for all registered tasks.
    /// </summary>
    IEnumerable<BackgroundTaskStatus> GetStatus();

    /// <summary>
    /// Manually triggers a task to run immediately.
    /// </summary>
    Task TriggerTaskAsync(string taskName);
}
