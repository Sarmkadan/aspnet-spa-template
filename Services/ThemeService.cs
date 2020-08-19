#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Caching;

namespace AspNetSpaTemplate.Services;

/// <summary>
/// In-memory implementation of <see cref="IThemeService"/>.
/// Preferences are stored with a 30-day TTL via <see cref="ICacheService"/>
/// and are automatically evicted without requiring a separate cleanup job.
/// </summary>
public sealed class ThemeService : IThemeService
{
    private const string KeyPrefix = "theme:user:";
    private static readonly TimeSpan PreferenceTtl = TimeSpan.FromDays(30);

    private readonly ICacheService _cache;
    private readonly ILogger<ThemeService> _logger;

    /// <summary>Initialises the service with the required dependencies.</summary>
    /// <param name="cache">The cache service.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when cache or logger is null.</exception>
    public ThemeService(ICacheService cache, ILogger<ThemeService> logger)
    {
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public async Task<ColourScheme> GetSchemeAsync(int userId, CancellationToken ct = default)
    {
        if (userId <= 0)
            throw new ArgumentOutOfRangeException(nameof(userId), "User ID must be greater than 0");

        try
        {
            var key = BuildKey(userId);
            var stored = await _cache.GetAsync<ThemeEntry>(key);

            if (stored is null)
                return ColourScheme.System;

            _logger.LogDebug("Theme preference for user {UserId}: {Scheme}", userId, stored.Scheme);
            return stored.Scheme;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error retrieving theme preference for user {UserId}", userId);
            return ColourScheme.System;
        }
    }

    /// <inheritdoc/>
    public async Task SetSchemeAsync(int userId, ColourScheme scheme, CancellationToken ct = default)
    {
        if (userId <= 0)
            throw new ArgumentOutOfRangeException(nameof(userId), "User ID must be greater than 0");

        if (scheme == ColourScheme.System)
        {
            await ClearSchemeAsync(userId, ct);
            return;
        }

        try
        {
            var key = BuildKey(userId);
            var entry = new ThemeEntry { Scheme = scheme, UpdatedAt = DateTime.UtcNow };
            await _cache.SetAsync(key, entry, PreferenceTtl);

            _logger.LogInformation("Theme preference updated for user {UserId}: {Scheme}", userId, scheme);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error setting theme preference for user {UserId}", userId);
            throw new BusinessException("Failed to save theme preference", "THEME_SAVE_FAILED", 500).WithData(ex);
        }
    }

    /// <inheritdoc/>
    public async Task ClearSchemeAsync(int userId, CancellationToken ct = default)
    {
        if (userId <= 0)
            throw new ArgumentOutOfRangeException(nameof(userId), "User ID must be greater than 0");

        try
        {
            var key = BuildKey(userId);
            await _cache.RemoveAsync(key);

            _logger.LogDebug("Theme preference cleared for user {UserId}", userId);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Error clearing theme preference for user {UserId}", userId);
            throw new BusinessException("Failed to clear theme preference", "THEME_CLEAR_FAILED", 500).WithData(ex);
        }
    }

    private static string BuildKey(int userId) => $"{KeyPrefix}{userId}";

    /// <summary>Internal cache value model.</summary>
    private sealed class ThemeEntry
    {
        public ColourScheme Scheme { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
