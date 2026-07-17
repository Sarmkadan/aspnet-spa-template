# PerformanceHelper

Utility class for measuring and analyzing the performance of operations, including execution time, memory usage, and statistical analysis of benchmark results. Designed for profiling synchronous and asynchronous code paths in .NET applications.

## API

### `MeasureTime<T>`
Measures the execution time of a synchronous operation.
- **Parameters**: `Func<T> action` – the operation to measure.
- **Returns**: A tuple `(T Result, long ElapsedMs)` containing the result of the operation and the elapsed time in milliseconds.
- **Throws**: `ArgumentNullException` if `action` is null.

### `MeasureTimeAsync<T>`
Measures the execution time of an asynchronous operation.
- **Parameters**: `Func<Task<T>> action` – the asynchronous operation to measure.
- **Returns**: A task that resolves to a tuple `(T Result, long ElapsedMs)` containing the result of the operation and the elapsed time in milliseconds.
- **Throws**: `ArgumentNullException` if `action` is null.

### `MeasureMemory<T>`
Measures the memory allocated by a synchronous operation.
- **Parameters**: `Func<T> action` – the operation to measure.
- **Returns**: A tuple `(T Result, long MemoryBytes)` containing the result of the operation and the allocated memory in bytes.
- **Throws**: `ArgumentNullException` if `action` is null.

### `GetMemoryUsageMb`
Gets the current process memory usage in megabytes.
- **Returns**: The total managed memory usage in megabytes.

### `GetPrivateMemoryMb`
Gets the current process private memory usage in megabytes.
- **Returns**: The private memory usage in megabytes.

### `BenchmarkOperation`
Runs a benchmark operation and collects performance statistics.
- **Parameters**: `Action action` – the operation to benchmark.
- **Returns**: `PerformanceStatistics` containing timing and statistical data.
- **Throws**: `ArgumentNullException` if `action` is null.

### `MeasureQueryTime<T>`
Measures the execution time of a query executed multiple times.
- **Parameters**:
  - `Func<List<T>> query` – the query to measure.
  - `int count` – the number of times to execute the query.
- **Returns**: A tuple `(List<T> Results, long ElapsedMs, int Count)` containing the results, total elapsed time, and execution count.
- **Throws**: `ArgumentNullException` if `query` is null or `count` is less than 1.

### `IsSlowOperation`
Gets or sets a flag indicating whether the current operation is considered slow.
- **Type**: `bool`
- **Remarks**: Used to influence profiling behavior or reporting.

### `Iterations`
Gets the number of iterations performed in a benchmark.
- **Type**: `int`
- **Remarks**: Valid only after a benchmark operation.

### `TotalMs`
Gets the total elapsed time in milliseconds for a benchmark.
- **Type**: `long`
- **Remarks**: Valid only after a benchmark operation.

### `AverageMs`
Gets the average execution time per iteration in milliseconds.
- **Type**: `double`
- **Remarks**: Valid only after a benchmark operation.

### `MinMs`
Gets the minimum execution time per iteration in milliseconds.
- **Type**: `long`
- **Remarks**: Valid only after a benchmark operation.

### `MaxMs`
Gets the maximum execution time per iteration in milliseconds.
- **Type**: `long`
- **Remarks**: Valid only after a benchmark operation.

### `MedianMs`
Gets the median execution time per iteration in milliseconds.
- **Type**: `long`
- **Remarks**: Valid only after a benchmark operation.

### `StdDevMs`
Gets the standard deviation of execution times in milliseconds.
- **Type**: `double`
- **Remarks**: Valid only after a benchmark operation.

### `ToString`
Returns a string summary of the performance statistics.
- **Returns**: A formatted string containing key statistics.
- **Remarks**: Includes total time, average, min, max, median, and standard deviation.

### `Profiler`
Gets the active profiler instance.
- **Type**: `Profiler`
- **Remarks**: Used for low-level profiling and diagnostics.

### `Dispose`
Releases resources used by the performance helper.
- **Remarks**: Implements `IDisposable`; call when done with profiling to clean up resources.

### `Profile`
Creates a disposable profiling scope.
- **Returns**: An `IDisposable` that ends profiling when disposed.
- **Remarks**: Use within a `using` block to profile a code block.

## Usage

### Example 1: Measuring Synchronous Execution Time
```csharp
var (result, elapsedMs) = PerformanceHelper.MeasureTime(() =>
{
    var data = new List<int>();
    for (int i = 0; i < 1000; i++) data.Add(i);
    return data;
});

Console.WriteLine($"Generated {result.Count} items in {elapsedMs}ms");
```

### Example 2: Profiling an Async Operation
```csharp
using (PerformanceHelper.Profile())
{
    var (response, elapsedMs) = await PerformanceHelper.MeasureTimeAsync(async () =>
    {
        await Task.Delay(150);
        return "Async complete";
    });

    Console.WriteLine($"{response} in {elapsedMs}ms");
}
```

## Notes

- Thread safety: All static methods are thread-safe. Instance members are not thread-safe unless explicitly documented otherwise.
- Memory measurements reflect managed heap allocations and may not include unmanaged or native allocations.
- Timing precision depends on system timer resolution and may vary across platforms.
- `MeasureQueryTime` executes the query once per iteration; ensure the query is idempotent or resets state between calls.
- `IsSlowOperation` is a user-configurable flag and does not affect measurements directly.
- Dispose `PerformanceHelper` instances when profiling is complete to avoid resource leaks.
