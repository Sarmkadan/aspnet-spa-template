#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.DTOs;

/// <summary>
/// Request body sent by the browser when registering a Web Push subscription.
/// Mirrors the <c>PushSubscriptionJSON</c> shape returned by <c>PushManager.subscribe()</c>.
/// The client should base64url-encode the ArrayBuffers from <c>PushSubscription.getKey()</c>
/// before serialising into this DTO.
/// </summary>
public record RegisterSubscriptionRequest
{
    /// <summary>The browser-provided push service endpoint URL.</summary>
    public required string Endpoint { get; init; }

    /// <summary>
    /// The base64url-encoded P-256 Diffie-Hellman public key from the browser subscription.
    /// Obtained via <c>subscription.getKey('p256dh')</c>.
    /// </summary>
    public required string P256dhKey { get; init; }

    /// <summary>
    /// The base64url-encoded authentication secret from the browser subscription.
    /// Obtained via <c>subscription.getKey('auth')</c>.
    /// </summary>
    public required string AuthKey { get; init; }

    /// <summary>Optional user-provided friendly label for this device (e.g. "Work PC").</summary>
    public string? DeviceLabel { get; init; }
}

/// <summary>
/// Request body for deactivating a specific push subscription by its endpoint.
/// </summary>
public record UnsubscribeRequest
{
    /// <summary>The endpoint URL of the subscription to remove.</summary>
    public required string Endpoint { get; init; }
}

/// <summary>
/// The JSON-serialisable body of a Web Push notification.
/// This object is serialised, encrypted (RFC 8291), and delivered to the browser
/// push service which forwards it to the service-worker <c>push</c> event handler.
/// </summary>
public record PushNotificationPayload
{
    /// <summary>Notification title displayed in the OS notification tray.</summary>
    public required string Title { get; init; }

    /// <summary>Notification body text shown below the title.</summary>
    public required string Body { get; init; }

    /// <summary>Absolute URL of a 192×192 notification icon image. Optional.</summary>
    public string? Icon { get; init; }

    /// <summary>
    /// Absolute URL of a 96×96 badge icon shown on mobile status bars. Optional.
    /// Monochrome PNG recommended.
    /// </summary>
    public string? Badge { get; init; }

    /// <summary>
    /// Deep-link URL to open when the user clicks the notification.
    /// If omitted, the service worker should focus or open the app's main URL.
    /// </summary>
    public string? ActionUrl { get; init; }

    /// <summary>
    /// Optional tag string used to coalesce or replace previously shown notifications
    /// with the same tag (browser deduplication).
    /// </summary>
    public string? Tag { get; init; }

    /// <summary>
    /// Optional arbitrary key-value data forwarded to the service-worker push handler.
    /// Values must be JSON-serialisable.
    /// </summary>
    public Dictionary<string, object>? Data { get; init; }
}

/// <summary>
/// Server-side request for dispatching a push notification to one or more users.
/// </summary>
public record SendPushRequest
{
    /// <summary>
    /// Target user IDs to notify.
    /// Pass an empty list to broadcast to all currently active subscribers
    /// (subject to <c>PwaOptions.MaxNotificationsPerBatch</c>).
    /// </summary>
    public IReadOnlyList<int> UserIds { get; init; } = [];

    /// <summary>The notification payload to encrypt and deliver.</summary>
    public required PushNotificationPayload Payload { get; init; }
}

/// <summary>
/// Represents an API request captured by the service worker during an offline period.
/// Submitted to the server in bulk when the client reconnects and the
/// Background Sync event fires.
/// </summary>
public record SyncQueueEntryRequest
{
    /// <summary>
    /// Client-generated idempotency key (UUID v4 recommended).
    /// The server uses this to silently deduplicate entries submitted more than once.
    /// </summary>
    public required string ClientRequestId { get; init; }

    /// <summary>HTTP method of the original offline request (GET, POST, PUT, PATCH, DELETE).</summary>
    public required string HttpMethod { get; init; }

    /// <summary>Relative API path of the original request (e.g. <c>/api/orders</c>).</summary>
    public required string RelativePath { get; init; }

    /// <summary>Request body serialised as a JSON string. Omit for GET and DELETE requests.</summary>
    public string? RequestBodyJson { get; init; }

    /// <summary>
    /// Background Sync tag registered with the service worker via
    /// <c>ServiceWorkerRegistration.sync.register(tag)</c>.
    /// Defaults to <c>"default-sync"</c>.
    /// </summary>
    public string SyncTag { get; init; } = "default-sync";
}

/// <summary>
/// Read model returned to the client representing a sync queue entry
/// (pending, completed, or failed).
/// </summary>
public record SyncQueueEntryResponse
{
    /// <summary>Server-assigned entry identifier.</summary>
    public int Id { get; init; }

    /// <summary>Client-generated idempotency key.</summary>
    public required string ClientRequestId { get; init; }

    /// <summary>HTTP method of the queued request.</summary>
    public required string HttpMethod { get; init; }

    /// <summary>Relative API path of the queued request.</summary>
    public required string RelativePath { get; init; }

    /// <summary>Background Sync tag.</summary>
    public string SyncTag { get; init; } = "";

    /// <summary>Number of replay attempts made so far.</summary>
    public int RetryCount { get; init; }

    /// <summary>UTC timestamp when the entry was originally queued.</summary>
    public DateTime QueuedAt { get; init; }

    /// <summary>UTC timestamp when the entry was successfully processed, or <c>null</c> if still pending.</summary>
    public DateTime? ProcessedAt { get; init; }

    /// <summary>Error message from the most recent failed attempt, or <c>null</c> if none.</summary>
    public string? LastError { get; init; }

    /// <summary>Human-readable processing status (Pending, Processing, Completed, Failed).</summary>
    public string Status { get; init; } = "";
}

/// <summary>
/// PWA state summary for the currently authenticated user.
/// Returned by <c>GET /api/v1/pwa/status</c> and used by the client
/// to decide whether to request notification permission and subscribe.
/// </summary>
public record PwaStatusResponse
{
    /// <summary>True when the user has at least one active push subscription.</summary>
    public bool IsSubscribed { get; init; }

    /// <summary>Total number of active push subscriptions across all devices for this user.</summary>
    public int ActiveSubscriptionCount { get; init; }

    /// <summary>Number of offline sync queue entries awaiting replay.</summary>
    public int PendingSyncEntryCount { get; init; }

    /// <summary>
    /// The server VAPID public key in base64url format.
    /// Clients must pass this as <c>applicationServerKey</c> to
    /// <c>PushManager.subscribe()</c> to create a compatible subscription.
    /// </summary>
    public string VapidPublicKey { get; init; } = "";
}
