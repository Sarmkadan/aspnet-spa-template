# StatusHistory

The `StatusHistory` entity captures a single status transition for an order, recording the previous and new status, the timestamp of the change, who made the change, optional notes, and a reference to the related order.

## API

### Id
- **Purpose**: Primary key uniquely identifying the status history record.
- **Return type**: `int`
- **Parameters**: None.
- **Throws**: None (auto‑implemented property).

### OrderId
- **Purpose**: Foreign key linking the record to the `Order` whose status changed.
- **Return type**: `int`
- **Parameters**: None.
- **Throws**: None.

### FromStatus
- **Purpose**: The order status before the change occurred.
- **Return type**: `OrderStatus`
- **Parameters**: None.
- **Throws**: None; assigning a value outside the defined enum range may result in undefined behavior.

### ToStatus
- **Purpose**: The order status after the change occurred.
- **Return type**: `OrderStatus`
- **Parameters**: None.
- **Throws**: None; similar to `FromStatus`.

### ChangedAt
- **Purpose**: Date and time when the status change was recorded.
- **Return type**: `DateTime`
- **Parameters**: None.
- **Throws**: None.

### ChangedBy
- **Purpose**: Identifier of the user or system that performed the change; can be `null` if unknown.
- **Return type**: `string?`
- **Parameters**: None.
- **Throws**: None.

### Notes
- **Purpose**: Free‑form text providing additional context about the status transition; can be `null`.
- **Return type**: `string?`
- **Parameters**: None.
- **Throws**: None.

### Order
- **Purpose**: Navigation property to the related `Order` entity; may be `null` when the order is not loaded.
- **Return type**: `Order?`
- **Parameters**: None.
- **Throws**: None.

## Usage

### Creating a new status history entry
```csharp
using AspNetSpaTemplate.Models; // adjust namespace as needed

var history = new StatusHistory
{
    OrderId = 42,
    FromStatus = OrderStatus.Pending,
    ToStatus = OrderStatus.Processing,
    ChangedAt = DateTime.UtcNow,
    ChangedBy = "system",
    Notes = "Order moved to processing after payment verification",
    Order = null // will be set by EF Core when attached
};

context.StatusHistories.Add(history);
await context.SaveChangesAsync();
```

### Reading status history for an order
```csharp
using AspNetSpaTemplate.Models;
using Microsoft.EntityFrameworkCore;

// Assume ctx is an instantiated DbContext
var orderId = 12;

var histories = await ctx.StatusHistories
    .Where(sh => sh.OrderId == orderId)
    .OrderBy(sh => sh.ChangedAt)
    .ToListAsync();

foreach (var h in histories)
{
    Console.WriteLine($"{h.ChangedAt:u} | {h.FromStatus} → {h.ToStatus} by {h.ChangedBy ?? "unknown"}");
    if (!string.IsNullOrWhiteSpace(h.Notes))
    {
        Console.WriteLine($"  Note: {h.Notes}");
    }
}
```

## Notes
- The class contains no custom validation; callers must ensure `FromStatus` and `ToStatus` are valid `OrderStatus` values.
- `ChangedBy` and `Notes` may be set to `null`; consuming code should handle null values appropriately.
- The `Order` navigation property can be `null` if the entity is queried without eager or explicit loading; accessing its members without a null check will throw a `NullReferenceException`.
- All members are simple mutable properties with no internal synchronization, so the type is **not thread‑safe**. Concurrent reads are safe, but concurrent writes to the same instance from multiple threads require external synchronization.
- `Id` and `OrderId` are intended to be populated by the database; manually assigning them may conflict with identity generation strategies.
- `ChangedAt` expects a `DateTime` value; using `DateTimeKind.Unspecified` may lead to unexpected comparisons with UTC values stored in the database.
