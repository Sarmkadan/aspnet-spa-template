#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.Services;

/// <summary>
/// Manages in-memory offline sync queue entries submitted by the service worker
/// when the client reconnects after an offline period.
/// </summary>
public interface ISyncQueueService
{
    /// <summary>
    /// Enqueues a sync entry captured by the service worker while offline.
    /// Duplicate entries (same <paramref name="clientRequestId"/>) are silently ignored.
    /// </summary>
    /// <param name="userId">Owner of the entry.</param>
    /// <param name="clientRequestId">Client-generated idempotency key (UUID).</param>
    /// <param name="method">HTTP method of the original request.</param>
    /// <param name="relativePath">Relative API path (e.g. <c>/api/orders</c>).</param>
    /// <param name="bodyJson">Optional request body as a JSON string.</param>
    /// <returns>The assigned server-side identifier for the new entry.</returns>
    int Enqueue(int userId, string clientRequestId, string method, string relativePath, string? bodyJson = null);

    /// <summary>Returns all pending sync entries for the given user.</summary>
    IReadOnlyList<SyncQueueEntry> GetPending(int userId);

    /// <summary>Marks the entry with the given server-side id as completed.</summary>
    /// <returns><c>true</c> when the entry was found and updated; <c>false</c> otherwise.</returns>
    bool Complete(int id);

    /// <summary>Marks the entry with the given server-side id as failed, recording an error message.</summary>
    /// <returns><c>true</c> when the entry was found and updated; <c>false</c> otherwise.</returns>
    bool Fail(int id, string error);

    /// <summary>
    /// Returns the total count of pending entries for the given user.
    /// Used by <c>GET /api/v1/pwa/status</c> to populate <c>pendingSyncEntryCount</c>.
    /// </summary>
    int PendingCount(int userId);
}

/// <summary>Status of a sync queue entry.</summary>
public enum SyncEntryStatus
{
    /// <summary>Waiting to be replayed.</summary>
    Pending = 0,

    /// <summary>Successfully replayed.</summary>
    Completed = 1,

    /// <summary>Replay failed after the maximum number of retries.</summary>
    Failed = 2
}

/// <summary>In-memory representation of an offline sync queue entry.</summary>
public sealed class SyncQueueEntry
{
    /// <summary>Server-assigned identifier.</summary>
    public int Id { get; set; }

    /// <summary>User who owns the entry.</summary>
    public int UserId { get; set; }

    /// <summary>Client-generated idempotency key.</summary>
    public string ClientRequestId { get; set; } = string.Empty;

    /// <summary>HTTP method of the queued request.</summary>
    public string Method { get; set; } = string.Empty;

    /// <summary>Relative API path.</summary>
    public string RelativePath { get; set; } = string.Empty;

    /// <summary>Optional serialised request body.</summary>
    public string? BodyJson { get; set; }

    /// <summary>Current processing status.</summary>
    public SyncEntryStatus Status { get; set; } = SyncEntryStatus.Pending;

    /// <summary>Error message from the most recent failed attempt.</summary>
    public string? LastError { get; set; }

    /// <summary>UTC timestamp when the entry was enqueued.</summary>
    public DateTime QueuedAt { get; set; } = DateTime.UtcNow;

    /// <summary>UTC timestamp when the entry was completed or permanently failed.</summary>
    public DateTime? ResolvedAt { get; set; }
}
