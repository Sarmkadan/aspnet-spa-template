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
    public ThemeService(ICacheService cache, ILogger<ThemeService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<ColourScheme> GetSchemeAsync(int userId, CancellationToken ct = default)
    {
        var key = BuildKey(userId);
        var stored = await _cache.GetAsync<ThemeEntry>(key);

        if (stored is null)
            return ColourScheme.System;

        _logger.LogDebug("Theme preference for user {UserId}: {Scheme}", userId, stored.Scheme);
        return stored.Scheme;
    }

    /// <inheritdoc/>
    public async Task SetSchemeAsync(int userId, ColourScheme scheme, CancellationToken ct = default)
    {
        if (scheme == ColourScheme.System)
        {
            await ClearSchemeAsync(userId, ct);
            return;
        }

        var key = BuildKey(userId);
        var entry = new ThemeEntry { Scheme = scheme, UpdatedAt = DateTime.UtcNow };
        await _cache.SetAsync(key, entry, PreferenceTtl);

        _logger.LogInformation("Theme preference updated for user {UserId}: {Scheme}", userId, scheme);
    }

    /// <inheritdoc/>
    public async Task ClearSchemeAsync(int userId, CancellationToken ct = default)
    {
        var key = BuildKey(userId);
        await _cache.RemoveAsync(key);

        _logger.LogDebug("Theme preference cleared for user {UserId}", userId);
    }

    private static string BuildKey(int userId) => $"{KeyPrefix}{userId}";

    /// <summary>Internal cache value model.</summary>
    private sealed class ThemeEntry
    {
        public ColourScheme Scheme { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
