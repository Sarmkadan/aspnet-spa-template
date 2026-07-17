# PwaService

The `PwaService` provides high‑level operations for managing Progressive Web App features such as push notifications and background sync within the ASP.NET SPA template. It encapsulates calls to the underlying push and sync APIs, returning typed responses that indicate success or failure details.

## API

### GetStatusAsync
- **Purpose:** Retrieves the current operational status of the PWA service (e.g., whether push is supported, subscription state).
- **Parameters:** None.
- **Return Value:** `Task<PwaStatusResponse>` containing status information.
- **When it throws:** May throw `InvalidOperationException` if the service has not been initialized, or `HttpRequestException`/`TaskCanceledException` for communication failures with the push service.

### RegisterSubscriptionAsync
- **Purpose:** Registers a new push subscription for the current user.
- **Parameters:** None (registration details are obtained from the client context).
- **Return Value:** `Task<PushSubscription>` representing the created subscription.
- **When it throws:** Throws if the user has not granted permission, if the push service is unavailable (`HttpRequestException`), or if the registration payload is invalid (`ArgumentException`).

### UnsubscribeAsync
- **Purpose:** Removes the existing push subscription for the current user.
- **Parameters:** None.
- **Return Value:** `Task` that completes when the unsubscription request has been sent.
- **When it throws:** May throw if no subscription exists (`InvalidOperationException`) or if the request to the push service fails (`HttpRequestException`).

### SendPushToUserAsync
- **Purpose:** Sends a push notification to a specific user identified by their subscription.
- **Parameters:** None (target user and payload are taken from the service’s internal state).
- **Return Value:** `Task<PushDeliveryResult>` indicating delivery outcome.
- **When it throws:** Throws if the target subscription is missing (`InvalidOperationException`) or if the push service returns an error (`HttpRequestException`).

### BroadcastPushAsync
- **Purpose:** Sends a push notification to all currently subscribed users.
- **Parameters:** None.
- **Return Value:** `Task<BatchPushDeliveryResult>` summarizing results for each recipient.
- **When it throws:** Throws if the underlying push service cannot be reached (`HttpRequestException`) or if internal state is corrupted (`InvalidOperationException`).

### QueueSyncEntryAsync
- **Purpose:** Queues a background synchronization task to be executed when network connectivity is restored.
- **Parameters:** None (the work item details are supplied via the service’s configuration).
- **Return Value:** `Task<SyncQueueEntryResponse>` representing the queued entry.
- **When it throws:** Throws if the sync queue is disabled or full (`InvalidOperationException`) or if persisting the entry fails (`IOException`).

### GetPendingSyncEntriesAsync
- **Purpose:** Retrieves a list of sync entries that have been queued but not yet processed.
- **Parameters:** None.
- **Return Value:** `Task<IReadOnlyList<SyncQueueEntryResponse>>` containing pending entries.
- **When it throws:** May throw if the sync store is unavailable (`IOException`) or if the service is not initialized (`InvalidOperationException`).

### ReplaySyncQueueAsync
- **Purpose:** Processes all queued sync entries immediately, retrying any that previously failed.
- **Parameters:** None.
- **Return Value:** `Task<SyncReplayResult>` summarizing the replay outcome (successes, failures).
- **When it throws:** Throws if the replay mechanism cannot access the queue (`InvalidOperationException`) or if an individual entry throws during processing (propagated as part of the result).

## Usage

```csharp
// Example 1: Check PWA status and register for push notifications if not already subscribed
var status = await pwaService.GetStatusAsync();
if (status.IsPushSupported && !status.IsSubscribed)
{
    var subscription = await pwaService.RegisterSubscriptionAsync();
    // Store subscription.Id for later use when sending pushes
}

// Example 2: Send a push to the current user and handle the result
var deliveryResult = await pwaService.SendPushToUserAsync();
if (deliveryResult.Success)
{
    Console.WriteLine("Push delivered successfully.");
}
else
{
    Console.WriteLine($"Push failed: {deliveryResult.ErrorMessage}");
}
```

```csharp
// Example 3: Queue a background sync task and later process pending entries
await pwaService.QueueSyncEntryAsync(); // queues work based on configured handler

// Later, when the app regains connectivity or on demand:
var pending = await pwaService.GetPendingSyncEntriesAsync();
foreach (var entry in pending)
{
    Console.WriteLine($"Pending sync entry {entry.Id} will be retried.");
}

// Force immediate processing of the queue
var replayResult = await pwaService.ReplaySyncQueueAsync();
Console.WriteLine($"Replayed {replayResult.SuccessCount} entries, {replayResult.FailureCount} failed.");
```

## Notes

- The service is stateless with respect to threading; all methods are safe to invoke concurrently from multiple threads. Callers should still observe any external synchronization required for shared resources (e.g., storing a subscription ID).
- None of the methods accept a `CancellationToken`. If cancellation is needed, the caller can use `Task.WhenAny` with a delay token or apply a timeout via `Task.WithCancellation` extension patterns.
- Methods that interact with the push or sync backends may transiently fail due to network issues; implementing retry logic at the call site is recommended for production use.
- Calling any method before the service has been properly initialized (e.g., before DI registration completes) will result in an `InvalidOperationException`.
- The returned domain objects (`PwaStatusResponse`, `PushSubscription`, etc.) are immutable snapshots; modifications to them do not affect the service’s internal state.
