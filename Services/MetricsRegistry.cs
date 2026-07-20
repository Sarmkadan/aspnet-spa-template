#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Diagnostics;
using System.Threading;

namespace AspNetSpaTemplate.Services;

/// <summary>
/// Singleton service that collects and tracks application metrics including:
/// - Application uptime
/// - Memory usage (GC, working set)
/// - Thread pool and active thread counts
/// - Request counters
/// - Process information
///
/// This service is thread-safe and can be accessed from any controller.
/// </summary>
public sealed class MetricsRegistry : IDisposable
{
    private readonly Stopwatch _uptimeStopwatch = Stopwatch.StartNew();
    private readonly object _lock = new object();
    private long _requestCount = 0;
    private DateTimeOffset _startTime = DateTimeOffset.UtcNow;
    private bool _disposed = false;

    /// <summary>
    /// Gets the singleton instance of MetricsRegistry.
    /// </summary>
    public static MetricsRegistry Instance { get; } = new MetricsRegistry();

    /// <summary>
    /// Gets the application start time.
    /// </summary>
    public DateTimeOffset StartTime => _startTime;

    /// <summary>
    /// Gets the application uptime in milliseconds.
    /// </summary>
    public long UptimeMilliseconds
    {
        get
        {
            lock (_lock)
            {
                return _uptimeStopwatch.ElapsedMilliseconds;
            }
        }
    }

    /// <summary>
    /// Gets the total number of requests processed.
    /// </summary>
    public long RequestCount
    {
        get
        {
            lock (_lock)
            {
                return _requestCount;
            }
        }
    }

    /// <summary>
    /// Increments the request counter.
    /// </summary>
    public void IncrementRequestCount()
    {
        lock (_lock)
        {
            _requestCount++;
        }
    }

    /// <summary>
    /// Gets current memory metrics.
    /// </summary>
    public MemoryMetrics GetMemoryMetrics()
    {
        return new MemoryMetrics
        {
            TotalMemoryMB = GC.GetTotalMemory(false) / 1024 / 1024,
            WorkingSetMB = Environment.WorkingSet / 1024 / 1024,
            PrivateMemoryMB = Environment.WorkingSet / 1024 / 1024,
            Gen0Collections = GC.CollectionCount(0),
            Gen1Collections = GC.CollectionCount(1),
            Gen2Collections = GC.CollectionCount(2),
            HeapSizeMB = (long)GC.GetTotalMemory(false) / 1024 / 1024
        };
    }

    /// <summary>
    /// Gets current thread and thread pool metrics.
    /// </summary>
    public ThreadMetrics GetThreadMetrics()
    {
        return new ThreadMetrics
        {
            ActiveThreadCount = Process.GetCurrentProcess().Threads.Count,
            ThreadPoolThreadCount = ThreadPool.ThreadCount,
            ThreadPoolCompletedWorkItemCount = ThreadPool.CompletedWorkItemCount,
            ThreadPoolPendingWorkItemCount = ThreadPool.PendingWorkItemCount,
            ThreadPoolQueueLength = ThreadPool.ThreadCount // Approximation
        };
    }

    /// <summary>
    /// Gets current process metrics.
    /// </summary>
    public ProcessMetrics GetProcessMetrics()
    {
        var process = Process.GetCurrentProcess();
        return new ProcessMetrics
        {
            ProcessId = process.Id,
            ProcessName = process.ProcessName,
            MachineName = Environment.MachineName,
            StartTime = process.StartTime,
            TotalProcessorTimeSeconds = process.TotalProcessorTime.TotalSeconds,
            PrivilegedProcessorTimeSeconds = process.PrivilegedProcessorTime.TotalSeconds,
            UserProcessorTimeSeconds = process.UserProcessorTime.TotalSeconds,
            PagedMemorySizeMB = process.PagedMemorySize64 / 1024 / 1024,
            VirtualMemorySizeMB = process.VirtualMemorySize64 / 1024 / 1024,
            PrivateMemorySizeMB = process.PrivateMemorySize64 / 1024 / 1024
        };
    }

    /// <summary>
    /// Gets comprehensive metrics report.
    /// </summary>
    public MetricsReport GetMetricsReport()
    {
        return new MetricsReport
        {
            Timestamp = DateTimeOffset.UtcNow,
            UptimeMilliseconds = UptimeMilliseconds,
            RequestCount = RequestCount,
            Memory = GetMemoryMetrics(),
            Threads = GetThreadMetrics(),
            Process = GetProcessMetrics(),
            Environment = new EnvironmentMetrics
            {
                AspNetCoreEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production",
                DotNetVersion = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
                OperatingSystem = System.Runtime.InteropServices.RuntimeInformation.OSDescription,
                ProcessArchitecture = System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString(),
                MachineName = Environment.MachineName,
                CurrentManagedThreadId = Environment.CurrentManagedThreadId
            }
        };
    }

    /// <summary>
    /// Resets the request counter (useful for testing).
    /// </summary>
    public void ResetRequestCounter()
    {
        lock (_lock)
        {
            _requestCount = 0;
        }
    }

    /// <summary>
    /// Disposes the MetricsRegistry and stops the uptime stopwatch.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _uptimeStopwatch.Stop();
            _disposed = true;
        }
    }
}

/// <summary>
/// Memory metrics structure.
/// </summary>
public sealed class MemoryMetrics
{
    public long TotalMemoryMB { get; set; }
    public long WorkingSetMB { get; set; }
    public long PrivateMemoryMB { get; set; }
    public int Gen0Collections { get; set; }
    public int Gen1Collections { get; set; }
    public int Gen2Collections { get; set; }
    public long HeapSizeMB { get; set; }
}

/// <summary>
/// Thread metrics structure.
/// </summary>
public sealed class ThreadMetrics
{
    public int ActiveThreadCount { get; set; }
    public int ThreadPoolThreadCount { get; set; }
    public long ThreadPoolCompletedWorkItemCount { get; set; }
    public long ThreadPoolPendingWorkItemCount { get; set; }
    public int ThreadPoolQueueLength { get; set; }
}

/// <summary>
/// Process metrics structure.
/// </summary>
public sealed class ProcessMetrics
{
    public int ProcessId { get; set; }
    public string ProcessName { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public double TotalProcessorTimeSeconds { get; set; }
    public double PrivilegedProcessorTimeSeconds { get; set; }
    public double UserProcessorTimeSeconds { get; set; }
    public long PagedMemorySizeMB { get; set; }
    public long VirtualMemorySizeMB { get; set; }
    public long PrivateMemorySizeMB { get; set; }
}

/// <summary>
/// Environment metrics structure.
/// </summary>
public sealed class EnvironmentMetrics
{
    public string AspNetCoreEnvironment { get; set; } = string.Empty;
    public string DotNetVersion { get; set; } = string.Empty;
    public string OperatingSystem { get; set; } = string.Empty;
    public string ProcessArchitecture { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public int CurrentManagedThreadId { get; set; }
}

/// <summary>
/// Comprehensive metrics report structure.
/// </summary>
public sealed class MetricsReport
{
    public DateTimeOffset Timestamp { get; set; }
    public long UptimeMilliseconds { get; set; }
    public long RequestCount { get; set; }
    public MemoryMetrics Memory { get; set; } = new();
    public ThreadMetrics Threads { get; set; } = new();
    public ProcessMetrics Process { get; set; } = new();
    public EnvironmentMetrics Environment { get; set; } = new();
}