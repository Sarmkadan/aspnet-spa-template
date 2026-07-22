#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Collections.Concurrent;
using System.Threading.Channels;

namespace AspNetSpaTemplate.Services;

/// <summary>
/// Thread-safe in-process queue service that uses <see cref="Channel{T}"/> for coordination.
/// Entries are stored in a <see cref="System.Collections.Concurrent.ConcurrentDictionary{TKey,TValue}"/> and are evicted
/// automatically after a configurable retention window (default 72 hours) to
/// prevent unbounded memory growth.
/// </summary>
public sealed class SyncQueueService : ISyncQueueService, IDisposable
{
    private static readonly TimeSpan DefaultRetention = TimeSpan.FromHours(72);
    private static readonly int DefaultCapacity = 10_000;

    private readonly ConcurrentDictionary<int, SyncQueueEntry> _store = new();
    private readonly ConcurrentDictionary<string, int> _idempotency = new();
    private readonly ILogger<SyncQueueService> _logger;
    private readonly Channel<int> _completionChannel;
    private readonly CancellationTokenSource _cts = new();
    private int _sequence;
    private volatile bool _disposed;

    /// <summary>Initialises the service with a logger.</summary>
    /// <param name="logger">The logger instance.</param>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> is <see langword="null"/></exception>
    public SyncQueueService(ILogger<SyncQueueService> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
        _completionChannel = Channel.CreateBounded<int>(
            new BoundedChannelOptions(DefaultCapacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false,
            });

        // Start background consumer
        _ = ConsumeEntriesAsync(_cts.Token);
    }

    /// <summary>
    /// Background consumer that processes completed entries from the channel.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for graceful shutdown.</param>
    private async ValueTask ConsumeEntriesAsync(CancellationToken cancellationToken)
    {
        await foreach (var id in _completionChannel.Reader.ReadAllAsync(cancellationToken))
        {
            try
            {
                if (_store.TryGetValue(id, out var entry))
                {
                    lock (entry)
                    {
                        if (entry.Status == SyncEntryStatus.Pending)
                        {
                            entry.Status = SyncEntryStatus.Completed;
                            entry.ResolvedAt = DateTime.UtcNow;
                        }
                    }
                    _logger.LogDebug("Sync entry {Id} completed", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error consuming sync entry {Id}", id);
            }
        }
    }

    /// <inheritdoc/>
    public int Enqueue(
        int userId,
        string clientRequestId,
        string method,
        string relativePath,
        string? bodyJson = null)
    {
        ArgumentNullException.ThrowIfNull(clientRequestId);
        ArgumentException.ThrowIfNullOrEmpty(method);
        ArgumentException.ThrowIfNullOrEmpty(relativePath);

        // Idempotency: return the existing id if we've already seen this client key.
        if (_idempotency.TryGetValue(clientRequestId, out var existingId))
        {
            _logger.LogDebug("Duplicate sync entry ignored for clientRequestId={ClientRequestId}", clientRequestId);
            return existingId;
        }

        int id;
        SyncQueueEntry entry;

        lock (_store)
        {
            id = Interlocked.Increment(ref _sequence);
            entry = new SyncQueueEntry
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
        }

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

        lock (entry)
        {
            // Check if already completed or failed
            if (entry.Status != SyncEntryStatus.Pending)
                return false;

            entry.Status = SyncEntryStatus.Completed;
            entry.ResolvedAt = DateTime.UtcNow;
        }

        // Signal completion to background consumer
        _completionChannel.Writer.TryWrite(id);

        _logger.LogDebug("Sync entry {Id} completed", id);
        return true;
    }

    /// <inheritdoc/>
    public bool Fail(int id, string error)
    {
        ArgumentException.ThrowIfNullOrEmpty(error);

        if (!_store.TryGetValue(id, out var entry))
            return false;

        lock (entry)
        {
            // Check if already completed or failed
            if (entry.Status != SyncEntryStatus.Pending)
                return false;

            entry.Status = SyncEntryStatus.Failed;
            entry.LastError = error;
            entry.ResolvedAt = DateTime.UtcNow;
        }

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

        lock (_store)
        {
            foreach (var id in stale)
            {
                if (_store.TryRemove(id, out var removed))
                    _idempotency.TryRemove(removed.ClientRequestId, out _);
            }
        }
    }

    /// <summary>
    /// Disposes the service and cancels the background consumer.
    /// </summary>
    /// <param name="disposing">True if called from Dispose, false if called from finalizer.</param>
    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _cts.Cancel();
            _completionChannel.Writer.Complete();
            _cts.Dispose();
        }

        _disposed = true;
    }

    /// <summary>Disposes the service and cancels the background consumer.</summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}