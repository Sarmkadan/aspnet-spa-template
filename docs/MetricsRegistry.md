# MetricsRegistry

`MetricsRegistry` is a singleton helper that collects runtime diagnostics for an ASP.NET Core SPA application. It exposes counters for HTTP requests, memory usage, thread‑pool activity, and garbage‑collection statistics, and provides snapshot objects that can be serialized or logged for monitoring purposes.

## API

### MetricsRegistry.Instance
- **Purpose**: Provides access to the shared singleton instance.
- **Parameters**: None.
- **Return value**: The live `MetricsRegistry` instance.
- **Exceptions**: None; the property never throws and lazily creates the instance on first access.

### void IncrementRequestCount()
- **Purpose**: Increments the internal counter that tracks the number of requests processed since the registry was created or last reset.
- **Parameters**: None.
- **Return value**: None.
- **Exceptions**: `ObjectDisposedException` if `Dispose()` has already been called on the singleton.

### MemoryMetrics GetMemoryMetrics()
- **Purpose**: Returns a snapshot of current memory usage metrics.
- **Parameters**: None.
- **Return value**: A `MemoryMetrics` struct containing `TotalMemoryMB`, `WorkingSetMB`, and `PrivateMemoryMB`.
- **Exceptions**: `ObjectDisposedException` if the registry has been disposed.

### ThreadMetrics GetThreadMetrics()
- **Purpose**: Returns a snapshot of thread‑pool related metrics.
- **Parameters**: None.
- **Return value**: A `ThreadMetrics` struct with `ActiveThreadCount`, `ThreadPoolThreadCount`, `ThreadPoolCompletedWorkItemCount`, `ThreadPoolPendingWorkItemCount`, and `ThreadPoolQueueLength`.
- **Exceptions**: `ObjectDisposedException` if the registry has been disposed.

### ProcessMetrics GetProcessMetrics()
- **Purpose**: Returns a snapshot of process‑level metrics such as heap size.
- **Parameters**: None.
- **Return value**: A `ProcessMetrics` struct containing `HeapSizeMB`.
- **Exceptions**: `ObjectDisposedException` if the registry has been disposed.

### MetricsReport GetMetricsReport()
- **Purpose**: Aggregates all available metrics into a single report object.
- **Parameters**: None.
- **Return value**: A `MetricsReport` instance that bundles memory, thread, process, and request‑count data.
- **Exceptions**: `ObjectDisposedException` if the registry has been disposed.

### void ResetRequestCounter()
- **Purpose**: Resets the request counter to zero.
- **Parameters**: None.
- **Return value**: None.
- **Exceptions**: `ObjectDisposedException` if the registry has been disposed.

### void Dispose()
- **Purpose**: Releases any internal resources and marks the registry as disposed. After disposal, all member accesses throw `ObjectDisposedException`.
- **Parameters**: None.
- **Return value**: None.
- **Exceptions**: None; calling `Dispose()` multiple times is safe.

### long TotalMemoryMB
- **Purpose**: Gets the total managed memory reported by the runtime, in megabytes.
- **Parameters**: None.
- **Return value**: Current total memory in MB.
- **Exceptions**: `ObjectDisposedException` if accessed after disposal.

### long WorkingSetMB
- **Purpose**: Gets the current working set of the process, in megabytes.
- **Parameters**: None.
- **Return value**: Working set size in MB.
- **Exceptions**: `ObjectDisposedException` if accessed after disposal.

### long PrivateMemoryMB
- **Purpose**: Gets the amount of private memory allocated for the process, in megabytes.
- **Parameters**: None.
- **Return value**: Private memory size in MB.
- **Exceptions**: `ObjectDisposedException` if accessed after disposal.

### int Gen0Collections
- **Purpose**: Gets the number of generation‑0 garbage collections that have occurred.
- **Parameters**: None.
- **Return value**: Gen0 collection count.
- **Exceptions**: `ObjectDisposedException` if accessed after disposal.

