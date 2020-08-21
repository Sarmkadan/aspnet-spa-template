using AspNetSpaTemplate.DTOs;
using AspNetSpaTemplate.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetSpaTemplate.Services;

public sealed class PwaService : IPwaService
{
    public Task<PwaStatusResponse> GetStatusAsync(int userId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<PushSubscription> RegisterSubscriptionAsync(int userId, RegisterSubscriptionRequest request, string? userAgent, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task UnsubscribeAsync(int userId, string endpoint, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<PushDeliveryResult> SendPushToUserAsync(int userId, PushNotificationPayload payload, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<BatchPushDeliveryResult> BroadcastPushAsync(IReadOnlyList<int> userIds, PushNotificationPayload payload, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<SyncQueueEntryResponse> QueueSyncEntryAsync(int userId, SyncQueueEntryRequest request, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<SyncQueueEntryResponse>> GetPendingSyncEntriesAsync(int userId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task<SyncReplayResult> ReplaySyncQueueAsync(int userId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }
}
