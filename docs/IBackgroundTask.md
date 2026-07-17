# IBackgroundTask

`IBackgroundTask` represents a contract for a named, monitorable background operation within the application. It exposes runtime state and execution metrics such as current running status, last and next execution timestamps, duration of the last run, cumulative execution and failure counts, and the most recent error message. This interface is intended to be implemented by services that perform recurring or long-running work and need to surface their operational health to monitoring dashboards, health checks, or administrative APIs.

## API

### `string TaskName`
Gets the unique, human-readable identifier for this background task. This name is typically used in logs, status displays, and management interfaces to distinguish one task from another.

### `bool IsRunning`
Indicates whether the task is currently executing. Returns `true` if the task’s logic is in progress; otherwise `false`.

### `DateTime? LastExecutedAt`
Gets the date and time (in UTC) when the task last completed an execution cycle. Returns `null` if the task has never run.

### `DateTime? NextExecutionAt`
Gets the date and time (in UTC) when the task is scheduled to run next. Returns `null` if the task is not scheduled to run again (e.g., it has been stopped, is a one-shot task that has already completed, or no schedule has been configured).

### `TimeSpan? LastExecutionDuration`
Gets the wall-clock duration of the most recent execution. Returns `null` if the task has never completed an execution.

### `int ExecutionCount`
Gets the total number of times the task has successfully started and completed its execution cycle. This count is incremented only after a full, successful run.

### `int FailureCount`
Gets the total number of times the task’s execution cycle has failed (i.e., thrown an unhandled exception). This count is incremented whenever an execution terminates with an error.

### `string? LastError`
Gets the message of the last unhandled exception that caused a failure. Returns `null` if no failures have occurred or if the last failure did not provide an error message.

## Usage

### Example 1: Displaying Task Status in a Razor Page
```csharp
@page
@inject IEnumerable<IBackgroundTask> BackgroundTasks

<h3>Background Tasks</h3>
<table class="table">
    <thead>
        <tr>
            <th>Task</th>
            <th>Status</th>
            <th>Last Run</th>
            <th>Next Run</th>
            <th>Duration</th>
            <th>Success / Fail</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var task in BackgroundTasks)
        {
            <tr>
                <td>@task.TaskName</td>
                <td>@(task.IsRunning ? "Running" : "Idle")</td>
                <td>@(task.LastExecutedAt?.ToString("g") ?? "Never")</td>
                <td>@(task.NextExecutionAt?.ToString("g") ?? "Not scheduled")</td>
                <td>@(task.LastExecutionDuration?.ToString(@"hh\:mm\:ss") ?? "—")</td>
                <td>@task.ExecutionCount / @task.FailureCount</td>
            </tr>
        }
    </tbody>
</table>
```

### Example 2: Implementing a Simple Recurring Task

```csharp
public class CacheMaintenanceWorker : IBackgroundTask
{
    public string TaskName => "Cache Maintenance";
    public bool IsRunning { get; private set; }
    public DateTime? LastExecutedAt { get; private set; }
    public DateTime? NextExecutionAt { get; private set; }
    public TimeSpan? LastExecutionDuration { get; private set; }
    public int ExecutionCount { get; private set; }
    public int FailureCount { get; private set; }
    public string? LastError { get; private set; }

    private readonly ICacheService _cache;

    public CacheMaintenanceWorker(ICacheService cache)
    {
        _cache = cache;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        IsRunning = true;
        var startedAt = DateTime.UtcNow;

        try
        {
            await _cache.EvictExpiredEntriesAsync(cancellationToken);
            ExecutionCount++;
            LastError = null;
        }
        catch (Exception ex)
        {
            FailureCount++;
            LastError = ex.Message;
        }
        finally
        {
            LastExecutedAt = DateTime.UtcNow;
            LastExecutionDuration = LastExecutedAt.Value - startedAt;
            NextExecutionAt = LastExecutedAt.Value.AddMinutes(30);
            IsRunning = false;
        }
    }
}
```

## Notes

- All date-time values are expected to be in UTC. Consumers should convert to local time for display purposes if needed.
- `IsRunning` reflects instantaneous state and is not guaranteed to be consistent with `LastExecutedAt` or `NextExecutionAt` across threads unless the implementation uses appropriate synchronization.
- `LastExecutedAt` and `NextExecutionAt` may both be `null` for a task that has never been scheduled or executed.
- `ExecutionCount` and `FailureCount` are cumulative and monotonically increasing; they are not reset between application restarts unless the implementation explicitly persists and resets them.
- `LastError` is overwritten on each subsequent failure. A successful execution does not automatically clear `LastError` unless the implementation explicitly sets it to `null`.
- Implementations must ensure thread safety when updating properties if the task can be inspected concurrently from multiple threads (e.g., from a status endpoint while the task is executing).