### int Gen1Collections
- **Purpose**: Gets the number of generation‑1 garbage collections that have occurred.
- **Parameters**: None.
- **Return value**: Gen1 collection count.
- **Exceptions**: `ObjectDisposedException` if accessed after disposal.

### int Gen2Collections
- **Purpose**: Gets the number of generation‑2 garbage collections that have occurred.
- **Parameters**: None.
- **Return value**: Gen2 collection count.
- **Exceptions**: `ObjectDisposedException` if accessed after disposal.

### long HeapSizeMB
- **Purpose**: Gets the size of the managed heap, in megabytes.
- **Parameters**: None.
- **Return value**: Heap size in MB.
- **Exceptions**: `ObjectDisposedException` if accessed after disposal.

### int ActiveThreadCount
- **Purpose**: Gets the number of currently active threads in the application.
- **Parameters**: None.
- **Return value**: Active thread count.
- **Exceptions**: `ObjectDisposedException` if accessed after disposal.

### int ThreadPoolThreadCount
- **Purpose**: Gets the number of threads in the thread pool.
- **Parameters**: None.
- **Return value**: Thread pool thread count.
- **Exceptions**: `ObjectDisposedException` if accessed after disposal.

### long ThreadPoolCompletedWorkItemCount
- **Purpose**: Gets the total number of work items completed by the thread pool.
- **Parameters**: None.
- **Return value**: Completed work‑item count.
- **Exceptions**: `ObjectDisposedException` if accessed after disposal.

### long ThreadPoolPendingWorkItemCount
- **Purpose**: Gets the number of work items currently queued but not yet processed by the thread pool.
- **Parameters**: None.
- **Return value**: Pending work‑item count.
- **Exceptions**: `ObjectDisposedException` if accessed after disposal.

### int ThreadPoolQueueLength
- **Purpose**: Gets the current length of the thread pool work‑item queue.
- **Parameters**: None.
- **Return value**: Queue length.
- **Exceptions**: `ObjectDisposedException` if accessed after disposal.

## Usage

```csharp
using AspNetSpaTemplate.Diagnostics; // namespace containing MetricsRegistry

// Example 1: Collect a metrics report and log it
var registry = MetricsRegistry.Instance;
registry.IncrementRequestCount(); // simulate a request

var report = registry.GetMetricsReport();
// Assuming a logger that accepts an object
_logger.Information("Runtime metrics: {Report}", report);

// Example 2: Safe disposal at application shutdown
try
{
    // Use the registry during the app lifetime
    var mem = registry.GetMemoryMetrics();
    // … other operations …
}
finally
{
    // Ensure resources are released; subsequent calls will throw ObjectDisposedException
    registry.Dispose();
}
```

## Notes

- The singleton instance is created lazily on first access to `Instance` and remains alive for the duration of the app domain unless `Dispose()` is invoked.
- All metric getters and snapshot methods are thread‑safe; they use atomic reads or return copies of internal state, allowing concurrent calls from multiple request threads without external synchronization.
- After `Dispose()` is called, any attempt to read a property, call a method, or obtain a snapshot will result in an `ObjectDisposedException`. It is therefore recommended to call `Dispose()` only once, during application shutdown, and to avoid using the registry thereafter.
- The request counter uses an `Interlocked.Increment` operation; it will wrap to zero only if it exceeds `Int64.MaxValue`, which is practically unreachable in normal operation.
- Memory‑related properties (`TotalMemoryMB`, `WorkingSetMB`, `PrivateMemoryMB`, `HeapSizeMB`) reflect values reported by `System.Diagnostics.Process` and the .NET runtime at the instant the property is accessed; they may fluctuate between calls.
- Thread‑pool metrics represent the state of the .NET thread pool at the time of the call; if the application does not use the thread pool explicitly, many of these values may be zero or reflect only internal runtime usage.
