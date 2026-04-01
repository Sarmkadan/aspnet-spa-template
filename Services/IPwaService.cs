#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.DTOs;
using AspNetSpaTemplate.Models;

namespace AspNetSpaTemplate.Services;

/// <summary>
/// Contract for the PWA subsystem: Web Push subscription management,
/// encrypted notification delivery, and offline sync queue operations.
/// <para>
/// Implementations are registered as <b>scoped</b> services so they share the
/// per-request EF Core <c>DbContext</c> lifetime and participate in ambient
/// unit-of-work patterns.
/// </para>
/// </summary>
public interface IPwaService
{
    /// <summary>
    /// Returns a PWA status summary for the specified user, including the server VAPID
    /// public key that the browser needs when calling <c>PushManager.subscribe()</c>.
    /// </summary>
    /// <param name="userId">The owning user's primary key.</param>
    /// <param name="ct">Propagates cancellation to async operations.</param>
    Task<PwaStatusResponse> GetStatusAsync(int userId, CancellationToken ct = default);

    /// <summary>
    /// Persists a new Web Push subscription for the given user device.
    /// If a record with the same endpoint already exists it is reactivated and its
    /// encryption keys are refreshed; otherwise a new record is created.
    /// </summary>
    /// <param name="userId">The owning user's primary key.</param>
    /// <param name="request">Browser-supplied subscription details.</param>
    /// <param name="userAgent">Optional <c>User-Agent</c> header value captured for diagnostics.</param>
    /// <param name="ct">Propagates cancellation to async operations.</param>
    /// <returns>The persisted <see cref="PushSubscription"/> entity.</returns>
    Task<PushSubscription> RegisterSubscriptionAsync(
        int userId,
        RegisterSubscriptionRequest request,
        string? userAgent,
        CancellationToken ct = default);

    /// <summary>
    /// Deactivates the push subscription identified by <paramref name="endpoint"/> for the
    /// given user. Silently no-ops if the subscription does not exist or is already inactive.
    /// </summary>
    /// <param name="userId">The owning user's primary key.</param>
    /// <param name="endpoint">The push service endpoint URL to deactivate.</param>
    /// <param name="ct">Propagates cancellation to async operations.</param>
    Task UnsubscribeAsync(int userId, string endpoint, CancellationToken ct = default);

    /// <summary>
    /// Encrypts and delivers <paramref name="payload"/> to every active push subscription
    /// held by <paramref name="userId"/>. Subscriptions that respond with HTTP 410 Gone
    /// are automatically deactivated.
    /// </summary>
    /// <param name="userId">The target user's primary key.</param>
    /// <param name="payload">The notification body to encrypt and deliver.</param>
    /// <param name="ct">Propagates cancellation to async operations.</param>
    /// <returns>Per-user delivery summary.</returns>
    Task<PushDeliveryResult> SendPushToUserAsync(
        int userId,
        PushNotificationPayload payload,
        CancellationToken ct = default);

    /// <summary>
    /// Broadcasts <paramref name="payload"/> to every active subscription of each user in
    /// <paramref name="userIds"/>. When <paramref name="userIds"/> is empty the operation
    /// targets all currently active subscribers, capped at
    /// <c>PwaOptions.MaxNotificationsPerBatch</c>.
    /// </summary>
    /// <param name="userIds">
    /// Explicit target user list. Pass an empty list for a full broadcast.
    /// </param>
    /// <param name="payload">The notification body to encrypt and deliver.</param>
    /// <param name="ct">Propagates cancellation to async operations.</param>
    /// <returns>Aggregate delivery summary across all targeted subscriptions.</returns>
    Task<BatchPushDeliveryResult> BroadcastPushAsync(
        IReadOnlyList<int> userIds,
        PushNotificationPayload payload,
        CancellationToken ct = default);

    /// <summary>
    /// Enqueues an offline API request captured by the service worker for later replay.
    /// The operation is idempotent: submitting the same
    /// <see cref="SyncQueueEntryRequest.ClientRequestId"/> more than once returns the
    /// existing entry without creating a duplicate.
    /// </summary>
    /// <param name="userId">The user who performed the offline action.</param>
    /// <param name="request">Captured request details from the service worker.</param>
    /// <param name="ct">Propagates cancellation to async operations.</param>
    /// <returns>The queued (or existing) entry read model.</returns>
    Task<SyncQueueEntryResponse> QueueSyncEntryAsync(
        int userId,
        SyncQueueEntryRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Returns all sync-queue entries for <paramref name="userId"/> that are in the
    /// <see cref="SyncEntryStatus.Pending"/> state.
    /// </summary>
    /// <param name="userId">The owning user's primary key.</param>
    /// <param name="ct">Propagates cancellation to async operations.</param>
    Task<IReadOnlyList<SyncQueueEntryResponse>> GetPendingSyncEntriesAsync(
        int userId,
        CancellationToken ct = default);

    /// <summary>
    /// Replays all eligible pending sync entries for <paramref name="userId"/> against the
    /// internal API. Entries that fail are rescheduled with exponential back-off up to
    /// <c>PwaOptions.MaxSyncRetries</c>, after which they are permanently marked
    /// <see cref="SyncEntryStatus.Failed"/>.
    /// </summary>
    /// <param name="userId">The owning user's primary key.</param>
    /// <param name="ct">Propagates cancellation to async operations.</param>
    /// <returns>Summary of the replay run.</returns>
    Task<SyncReplayResult> ReplaySyncQueueAsync(int userId, CancellationToken ct = default);
}

/// <summary>
/// Result of delivering a Web Push notification to all active subscriptions of a single user.
/// </summary>
/// <param name="UserId">The target user's primary key.</param>
/// <param name="Delivered">Number of subscription endpoints that accepted the message (HTTP 201/200).</param>
/// <param name="Failed">Number of endpoints that returned a transient error.</param>
/// <param name="Deactivated">
/// Number of subscriptions invalidated because the push service returned HTTP 410 Gone,
/// indicating the browser has revoked the subscription.
/// </param>
public record PushDeliveryResult(int UserId, int Delivered, int Failed, int Deactivated);

/// <summary>
/// Aggregate result of a broadcast push-notification run across multiple users.
/// </summary>
/// <param name="TotalUsers">Total number of distinct users targeted in the batch.</param>
/// <param name="TotalDelivered">Sum of accepted deliveries across all subscriptions.</param>
/// <param name="TotalFailed">Sum of transient delivery failures across all subscriptions.</param>
/// <param name="TotalDeactivated">
/// Total subscriptions removed as a result of 410 Gone responses during this batch.
/// </param>
public record BatchPushDeliveryResult(
    int TotalUsers,
    int TotalDelivered,
    int TotalFailed,
    int TotalDeactivated);

/// <summary>
/// Summary of a single offline sync queue replay run for one user.
/// </summary>
/// <param name="Processed">Total entries examined in this run (eligible for replay).</param>
/// <param name="Succeeded">Entries successfully replayed and marked <see cref="SyncEntryStatus.Completed"/>.</param>
/// <param name="Failed">Entries that encountered an error and were rescheduled or abandoned.</param>
/// <param name="Skipped">Entries bypassed because they were not yet due for retry.</param>
public record SyncReplayResult(int Processed, int Succeeded, int Failed, int Skipped);
