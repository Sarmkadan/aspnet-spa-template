# IEventBus

The `IEventBus` interface defines the contract for domain events within the `aspnet-spa-template` project. It serves as the base type for all events that flow through the event bus, providing a common set of metadata and payload properties. Implementations of this interface represent specific occurrences in the domain (e.g., product creation, order placement, order completion) and are used by the event bus infrastructure to dispatch handlers.

## API

The interface exposes the following public members. All properties are read-only (getter only) unless otherwise noted.

| Member | Type | Description |
|--------|------|-------------|
| `EventId` | `string` | A unique identifier for the event instance. |
| `OccurredAt` | `DateTime` | The UTC timestamp when the event occurred. |
| `AggregateId` | `int?` | The identifier of the aggregate root that raised the event, if applicable; otherwise `null`. |
| `AggregateType` | `string` | The type name of the aggregate root (e.g., `"Order"`, `"Product"`). |
| `ProductId` | `int` | The identifier of the product associated with the event. Present in product-related events. |
| `ProductName` | `string` | The name of the product at the time of the event. |
| `Price` | `decimal` | The price of the product at the time of the event. |
| `OrderId` | `int` | The identifier of the order associated with the event. Present in order-related events. |
| `UserId` | `int` | The identifier of the user who triggered the event. |
| `TotalAmount` | `decimal` | The total monetary amount of the order. |
| `ItemCount` | `int` | The number of items in the order. |
| `CompletedAt` | `DateTime` | The UTC timestamp when the order was completed. |

**Remarks:**  
- Some properties are only meaningful for specific event subtypes (e.g., `CompletedAt` is only set for order completion events).  
- The interface does not define any methods; it is purely a data contract.

## Usage

### Example 1: Implementing a concrete event

```csharp
public class ProductCreatedEvent : IEventBus
{
    public string EventId { get; init; } = Guid.NewGuid().ToString();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public int? AggregateId => ProductId;
    public string AggregateType => "Product";
    public int ProductId { get; init; }
    public string ProductName { get; init; }
    public decimal Price { get; init; }
    // Other IEventBus members are not applicable; set to defaults.
    public int OrderId => 0;
    public int UserId => 0;
    public decimal TotalAmount => 0m;
    public int ItemCount => 0;
    public DateTime CompletedAt => DateTime.MinValue;
}
```

### Example 2: Publishing an event via the bus

```csharp
public class OrderService
{
    private readonly IEventBusPublisher _publisher; // assumed dependency

    public async Task PlaceOrderAsync(Order order)
    {
        // ... business logic ...

        var orderPlacedEvent = new OrderPlacedEvent
        {
            OrderId = order.Id,
            UserId = order.UserId,
            TotalAmount = order.Total,
            ItemCount = order.Items.Count,
            OccurredAt = DateTime.UtcNow,
            EventId = Guid.NewGuid().ToString(),
            AggregateId = order.Id,
            AggregateType = "Order",
            // Product-related properties are not set (defaults)
        };

        await _publisher.PublishAsync(orderPlacedEvent);
    }
}
```

## Notes

- **Thread safety:** The `IEventBus` interface itself is immutable by design (all properties are read-only). Implementations should ensure that property values are set during construction and never modified afterward. Concurrent access to the same event instance is safe as long as the underlying types (e.g., `string`, `DateTime`, `int`) are immutable.
- **Nullability:** `AggregateId` is nullable (`int?`). Consumers must check for `null` before using it. Other properties are non-nullable; however, implementors may set them to default values (e.g., `0` for `int`, `string.Empty` for `string`) when they are not applicable. It is recommended to use meaningful defaults or throw in the getter if a property is accessed in an invalid context.
- **Duplicate property names:** The interface defines `ProductId`, `ProductName`, and `Price` only once each. The repeated listing in the original specification is a documentation artifact; the actual C# interface contains each property exactly once.
- **Event identity:** `EventId` should be globally unique. Using `Guid.NewGuid().ToString()` is the recommended approach.
- **Serialization:** When serializing events (e.g., to JSON), all properties are included. Implementations should use `System.Text.Json` or Newtonsoft.Json attributes as needed for round-trip support (see related commits on `EventBusImplementationJson` and `DomainEventHandlersJson`).
