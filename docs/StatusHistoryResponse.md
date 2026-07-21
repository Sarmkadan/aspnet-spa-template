# StatusHistoryResponse

A lightweight data transfer object used to record and transmit the history of state changes for domain entities such as orders, reviews, or products. It captures the essential details of each transition, including the timestamps and actors involved, enabling audit trails and rollback scenarios.

## API

### `Id`
An integer identifier that uniquely references this history entry within the system. This value is assigned by the persistence layer and is immutable once persisted.

### `FromStatus`
A non-null string representing the previous state of the entity before the change occurred. This field is required to reconstruct the sequence of state transitions.

### `ToStatus`
A non-null string representing the new state of the entity after the change occurred. This field is required to reconstruct the sequence of state transitions.

### `ChangedAt`
A `DateTime` indicating when the state change was recorded. This value is set by the server at the moment the change is persisted and is not user-settable.

### `ChangedBy`
An optional string indicating the user or system account responsible for the change. This field may be null when the change is initiated by an automated process or when the actor is unknown.

### `Notes`
An optional string providing additional context or justification for the state transition. This field may be null when no supplementary information is provided.

## Usage

```csharp
// Example 1: Retrieving status history for an order
var history = await orderClient.GetStatusHistoryAsync(orderId: 12345);
foreach (var entry in history)
{
    Console.WriteLine($"{entry.ChangedAt:O} {entry.FromStatus} → {entry.ToStatus} by {entry.ChangedBy ?? "system"}");
}

// Example 2: Recording a new status change
var newEntry = new StatusHistoryResponse
{
    FromStatus = "Pending",
    ToStatus = "Shipped",
    ChangedAt = DateTime.UtcNow,
    ChangedBy = User.Identity?.Name,
    Notes = "Shipped via FedEx Ground"
};
await historyClient.AddStatusHistoryAsync(orderId: 12345, entry: newEntry);
```

## Notes

`ChangedAt` is recorded in UTC and should be treated as authoritative; client clocks should not be used. The `ChangedBy` field is nullable to accommodate automated or system-initiated transitions, but callers should prefer populating it with a meaningful identifier when available. Thread safety is guaranteed by the immutable nature of the type once instantiated; concurrent reads are safe, and concurrent writes are serialized by the persistence layer.
