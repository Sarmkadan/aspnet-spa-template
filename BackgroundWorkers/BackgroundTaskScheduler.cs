#nullable enable
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

    /// <summary>
    /// Initializes a new instance of the <see cref="BackgroundTaskScheduler"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public BackgroundTaskScheduler(ILogger<BackgroundTaskScheduler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Registers a background task to be managed by the scheduler.
    /// </summary>
    /// <param name="task">The background task to register.</param>
    public void RegisterTask(IBackgroundTask task)
    {
        if (task is null)
            throw new ArgumentNullException(nameof(task));

        _tasks.Add(task);
        _taskStates[task.TaskName] = new BackgroundTaskState { TaskName = task.TaskName };
        _logger.LogInformation($"Registered background task: {task.TaskName}");
    }

    /// <summary>
    /// Starts the background task scheduler.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (_schedulerTask is not null)
            throw new InvalidOperationException("Scheduler is already running");

        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _logger.LogInformation($"Starting background task scheduler with {_tasks.Count} tasks");

        _schedulerTask = RunSchedulerAsync(_cancellationTokenSource.Token);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Stops the background task scheduler.
    /// </summary>
    public async Task StopAsync()
    {
        _logger.LogInformation("Stopping background task scheduler");
        _cancellationTokenSource?.Cancel();

        if (_schedulerTask is not null)
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

    /// <summary>
    /// Gets the current status of all managed background tasks.
    /// </summary>
    /// <returns>A collection of <see cref="BackgroundTaskStatus"/> objects.</returns>
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

    /// <summary>
    /// Manually triggers a specific background task to execute immediately.
    /// </summary>
    /// <param name="taskName">The name of the task to trigger.</param>
    public async Task TriggerTaskAsync(string taskName)
    {
        var task = _tasks.FirstOrDefault(t => t.TaskName == taskName);
        if (task is null)
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
                    if (task.ExecutionInterval is null)
                        continue;

                    var state = _taskStates[task.TaskName];
                    var now = DateTime.UtcNow;

                    // Check if it's time to execute
                    if (state.NextExecutionAt is null || state.NextExecutionAt <= now)
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

    /// <summary>
    /// Disposes of the resources used by the scheduler.
    /// </summary>
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
    /// <summary>
    /// Adds the background task scheduler to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddBackgroundTaskScheduler(this IServiceCollection services)
    {
        services.AddSingleton<IBackgroundTaskScheduler, BackgroundTaskScheduler>();
        return services;
    }

    /// <summary>
    /// Adds a background task type to the service collection.
    /// </summary>
    /// <typeparam name="T">The type of the background task.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddBackgroundTask<T>(this IServiceCollection services) where T : class, IBackgroundTask
    {
        services.AddSingleton<T>();
        return services;
    }

    /// <summary>
    /// Configures the application to use the background task scheduler.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The updated application builder.</returns>
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
        appLifetime.ApplicationStopping.Register(() => {
            _ = Task.Run(async () => await scheduler.StopAsync());
        });

        return app;
    }
}
