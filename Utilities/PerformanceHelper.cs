// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;

namespace AspNetSpaTemplate.Utilities;

/// <summary>
/// Helper for performance monitoring and benchmarking.
/// Measures execution time, memory usage, and identifies bottlenecks.
/// </summary>
public static class PerformanceHelper
{
    /// <summary>
    /// Measures execution time of a synchronous operation.
    /// Returns operation result and elapsed time.
    /// </summary>
    public static (T Result, long ElapsedMs) MeasureTime<T>(Func<T> operation, string operationName = "Operation")
    {
        var stopwatch = Stopwatch.StartNew();
        var result = operation();
        stopwatch.Stop();

        return (result, stopwatch.ElapsedMilliseconds);
    }

    /// <summary>
    /// Measures execution time of an asynchronous operation.
    /// </summary>
    public static async Task<(T Result, long ElapsedMs)> MeasureTimeAsync<T>(Func<Task<T>> operation, string operationName = "Operation")
    {
        var stopwatch = Stopwatch.StartNew();
        var result = await operation();
        stopwatch.Stop();

        return (result, stopwatch.ElapsedMilliseconds);
    }

    /// <summary>
    /// Measures memory allocation by an operation.
    /// Forces garbage collection before/after for accuracy.
    /// </summary>
    public static (T Result, long MemoryBytes) MeasureMemory<T>(Func<T> operation)
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        var initialMemory = GC.GetTotalMemory(true);
        var result = operation();
        var finalMemory = GC.GetTotalMemory(false);

        return (result, finalMemory - initialMemory);
    }

    /// <summary>
    /// Gets current process memory usage in MB.
    /// Includes working set (physical memory).
    /// </summary>
    public static double GetMemoryUsageMb()
    {
        using (var process = Process.GetCurrentProcess())
        {
            return process.WorkingSet64 / (1024.0 * 1024.0);
        }
    }

    /// <summary>
    /// Gets private memory usage (allocated to this process) in MB.
    /// </summary>
    public static double GetPrivateMemoryMb()
    {
        using (var process = Process.GetCurrentProcess())
        {
            return process.PrivateMemorySize64 / (1024.0 * 1024.0);
        }
    }

    /// <summary>
    /// Performs operation multiple times and returns timing statistics.
    /// Useful for benchmarking and understanding performance under load.
    /// </summary>
    public static PerformanceStatistics BenchmarkOperation(Action operation, int iterations = 100)
    {
        var timings = new List<long>();

        // Warmup
        for (int i = 0; i < 5; i++)
            operation();

        // Benchmark
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            var iterationWatch = Stopwatch.StartNew();
            operation();
            iterationWatch.Stop();
            timings.Add(iterationWatch.ElapsedMilliseconds);
        }
        stopwatch.Stop();

        return new PerformanceStatistics
        {
            Iterations = iterations,
            TotalMs = stopwatch.ElapsedMilliseconds,
            AverageMs = timings.Average(),
            MinMs = timings.Min(),
            MaxMs = timings.Max(),
            MedianMs = timings.OrderBy(x => x).ElementAt(timings.Count / 2),
            StdDevMs = CalculateStandardDeviation(timings)
        };
    }

    /// <summary>
    /// Measures query execution time and result count.
    /// Useful for database query optimization.
    /// </summary>
    public static (List<T> Results, long ElapsedMs, int Count) MeasureQueryTime<T>(Func<List<T>> query)
    {
        var stopwatch = Stopwatch.StartNew();
        var results = query();
        stopwatch.Stop();

        return (results, stopwatch.ElapsedMilliseconds, results.Count);
    }

    /// <summary>
    /// Calculates standard deviation for a dataset.
    /// Used for performance statistics analysis.
    /// </summary>
    private static double CalculateStandardDeviation(List<long> values)
    {
        if (values.Count < 2)
            return 0;

        var average = values.Average();
        var sumOfSquares = values.Sum(v => Math.Pow(v - average, 2));
        return Math.Sqrt(sumOfSquares / (values.Count - 1));
    }

    /// <summary>
    /// Reports if operation is slow (exceeds threshold).
    /// </summary>
    public static bool IsSlowOperation(long elapsedMs, long thresholdMs = 1000)
    {
        return elapsedMs > thresholdMs;
    }
}

/// <summary>
/// Performance statistics from benchmarking.
/// </summary>
public class PerformanceStatistics
{
    public int Iterations { get; set; }
    public long TotalMs { get; set; }
    public double AverageMs { get; set; }
    public long MinMs { get; set; }
    public long MaxMs { get; set; }
    public long MedianMs { get; set; }
    public double StdDevMs { get; set; }

    public override string ToString()
    {
        return $"Benchmark: {Iterations} iterations in {TotalMs}ms | " +
               $"Avg: {AverageMs:F2}ms | Min: {MinMs}ms | Max: {MaxMs}ms | " +
               $"Median: {MedianMs}ms | StdDev: {StdDevMs:F2}ms";
    }
}

/// <summary>
/// Helper for profiling code blocks.
/// Useful for identifying hotspots in production.
/// </summary>
public class Profiler : IDisposable
{
    private readonly string _name;
    private readonly ILogger _logger;
    private readonly Stopwatch _stopwatch;
    private readonly long _thresholdMs;

    public Profiler(string name, ILogger logger, long thresholdMs = 100)
    {
        _name = name;
        _logger = logger;
        _thresholdMs = thresholdMs;
        _stopwatch = Stopwatch.StartNew();
    }

    public void Dispose()
    {
        _stopwatch.Stop();
        if (_stopwatch.ElapsedMilliseconds > _thresholdMs)
        {
            _logger.LogWarning($"[PERF] {_name} took {_stopwatch.ElapsedMilliseconds}ms (threshold: {_thresholdMs}ms)");
        }
        else
        {
            _logger.LogDebug($"[PERF] {_name} took {_stopwatch.ElapsedMilliseconds}ms");
        }
    }
}

/// <summary>
/// Extension methods for easy performance measurement.
/// </summary>
public static class PerformanceExtensions
{
    public static IDisposable Profile(this ILogger logger, string operationName, long thresholdMs = 100)
    {
        return new Profiler(operationName, logger, thresholdMs);
    }
}
