// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Background Tasks & Scheduled Jobs Guide

How to implement background tasks and scheduled jobs in aspnet-spa-template.

## Table of Contents
- [Hosted Services Overview](#hosted-services-overview)
- [Creating a Background Task](#creating-a-background-task)
- [Task Scheduling](#task-scheduling)
- [Best Practices](#best-practices)
- [Examples](#examples)

## Hosted Services Overview

ASP.NET Core's `IHostedService` allows long-running background tasks that start with the application.

### Key Interfaces

- **IHostedService**: Minimal interface with `StartAsync` and `StopAsync`
- **BackgroundService**: Abstract base class with `ExecuteAsync` method (recommended)

## Creating a Background Task

### Basic Implementation

**File: `BackgroundWorkers/MyBackgroundTask.cs`**

```csharp
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Hosting;

namespace AspNetSpaTemplate.BackgroundWorkers
{
    /// Example background task that runs periodically
    public class MyBackgroundTask : BackgroundService
    {
        private readonly ILogger<MyBackgroundTask> _logger;
        private readonly IServiceProvider _serviceProvider;

        public MyBackgroundTask(
            ILogger<MyBackgroundTask> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "MyBackgroundTask is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await DoWorkAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "An error occurred in background task");
                }

                // Wait 5 minutes before next execution
                await Task.Delay(
                    TimeSpan.FromMinutes(5), stoppingToken);
            }

            _logger.LogInformation(
                "MyBackgroundTask is stopping.");
        }

        private async Task DoWorkAsync(
            CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Executing background task at {time}",
                DateTime.UtcNow);

            // Use scoped services
            using (var scope = _serviceProvider
                .CreateScope())
            {
                var myService = scope.ServiceProvider
                    .GetRequiredService<IMyService>();

                await myService.DoSomethingAsync();
            }
        }
    }
}
```

## Task Scheduling

### Using a Timer-Based Schedule

```csharp
public class ScheduledTask : BackgroundService
{
    private readonly ILogger<ScheduledTask> _logger;
    private Timer _timer;

    public ScheduledTask(ILogger<ScheduledTask> logger)
    {
        _logger = logger;
    }

    protected override Task StartAsync(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Scheduled task starting.");

        // Run task immediately on startup
        _timer = new Timer(
            DoWork, null, TimeSpan.Zero,
            TimeSpan.FromHours(1));

        return Task.CompletedTask;
    }

    private void DoWork(object state)
    {
        _logger.LogInformation(
            "Scheduled task is working.");

        // Perform work here
    }

    protected override Task StopAsync(
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Scheduled task stopping.");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _timer?.Dispose();
        base.Dispose();
    }
}
```

### Using Cron Expressions

For more complex scheduling, use **NCronTab**:

```bash
dotnet add package NCronTab
```

```csharp
using NCrontab;

public class CronScheduledTask : BackgroundService
{
    private readonly ILogger<CronScheduledTask> _logger;
    private CrontabSchedule _schedule;
    private DateTime _nextRun;

    public CronScheduledTask(
        ILogger<CronScheduledTask> logger)
    {
        _logger = logger;
        // Run every day at 2 AM
        _schedule = CrontabSchedule.Parse("0 2 * * *");
        _nextRun = _schedule.GetNextOccurrence(
            DateTime.UtcNow);
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;

            if (now >= _nextRun)
            {
                _logger.LogInformation(
                    "Running scheduled task at {time}", now);

                // Do work here
                await DoWorkAsync();

                _nextRun = _schedule.GetNextOccurrence(now);
            }

            await Task.Delay(TimeSpan.FromSeconds(1),
                stoppingToken);
        }
    }

    private async Task DoWorkAsync()
    {
        // Implement your scheduled task logic
        await Task.CompletedTask;
    }
}
```

## Registering Background Tasks

**Update `Program.cs`:**

```csharp
// Register background tasks
builder.Services.AddHostedService<MyBackgroundTask>();
builder.Services.AddHostedService<CacheMaintenanceWorker>();
builder.Services.AddHostedService<NotificationWorker>();

// Register any services used by background tasks
builder.Services.AddScoped<IMyService, MyService>();
```

## Best Practices

### 1. Use Scoped Services Properly

```csharp
protected override async Task ExecuteAsync(
    CancellationToken stoppingToken)
{
    while (!stoppingToken.IsCancellationRequested)
    {
        // Create a new scope for each iteration
        using (var scope = _serviceProvider
            .CreateScope())
        {
            var myService = scope.ServiceProvider
                .GetRequiredService<IMyService>();

            await myService.DoWorkAsync();
        }

        await Task.Delay(TimeSpan.FromMinutes(1),
            stoppingToken);
    }
}
```

### 2. Handle Graceful Shutdown

```csharp
protected override Task StopAsync(
    CancellationToken cancellationToken)
{
    _logger.LogInformation("Background service stopping");
    
    // Cancel pending operations
    _timer?.Change(Timeout.Infinite, 0);
    
    return Task.CompletedTask;
}
```

### 3. Implement Comprehensive Logging

```csharp
_logger.LogInformation(
    "Task started at {StartTime}",
    DateTime.UtcNow);

try
{
    // Do work
    _logger.LogInformation("Task completed successfully");
}
catch (Exception ex)
{
    _logger.LogError(ex,
        "Task failed with exception");
}
```

### 4. Monitor Task Execution

```csharp
private async Task ExecuteWithMetricsAsync(
    Func<Task> action)
{
    var sw = Stopwatch.StartNew();

    try
    {
        await action();
        sw.Stop();
        _logger.LogInformation(
            "Task completed in {Elapsed}ms",
            sw.ElapsedMilliseconds);
    }
    catch (Exception ex)
    {
        sw.Stop();
        _logger.LogError(ex,
            "Task failed after {Elapsed}ms",
            sw.ElapsedMilliseconds);
    }
}
```

### 5. Prevent Overlapping Executions

```csharp
private bool _isRunning = false;

private async Task DoWorkAsync()
{
    if (_isRunning)
    {
        _logger.LogWarning("Task already running");
        return;
    }

    _isRunning = true;

    try
    {
        // Do work
    }
    finally
    {
        _isRunning = false;
    }
}
```

## Examples

### Example 1: Cache Maintenance Worker

See `BackgroundWorkers/CacheMaintenanceWorker.cs` in the project.

```csharp
public class CacheMaintenanceWorker : BackgroundService
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheMaintenanceWorker> _logger;

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation(
                "Cache maintenance starting");

            // Clear expired cache entries
            await _cacheService.ClearExpiredAsync();

            _logger.LogInformation(
                "Cache maintenance completed");

            // Run every hour
            await Task.Delay(
                TimeSpan.FromHours(1), stoppingToken);
        }
    }
}
```

### Example 2: Notification Worker

```csharp
public class NotificationWorker : BackgroundService
{
    private readonly INotificationService _notificationService;

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider
                .CreateScope())
            {
                var service = scope.ServiceProvider
                    .GetRequiredService<INotificationService>();

                // Process pending notifications
                await service.ProcessPendingAsync();
            }

            // Check every 5 minutes
            await Task.Delay(
                TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
```

### Example 3: Database Maintenance Task

```csharp
public class DatabaseMaintenanceTask : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        // Wait 5 minutes after startup before running
        await Task.Delay(TimeSpan.FromMinutes(5),
            stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider
                    .CreateScope())
                {
                    var context = scope.ServiceProvider
                        .GetRequiredService<AppDbContext>();

                    // Delete old logs
                    context.Database.ExecuteSqlRaw(
                        "DELETE FROM Logs WHERE CreatedAt < DATEADD(MONTH, -3, GETUTCDATE())");

                    await context.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Database maintenance failed");
            }

            // Run daily at 3 AM
            await Task.Delay(
                TimeSpan.FromHours(24), stoppingToken);
        }
    }
}
```

## Testing Background Tasks

```csharp
[Fact]
public async Task BackgroundTask_ExecutesSuccessfully()
{
    // Arrange
    var mockService = new Mock<IMyService>();
    var logger = new Mock<ILogger<MyBackgroundTask>>();
    var serviceProvider = new Mock<IServiceProvider>();
    var scope = new Mock<IServiceScope>();

    serviceProvider
        .Setup(sp => sp.CreateScope())
        .Returns(scope.Object);

    scope
        .Setup(s => s.ServiceProvider
            .GetService(typeof(IMyService)))
        .Returns(mockService.Object);

    var task = new MyBackgroundTask(logger.Object,
        serviceProvider.Object);

    // Act
    var cts = new CancellationTokenSource();
    cts.CancelAfter(TimeSpan.FromSeconds(1));

    await task.StartAsync(cts.Token);

    // Assert
    mockService.Verify(
        s => s.DoSomethingAsync(),
        Times.AtLeastOnce);
}
```

## Troubleshooting

### Task Not Running
- Check if registered in `Program.cs`
- Verify `ExecuteAsync` implementation
- Check application logs

### Task Running Too Often
- Adjust delay between executions
- Use appropriate time span

### Memory Leak in Task
- Ensure proper disposal of resources
- Use `using` statements for disposable objects
- Check for circular references

### Task Overlapping
- Implement locking mechanism
- Use `_isRunning` flag
- Consider using distributed lock for multiple instances

---

For more examples, see `BackgroundWorkers/` directory in the project.
