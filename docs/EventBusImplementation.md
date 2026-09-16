# EventBusImplementation

The `EventBusImplementation` class provides an in-process implementation of `IEventBus` for single-server deployments. It stores asynchronous handlers in memory by event type, protects subscription state with a lock, and invokes matching handlers sequentially in subscription order. Each handler is retried up to three times with exponential backoff and jitter; a handler that exhausts its retries is sent to an `IDeadLetterSink` without preventing later handlers from receiving the event.

The same source file also defines `EventBusExtensions`, a set of convenience methods for constructing and publishing common domain events.

## API

All generic event methods require `TEvent` to derive from `DomainEvent`.

### EventBusImplementation

| Member | Return type | Description |
|--------|-------------|-------------|
| `EventBusImplementation(ILogger<EventBusImplementation> logger)` | Constructor | Creates a bus that uses the supplied logger and a default logging-based dead-letter sink. Throws `ArgumentNullException` when `logger` is `null`. |
| `EventBusImplementation(ILogger<EventBusImplementation> logger, IDeadLetterSink deadLetterSink)` | Constructor | Creates a bus with explicit logging and dead-letter dependencies. Throws `ArgumentNullException` when either argument is `null`. |
| `Subscribe<TEvent>(Func<TEvent, Task> handler)` | `void` | Adds an asynchronous handler for `TEvent`. Multiple handlers can be registered for the same event type. Throws `ArgumentNullException` when `handler` is `null`. |
| `Unsubscribe<TEvent>(Func<TEvent, Task> handler)` | `void` | Removes the matching handler from `TEvent`'s subscriber list. Removes the event-type entry when its last handler is removed. Throws `ArgumentNullException` when `handler` is `null`. |
| `PublishAsync<TEvent>(TEvent event)` | `Task` | Publishes one event to a snapshot of its subscribers, invoking them sequentially. Throws `ArgumentNullException` when `event` is `null`. |
| `PublishManyAsync<TEvent>(IEnumerable<TEvent> events)` | `Task` | Enumerates and publishes events sequentially through `PublishAsync`. Throws `ArgumentNullException` when `events` is `null`. |
| `GetSubscriberCount<TEvent>()` | `int` | Returns the current number of handlers registered for `TEvent`, or zero when none are registered. |
| `Clear()` | `void` | Removes every subscription from the bus. Intended for testing and cleanup. |

### EventBusExtensions

| Member | Return type | Description |
|--------|-------------|-------------|
| `PublishProductCreatedAsync(this IEventBus eventBus, int productId, string productName, decimal price)` | `Task` | Creates and publishes a `ProductCreatedEvent`. Throws `ArgumentNullException` when `eventBus` or `productName` is `null`. |
| `PublishOrderPlacedAsync(this IEventBus eventBus, int orderId, int userId, decimal totalAmount, int itemCount)` | `Task` | Creates and publishes an `OrderPlacedEvent`. Throws `ArgumentNullException` when `eventBus` is `null`. |
| `PublishUserRegisteredAsync(this IEventBus eventBus, int userId, string email, string fullName)` | `Task` | Creates and publishes a `UserRegisteredEvent`. Throws `ArgumentNullException` when `eventBus`, `email`, or `fullName` is `null`. |
| `PublishCustomEventAsync(this IEventBus eventBus, string eventName, Dictionary<string, object> data)` | `Task` | Creates and publishes a `CustomEvent`. Throws `ArgumentNullException` when `eventBus`, `eventName`, or `data` is `null`. |

## Usage

### Example 1: Subscribe and publish

```csharp
var logger = loggerFactory.CreateLogger<EventBusImplementation>();
var eventBus = new EventBusImplementation(logger);

Func<OrderPlacedEvent, Task> handler = order =>
{
    Console.WriteLine($"Order {order.OrderId} was placed.");
    return Task.CompletedTask;
};

eventBus.Subscribe(handler);

await eventBus.PublishAsync(new OrderPlacedEvent
{
    OrderId = 42,
    UserId = 7,
    TotalAmount = 125.00m,
    ItemCount = 2,
    AggregateId = 42,
    AggregateType = "Order"
});

eventBus.Unsubscribe(handler);
```

### Example 2: Publish a common event

```csharp
IEventBus eventBus = serviceProvider.GetRequiredService<IEventBus>();

await eventBus.PublishProductCreatedAsync(
    productId: 10,
    productName: "Desk Lamp",
    price: 39.99m);
```

## Notes

- **Deployment scope:** Subscriptions exist only in the current process and are not persisted. Use a message broker when events must cross process or server boundaries.
- **Thread safety:** Subscription changes, clearing, and count reads are synchronized. Publishing copies the handler list while locked and releases the lock before invoking handlers.
- **Ordering:** Events passed to `PublishManyAsync` and handlers for an individual event are processed sequentially.
- **Failure handling:** Each failing handler receives up to three attempts. After the final failure, an `AggregateException` containing the attempt failures is passed to the configured dead-letter sink, and publishing continues with the next handler.
- **Subscription lifetime:** The bus holds strong references to handler delegates. Unsubscribe handlers that should no longer receive events, or call `Clear` during cleanup.
