# ISyncQueueService

Defines the contract for a queued synchronization request that captures the details of an incoming HTTP request and tracks its processing state within the background sync worker.

## API

| Member | Purpose | Parameters | Return Value | Exceptions |
|--------|---------|------------|--------------|------------|
| `Id` | Unique identifier for the queue entry. | None | `int` | None |
| `UserId` | Identifier of the user who originated the request. | None | `int` | None |
| `ClientRequestId` | Correlation ID supplied by the client to trace the request across services. | None | `string` | None |
| `Method` | HTTP verb of the original request (e.g., `GET`, `POST`, `PUT`, `DELETE`). | None | `string` | None |
| `RelativePath` | Path of the request relative to the application root (excluding query string). | None | `string` | None |
| `BodyJson` | Optional JSON‑serialized request body; `null` for requests without a body (e.g., `GET`). | None | `string?` | None |
| `Status` | Current processing state of the entry; see `SyncEntryStatus` enumeration. | None | `SyncEntryStatus` | None |
| `LastError` | Error message if the last processing attempt failed; `null` when successful or not yet attempted. | None | `string?` | None |
| `QueuedAt` | UTC timestamp indicating when the entry was added to the queue. | None | `DateTime` | None |
| `ResolvedAt` | UTC timestamp indicating when processing completed (success or failure); `null` while still pending or in progress. | None | `DateTime?` | None |

## Usage

### Creating a new queue entry
```csharp
// Assuming a concrete implementation SyncQueueEntry : ISyncQueueService
var entry = new SyncQueueEntry
{
    Id = await _queueRepository.GetNextIdAsync(),
    UserId = currentUser.Id,
    ClientRequestId = HttpContext.TraceIdentifier,
    Method = Request.Method,
    RelativePath = Request.Path.Value,
    BodyJson = await ReadBodyAsJsonAsync(),
    Status = SyncEntryStatus.Queued,
    LastError = null,
    QueuedAt = DateTime.UtcNow,
    ResolvedAt = null
};

await _queueRepository.AddAsync(entry);
```

### Inspecting an entry’s state
```csharp
var entry = await _queueRepository.GetByIdAsync(queueId);

if (entry.Status == SyncEntryStatus.Completed && entry.ResolvedAt.HasValue)
{
    // Successfully processed; optionally deserialize BodyJson for further use.
    var payload = entry.BodyJson != null ? JsonSerializer.Deserialize<MyDto>(entry.BodyJson) : null;
    ProcessResult(payload);
}
else if (entry.Status == SyncEntryStatus.Failed)
{
    // Log or surface the error recorded in LastError.
    _logger.LogWarning("Sync entry {Id} failed: {Error}", entry.Id, entry.LastError);
}
else
{
    // Still queued or processing; consider retrying later.
}
```

## Notes

- `BodyJson` and `LastError` are nullable to accommodate requests without a payload and successful processing runs, respectively. Consumers should check for `null` before attempting to deserialize or display these values.
- `ResolvedAt` remains `null` until the worker finishes processing the entry, irrespective of outcome; therefore a non‑null value indicates a terminal state (`Completed` or `Failed`).
- The type does not enforce any invariants (e.g., `Id` > 0, valid `Method` values) – validation must be performed by the caller or the persisting layer.
- Properties are simple get/set members with no internal locking; concurrent reads or writes from multiple threads are not thread‑safe. External synchronization (e.g., locking around the repository or using immutable snapshots) is required when the instance is shared across threads. 
- Once an entry is persisted, treating the properties as immutable after creation helps avoid race conditions; mutating `Status`, `LastError`, or `ResolvedAt` should be done exclusively by the background worker.
