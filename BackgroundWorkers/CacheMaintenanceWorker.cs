// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Caching;

namespace AspNetSpaTemplate.BackgroundWorkers;

/// <summary>
/// Background worker for cache maintenance and cleanup.
/// Periodically monitors cache health, evicts stale entries, and reports statistics.
/// Prevents cache memory bloat and ensures cache effectiveness.
/// </summary>
public class CacheMaintenanceWorker : IBackgroundTask
{
    public string TaskName => "CacheMaintenanceWorker";
    public TimeSpan? ExecutionInterval => TimeSpan.FromMinutes(5);

    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheMaintenanceWorker> _logger;
    private DateTime? _lastExecutedAt;
    private int _executionCount;

    public CacheMaintenanceWorker(
        ICacheService cacheService,
        ILogger<CacheMaintenanceWorker> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Starting cache maintenance");

            // Get cache statistics
            var stats = await _cacheService.GetStatisticsAsync();

            // Log cache health
            var hitRatePercentage = stats.HitRate * 100;
            _logger.LogInformation(
                $"Cache Statistics: {stats.ItemCount} items, {hitRatePercentage:F1}% hit rate, " +
                $"{stats.CacheHits}/{stats.TotalRequests} hits");

            // Alert if hit rate is low (less than 50%)
            if (stats.HitRate < 0.5 && stats.TotalRequests > 100)
            {
                _logger.LogWarning(
                    $"Low cache hit rate detected: {hitRatePercentage:F1}%. " +
                    $"Consider reviewing cache TTL values or cache key strategy");
            }

            // Clean up specific cache patterns that might be stale
            await CleanupStaleData(cancellationToken);

            _lastExecutedAt = DateTime.UtcNow;
            _executionCount++;

            _logger.LogInformation("Cache maintenance completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CacheMaintenanceWorker");
        }
    }

    public BackgroundTaskStatus GetStatus()
    {
        return new BackgroundTaskStatus
        {
            TaskName = TaskName,
            LastExecutedAt = _lastExecutedAt,
            ExecutionCount = _executionCount
        };
    }

    /// <summary>
    /// Removes cache entries for data that's likely stale or no longer needed.
    /// </summary>
    private async Task CleanupStaleData(CancellationToken cancellationToken)
    {
        try
        {
            // Clean up temporary cache entries (older than 1 hour)
            // Pattern: temp:*
            _logger.LogDebug("Cleaning up temporary cache entries");
            await _cacheService.RemoveByPatternAsync("temp:*");

            // Clean up rate limit entries (expired tokens)
            _logger.LogDebug("Cleaning up expired rate limit data");
            await _cacheService.RemoveByPatternAsync("ratelimit:*");

            // Clean up session data (assuming 1-hour session TTL)
            // In real scenario, would be more selective based on expiration
            _logger.LogDebug("Analyzing session cache health");

            _logger.LogInformation("Stale data cleanup completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up stale cache data");
        }
    }
}

/// <summary>
/// Cache health monitoring interface.
/// Can be extended to send alerts to monitoring systems.
/// </summary>
public interface ICacheHealthMonitor
{
    Task<CacheHealthReport> GetHealthReportAsync();
    Task<bool> IsCacheHealthyAsync();
}

/// <summary>
/// Cache health report data.
/// </summary>
public class CacheHealthReport
{
    public bool IsHealthy { get; set; }
    public double HitRate { get; set; }
    public long ItemCount { get; set; }
    public long MemoryUsageBytes { get; set; }
    public int? WarningCount { get; set; }
    public List<string> Warnings { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Cache health monitor implementation.
/// </summary>
public class DefaultCacheHealthMonitor : ICacheHealthMonitor
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<DefaultCacheHealthMonitor> _logger;

    private const double MinimumAcceptableHitRate = 0.4;
    private const long MaximumAcceptableMemoryBytes = 1_000_000_000; // 1 GB

    public DefaultCacheHealthMonitor(
        ICacheService cacheService,
        ILogger<DefaultCacheHealthMonitor> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<CacheHealthReport> GetHealthReportAsync()
    {
        var stats = await _cacheService.GetStatisticsAsync();
        var warnings = new List<string>();

        // Check hit rate
        if (stats.TotalRequests > 100 && stats.HitRate < MinimumAcceptableHitRate)
        {
            warnings.Add($"Low cache hit rate: {stats.HitRate:P}");
        }

        // Check memory usage
        if (stats.ApproximateMemoryUsage > MaximumAcceptableMemoryBytes)
        {
            warnings.Add($"High cache memory usage: {stats.ApproximateMemoryUsage / 1_000_000}MB");
        }

        // Check for too many items (might indicate memory leak)
        if (stats.ItemCount > 100_000)
        {
            warnings.Add($"Excessive cache items: {stats.ItemCount}");
        }

        var isHealthy = warnings.Count == 0;
        return new CacheHealthReport
        {
            IsHealthy = isHealthy,
            HitRate = stats.HitRate,
            ItemCount = stats.ItemCount,
            MemoryUsageBytes = stats.ApproximateMemoryUsage,
            WarningCount = warnings.Count,
            Warnings = warnings
        };
    }

    public async Task<bool> IsCacheHealthyAsync()
    {
        var report = await GetHealthReportAsync();
        return report.IsHealthy;
    }
}
