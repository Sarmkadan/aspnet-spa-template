// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.BackgroundWorkers;

/// <summary>
/// Scheduler for managing background tasks.
/// Runs periodic tasks at specified intervals.
/// Suitable for single-server deployments (use Hangfire/Quartz for distributed).
/// </summary>
public class BackgroundTaskScheduler : IBackgroundTaskScheduler, IDisposable
{
    private readonly List<IBackgroundTask> _tasks = new();
    private readonly Dictionary<string, BackgroundTaskState> _taskStates = new();
    private readonly ILogger<BackgroundTaskScheduler> _logger;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _schedulerTask;

    public BackgroundTaskScheduler(ILogger<BackgroundTaskScheduler> logger)
    {
        _logger = logger;
    }

    public void RegisterTask(IBackgroundTask task)
    {
        if (task == null)
            throw new ArgumentNullException(nameof(task));

        _tasks.Add(task);
        _taskStates[task.TaskName] = new BackgroundTaskState { TaskName = task.TaskName };
        _logger.LogInformation($"Registered background task: {task.TaskName}");
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_schedulerTask != null)
            throw new InvalidOperationException("Scheduler is already running");

        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _logger.LogInformation($"Starting background task scheduler with {_tasks.Count} tasks");

        _schedulerTask = RunSchedulerAsync(_cancellationTokenSource.Token);
        await Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        _logger.LogInformation("Stopping background task scheduler");
        _cancellationTokenSource?.Cancel();

        if (_schedulerTask != null)
        {
            try
            {
                await _schedulerTask;
            }
            catch (OperationCanceledException)
            {
                // Expected when cancellation is requested
            }
        }
    }

    public IEnumerable<BackgroundTaskStatus> GetStatus()
    {
        return _taskStates.Values.Select(s => new BackgroundTaskStatus
        {
            TaskName = s.TaskName,
            IsRunning = s.IsRunning,
            LastExecutedAt = s.LastExecutedAt,
            NextExecutionAt = s.NextExecutionAt,
            LastExecutionDuration = s.LastExecutionDuration,
            ExecutionCount = s.ExecutionCount,
            FailureCount = s.FailureCount,
            LastError = s.LastError
        });
    }

    public async Task TriggerTaskAsync(string taskName)
    {
        var task = _tasks.FirstOrDefault(t => t.TaskName == taskName);
        if (task == null)
            throw new KeyNotFoundException($"Task not found: {taskName}");

        _logger.LogInformation($"Manually triggering task: {taskName}");
        await ExecuteTaskAsync(task, _cancellationTokenSource?.Token ?? CancellationToken.None);
    }

    private async Task RunSchedulerAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                foreach (var task in _tasks)
                {
                    // Skip tasks that don't have interval (one-time tasks)
                    if (task.ExecutionInterval == null)
                        continue;

                    var state = _taskStates[task.TaskName];
                    var now = DateTime.UtcNow;

                    // Check if it's time to execute
                    if (state.NextExecutionAt == null || state.NextExecutionAt <= now)
                    {
                        // Execute without blocking scheduler
                        _ = ExecuteTaskAsync(task, cancellationToken);
                    }
                }

                // Check every second if tasks need to run
                await Task.Delay(1000, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Background task scheduler cancelled");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in background task scheduler");
            }
        }
    }

    private async Task ExecuteTaskAsync(IBackgroundTask task, CancellationToken cancellationToken)
    {
        var state = _taskStates[task.TaskName];

        // Don't run if already executing
        if (state.IsRunning)
            return;

        state.IsRunning = true;
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogInformation($"Starting background task: {task.TaskName}");
            await task.ExecuteAsync(cancellationToken);

            var duration = DateTime.UtcNow - startTime;
            state.LastExecutedAt = startTime;
            state.LastExecutionDuration = duration;
            state.ExecutionCount++;
            state.LastError = null;

            _logger.LogInformation($"Background task completed: {task.TaskName} (duration: {duration.TotalSeconds:F1}s)");

            // Schedule next execution
            if (task.ExecutionInterval.HasValue)
            {
                state.NextExecutionAt = DateTime.UtcNow.Add(task.ExecutionInterval.Value);
            }
        }
        catch (Exception ex)
        {
            state.FailureCount++;
            state.LastError = ex.Message;
            _logger.LogError(ex, $"Error executing background task: {task.TaskName}");
        }
        finally
        {
            state.IsRunning = false;
        }
    }

    public void Dispose()
    {
        _cancellationTokenSource?.Dispose();
    }

    /// <summary>
    /// Internal state tracking for a background task.
    /// </summary>
    private class BackgroundTaskState
    {
        public string TaskName { get; set; } = "";
        public bool IsRunning { get; set; }
        public DateTime? LastExecutedAt { get; set; }
        public DateTime? NextExecutionAt { get; set; }
        public TimeSpan? LastExecutionDuration { get; set; }
        public int ExecutionCount { get; set; }
        public int FailureCount { get; set; }
        public string? LastError { get; set; }
    }
}

/// <summary>
/// Extension methods for registering background tasks in DI container.
/// </summary>
public static class BackgroundTaskExtensions
{
    public static IServiceCollection AddBackgroundTaskScheduler(this IServiceCollection services)
    {
        services.AddSingleton<IBackgroundTaskScheduler, BackgroundTaskScheduler>();
        return services;
    }

    public static IServiceCollection AddBackgroundTask<T>(this IServiceCollection services) where T : class, IBackgroundTask
    {
        services.AddSingleton<T>();
        return services;
    }

    public static async Task<IApplicationBuilder> UseBackgroundTaskScheduler(this IApplicationBuilder app)
    {
        var scheduler = app.ApplicationServices.GetRequiredService<IBackgroundTaskScheduler>();

        // Register all IBackgroundTask implementations
        var tasks = app.ApplicationServices.GetServices<IBackgroundTask>();
        foreach (var task in tasks)
        {
            scheduler.RegisterTask(task);
        }

        // Start scheduler
        var appLifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();
        await scheduler.StartAsync(appLifetime.ApplicationStopping);

        // Stop scheduler on app shutdown
        appLifetime.ApplicationStopping.Register(() => scheduler.StopAsync().Wait());

        return app;
    }
}
