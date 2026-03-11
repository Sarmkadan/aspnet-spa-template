#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;

namespace AspNetSpaTemplate.Services;

/// <summary>
/// Thread-safe in-process implementation of <see cref="ISyncQueueService"/>.
/// Entries are stored in a <see cref="ConcurrentDictionary"/> and are evicted
/// automatically after a configurable retention window (default 72 hours) to
/// prevent unbounded memory growth.
/// </summary>
public sealed class SyncQueueService : ISyncQueueService
{
    private static readonly TimeSpan DefaultRetention = TimeSpan.FromHours(72);

    private readonly ConcurrentDictionary<int, SyncQueueEntry> _store = new();
    private readonly ConcurrentDictionary<string, int> _idempotency = new();
    private readonly ILogger<SyncQueueService> _logger;
    private int _sequence;

    /// <summary>Initialises the service with a logger.</summary>
    public SyncQueueService(ILogger<SyncQueueService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public int Enqueue(
        int userId,
        string clientRequestId,
        string method,
        string relativePath,
        string? bodyJson = null)
    {
        // Idempotency: return the existing id if we've already seen this client key.
        if (_idempotency.TryGetValue(clientRequestId, out var existingId))
        {
            _logger.LogDebug("Duplicate sync entry ignored for clientRequestId={ClientRequestId}", clientRequestId);
            return existingId;
        }

        var id = Interlocked.Increment(ref _sequence);
        var entry = new SyncQueueEntry
        {
            Id = id,
            UserId = userId,
            ClientRequestId = clientRequestId,
            Method = method.ToUpperInvariant(),
            RelativePath = relativePath,
            BodyJson = bodyJson,
            Status = SyncEntryStatus.Pending,
            QueuedAt = DateTime.UtcNow
        };

        _store[id] = entry;
        _idempotency[clientRequestId] = id;

        _logger.LogInformation(
            "Offline sync entry queued: id={Id} user={UserId} {Method} {Path}",
            id, userId, method, relativePath);

        Evict();
        return id;
    }

    /// <inheritdoc/>
    public IReadOnlyList<SyncQueueEntry> GetPending(int userId) =>
        _store.Values
              .Where(e => e.UserId == userId && e.Status == SyncEntryStatus.Pending)
              .OrderBy(e => e.QueuedAt)
              .ToList();

    /// <inheritdoc/>
    public bool Complete(int id)
    {
        if (!_store.TryGetValue(id, out var entry))
            return false;

        entry.Status = SyncEntryStatus.Completed;
        entry.ResolvedAt = DateTime.UtcNow;
        _logger.LogDebug("Sync entry {Id} completed", id);
        return true;
    }

    /// <inheritdoc/>
    public bool Fail(int id, string error)
    {
        if (!_store.TryGetValue(id, out var entry))
            return false;

        entry.Status = SyncEntryStatus.Failed;
        entry.LastError = error;
        entry.ResolvedAt = DateTime.UtcNow;
        _logger.LogWarning("Sync entry {Id} failed: {Error}", id, error);
        return true;
    }

    /// <inheritdoc/>
    public int PendingCount(int userId) =>
        _store.Values.Count(e => e.UserId == userId && e.Status == SyncEntryStatus.Pending);

    /// <summary>Removes completed and failed entries older than <see cref="DefaultRetention"/>.</summary>
    private void Evict()
    {
        var cutoff = DateTime.UtcNow - DefaultRetention;
        var stale = _store.Values
            .Where(e => e.Status != SyncEntryStatus.Pending && e.ResolvedAt < cutoff)
            .Select(e => e.Id)
            .ToList();

        foreach (var id in stale)
        {
            if (_store.TryRemove(id, out var removed))
                _idempotency.TryRemove(removed.ClientRequestId, out _);
        }
    }
}
