# CacheMaintenanceWorker

`CacheMaintenanceWorker` is an `IBackgroundTask` implementation that performs one cache-maintenance pass each time the scheduler invokes it. A pass reads and logs cache statistics, warns when the hit rate is below 50% after more than 100 requests, removes expired entries, and removes entries matching `temp:*` and `ratelimit:*`.

The same source file also defines the cache-health monitoring API: `ICacheHealthMonitor`, `CacheHealthReport`, and `DefaultCacheHealthMonitor`.

## `CacheMaintenanceWorker`

### `public CacheMaintenanceWorker(ICacheService cacheService, ILogger<CacheMaintenanceWorker> logger)`

Creates the worker with the cache service it maintains and the logger used for progress, warning, and error messages.

- `cacheService`: Provides cache statistics and removal operations.
- `logger`: Receives maintenance logs.

### `public string TaskName { get; }`

Returns the fixed task name `"CacheMaintenanceWorker"`, which the scheduler uses to identify the task.

### `public TimeSpan? ExecutionInterval { get; }`

Returns a five-minute interval. The scheduler is responsible for applying this interval; the worker itself does not contain a continuous loop.

### `public async Task ExecuteAsync(CancellationToken cancellationToken)`

Executes one maintenance pass. It:

1. Retrieves statistics with `ICacheService.GetStatisticsAsync()` and logs the item count, hit rate, and hit/request totals.
2. Logs a warning when there have been more than 100 requests and the hit rate is below `0.5`.
3. Removes expired entries, then removes entries matching `temp:*` and `ratelimit:*`.
4. On successful completion, records the current UTC time and increments the execution count.

Exceptions from the maintenance pass are caught and logged rather than propagated. Cleanup also catches and logs its own exceptions. Consequently, a cleanup failure does not prevent the pass from being recorded as completed. The cancellation token is accepted to satisfy `IBackgroundTask`; the current cache operations do not consume it directly.

### `public BackgroundTaskStatus GetStatus()`

Returns a new status snapshot containing:

- `TaskName`: `"CacheMaintenanceWorker"`.
- `LastExecutedAt`: the UTC completion time of the latest pass that reached the end of `ExecuteAsync`, or `null` before then.
- `ExecutionCount`: the number of passes that reached the end of `ExecuteAsync`.

Other `BackgroundTaskStatus` properties retain their defaults because this worker does not populate them.

## `ICacheHealthMonitor`

The public health-monitoring interface declares:

### `Task<CacheHealthReport> GetHealthReportAsync()`

Builds a current health report from cache statistics.

### `Task<bool> IsCacheHealthyAsync()`

Returns whether the current report contains no health warnings.

## `CacheHealthReport`

`CacheHealthReport` is a mutable report model with the following public members:

- `bool IsHealthy { get; set; }`: Whether no health thresholds were exceeded.
- `double HitRate { get; set; }`: Cache hits divided by total requests, as reported by the cache service.
- `long ItemCount { get; set; }`: Current number of cache entries.
- `long MemoryUsageBytes { get; set; }`: Approximate cache memory usage in bytes.
- `int? WarningCount { get; set; }`: Number of generated warnings.
- `List<string> Warnings { get; set; }`: Mutable warning collection, initialized to an empty list.
- `DateTime GeneratedAt { get; set; }`: Report creation timestamp, initialized to `DateTime.UtcNow`.
- `string ToString()`: Returns a summary containing the health flag, metrics, warning count, and warning-list representation.

## `DefaultCacheHealthMonitor`

### `public DefaultCacheHealthMonitor(ICacheService cacheService, ILogger<DefaultCacheHealthMonitor> logger)`

Creates the default monitor with the cache service used to obtain statistics and a logger. The logger is retained by the monitor, although the current health-check methods do not write log messages.

### `public async Task<CacheHealthReport> GetHealthReportAsync()`

Retrieves current cache statistics and adds warnings under these conditions:

- Hit rate is below `0.4` after more than 100 total requests.
- Approximate memory usage is greater than `1,000,000,000` bytes.
- Item count is greater than `100,000`.

The returned report is healthy only when no warnings were added. Its `GeneratedAt` value comes from the report model's UTC initializer.

### `public async Task<bool> IsCacheHealthyAsync()`

Calls `GetHealthReportAsync()` and returns its `IsHealthy` value.

## Registration and usage

Register the worker with the application's `IBackgroundTaskScheduler`, together with its dependencies. The scheduler calls `ExecuteAsync` according to `ExecutionInterval` or when manually triggered.

```csharp
var worker = new CacheMaintenanceWorker(cacheService, workerLogger);
backgroundTaskScheduler.RegisterTask(worker);

await backgroundTaskScheduler.TriggerTaskAsync(worker.TaskName);

BackgroundTaskStatus status = worker.GetStatus();
```

The health monitor is a separate type; it is not exposed as a property of `CacheMaintenanceWorker`.

```csharp
var monitor = new DefaultCacheHealthMonitor(cacheService, healthLogger);
CacheHealthReport report = await monitor.GetHealthReportAsync();

if (!report.IsHealthy)
{
    foreach (string warning in report.Warnings)
    {
        // Forward the warning to the application's monitoring system.
    }
}
```
