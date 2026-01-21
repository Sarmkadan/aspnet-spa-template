#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.Models;

/// <summary>
/// Represents a browser Web Push subscription for a user device.
/// Stores the VAPID endpoint URL and encryption keys required to deliver
/// push notifications via the Web Push Protocol (RFC 8030).
/// One user may hold multiple subscriptions — one per browser/device.
/// </summary>
public sealed class PushSubscription
{
    /// <summary>Gets or sets the primary key.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the ID of the owning user.</summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the push service endpoint URL provided by the browser.
    /// This value is unique per subscription and is the HTTP delivery target
    /// for Web Push messages (RFC 8030 §5).
    /// </summary>
    public string Endpoint { get; set; } = "";

    /// <summary>
    /// Gets or sets the P-256 Diffie-Hellman public key from the browser subscription
    /// (base64url-encoded). Used together with <see cref="AuthKey"/> for payload
    /// encryption per RFC 8291.
    /// Corresponds to <c>PushSubscription.getKey('p256dh')</c> on the client.
    /// </summary>
    public string P256dhKey { get; set; } = "";

    /// <summary>
    /// Gets or sets the authentication secret from the browser subscription
    /// (base64url-encoded). Combined with <see cref="P256dhKey"/> for ECDH-based
    /// payload encryption.
    /// Corresponds to <c>PushSubscription.getKey('auth')</c> on the client.
    /// </summary>
    public string AuthKey { get; set; } = "";

    /// <summary>
    /// Gets or sets a user-provided friendly label for this device (e.g. "Home laptop").
    /// Optional; used for display in subscription management UIs.
    /// </summary>
    public string? DeviceLabel { get; set; }

    /// <summary>
    /// Gets or sets the User-Agent string captured at subscription time.
    /// Stored for diagnostics and subscription management; not used in delivery.
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>Gets or sets whether this subscription is currently active.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Gets or sets the UTC timestamp of the last successful push delivery to this endpoint.
    /// <c>null</c> when the subscription has never received a notification.
    /// </summary>
    public DateTime? LastActiveAt { get; set; }

    /// <summary>Gets or sets the UTC timestamp when the subscription record was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the UTC timestamp when the subscription record was last updated.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Navigation property — the owning user.</summary>
    public User? User { get; set; }

    /// <summary>
    /// Refreshes the <see cref="LastActiveAt"/> timestamp after a successful push delivery.
    /// </summary>
    public void RecordDelivery()
    {
        LastActiveAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks this subscription as inactive.
    /// Typically called after the push service returns HTTP 410 Gone, indicating
    /// the browser has unsubscribed or the endpoint is no longer valid.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Represents an API request captured offline by the service worker while the client
/// had no network connectivity. Entries are replayed against the server API when
/// connectivity is restored and a Background Sync event fires (WICG Background Sync spec).
/// </summary>
public sealed class SyncQueueEntry
{
    /// <summary>Gets or sets the primary key.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the ID of the user who queued the request.</summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets a client-generated idempotency key (UUID v4 recommended).
    /// Prevents duplicate replay if the same entry is submitted more than once
    /// due to service worker retry logic.
    /// </summary>
    public string ClientRequestId { get; set; } = "";

    /// <summary>Gets or sets the HTTP method of the original offline request (GET, POST, PUT, PATCH, DELETE).</summary>
    public string HttpMethod { get; set; } = "";

    /// <summary>Gets or sets the relative API path to replay (e.g. <c>/api/orders</c>).</summary>
    public string RelativePath { get; set; } = "";

    /// <summary>Gets or sets the request body serialised as a JSON string, if the method carries a body.</summary>
    public string? RequestBodyJson { get; set; }

    /// <summary>Gets or sets JSON-serialised additional headers to forward on replay.</summary>
    public string? RequestHeadersJson { get; set; }

    /// <summary>
    /// Gets or sets the Background Sync tag registered with the service worker
    /// via <c>ServiceWorkerRegistration.sync.register(tag)</c>.
    /// </summary>
    public string SyncTag { get; set; } = "default-sync";

    /// <summary>Gets or sets the current number of replay attempts made for this entry.</summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Gets or sets the UTC time after which the next retry attempt may be made.
    /// <c>null</c> means the entry is eligible for immediate processing.
    /// </summary>
    public DateTime? NextRetryAfter { get; set; }

    /// <summary>Gets or sets the UTC timestamp when this entry was first queued by the service worker.</summary>
    public DateTime QueuedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the UTC timestamp when this entry was successfully replayed.
    /// <c>null</c> for entries that have not yet completed.
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>Gets or sets the error message from the most recent failed replay attempt, if any.</summary>
    public string? LastError { get; set; }

    /// <summary>Gets or sets the current processing state of this entry.</summary>
    public SyncEntryStatus Status { get; set; } = SyncEntryStatus.Pending;

    /// <summary>Navigation property — the owning user.</summary>
    public User? User { get; set; }
}

/// <summary>Processing states for an offline sync queue entry.</summary>
public enum SyncEntryStatus
{
    /// <summary>Entry is waiting to be processed by the sync engine.</summary>
    Pending,

    /// <summary>Entry is currently being replayed against the API.</summary>
    Processing,

    /// <summary>Entry was successfully replayed and the response accepted.</summary>
    Completed,

    /// <summary>All retry attempts have been exhausted; the entry is permanently abandoned.</summary>
    Failed
}
