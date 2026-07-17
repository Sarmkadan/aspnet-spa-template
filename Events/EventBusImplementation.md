# EventBusImplementation

A lightweight in-process event bus for single-server .NET applications. It maintains an in-memory registry of async event handlers keyed by event type and dispatches events synchronously to each registered handler in registration order. The implementation is thread-safe via internal locking and is intended for use cases where all components run in the same process (e.g., ASP.NET Core applications). For distributed scenarios, a dedicated message broker should be used instead.

## API

### `EventBusImplementation`
Constructor that initializes a new in-memory event bus with logging support.
- **Parameters**
  - `logger` – `ILogger<EventBusImplementation>` used for diagnostic logging.
- **Exceptions**
  - Throws `ArgumentNullException` if `logger` is `null`.

### `Subscribe<TEvent>(Func<TEvent, Task> handler)`
Registers an async handler for a specific event type.
- **Type Parameters**
  - `TEvent` – the event type, constrained to `DomainEvent`.
- **Parameters**
  - `handler` – async delegate to invoke when the event is published.
- **Exceptions**
  - Throws `ArgumentNullException` if `handler` is `null`.
- **Logging**
  - Logs an information message when the handler is successfully subscribed.

### `Unsubscribe<TEvent>(Func<TEvent, Task> handler)`
Removes a previously registered handler for an event type.
- **Type Parameters**
  - `TEvent` – the event type, constrained to `DomainEvent`.
- **Parameters**
  - `handler` – delegate to remove.
- **Behavior**
  - If the handler is the last subscriber for its event type, the event type entry is removed from the registry.

### `PublishAsync<TEvent>(TEvent @event)`
Publishes a single event and invokes all registered handlers sequentially in registration order.
- **Type Parameters**
  - `TEvent` – the event type, constrained to `DomainEvent`.
- **Parameters**
  - `@event` – the event instance to publish.
- **Exceptions**
  - Throws `ArgumentNullException` if `@event` is `null`.
- **Logging**
  - Logs an information message when publishing starts and after all handlers complete.
- **Error Handling**
  - Errors thrown by individual handlers are caught and logged; subsequent handlers are still invoked.

### `PublishManyAsync<TEvent>(IEnumerable<TEvent> events)`
Publishes multiple events of the same type sequentially.
- **Type Parameters**
  - `TEvent` – the event type, constrained to `DomainEvent`.
- **Parameters**
  - `events` – enumerable of events to publish.
- **Behavior**
  - Each event is published in sequence via `PublishAsync<TEvent>(TEvent)`.

### `GetSubscriberCount<TEvent>()`
Returns the number of handlers currently registered for an event type.
- **Type Parameters**
  - `TEvent` – the event type, constrained to `DomainEvent`.
- **Return Value**
  - `int` – count of handlers; zero if no handlers are registered.
- **Thread Safety**
  - Safe to call from any thread.

### `Clear()`
Removes all registered handlers (intended for testing or cleanup scenarios).
- **Behavior**
  - All event-type entries and their handlers are discarded.
- **Thread Safety**
  - Safe to call from any thread.

### `PublishProductCreatedAsync(this IEventBus eventBus, int productId, string productName, decimal price)`
Convenience extension method to raise a `ProductCreatedEvent`.
- **Parameters**
  - `eventBus` – the event bus instance.
  - `productId` – identifier of the created product.
  - `productName` – name of the created product.
  - `price` – price of the created product.
- **Behavior**
  - Constructs and publishes a new `ProductCreatedEvent` with the provided values.

### `PublishOrderPlacedAsync(this IEventBus eventBus, int orderId, int userId, decimal totalAmount, int itemCount)`
Convenience extension method to raise an `OrderPlacedEvent`.
- **Parameters**
  - `eventBus` – the event bus instance.
  - `orderId` – identifier of the placed order.
  - `userId` – identifier of the user who placed the order.
  - `totalAmount` – total monetary amount of the order.
  - `itemCount` – number of items in the order.
- **Behavior**
  - Constructs and publishes a new `OrderPlacedEvent` with the provided values.

### `PublishUserRegisteredAsync(this IEventBus eventBus, int userId, string email, string fullName)`
Convenience extension method to raise a `UserRegisteredEvent`.
- **Parameters**
  - `eventBus` – the event bus instance.
  - `userId` – identifier of the newly registered user.
  - `email` – email address of the user.
  - `fullName` – full name of the user.
- **Behavior**
  - Constructs and publishes a new `UserRegisteredEvent` with the provided values.

### `PublishCustomEventAsync(this IEventBus eventBus, string eventName, Dictionary<string, object> data)`
Convenience extension method to raise a `CustomEvent` with dynamic payload.
- **Parameters**
  - `eventBus` – the event bus instance.
  - `eventName` – name of the custom event.
  - `data` – dictionary of key/value pairs representing the event payload.
- **Behavior**
  - Constructs and publishes a new `CustomEvent` with the provided values.

## Usage

Example 1: Basic subscription and publishing
```csharp
// Setup
var services = new ServiceCollection();
services.AddLogging(logging => logging.AddConsole());
services.AddSingleton<IEventBus, EventBusImplementation>();
var provider = services.BuildServiceProvider();
var eventBus = provider.GetRequiredService<IEventBus>();

// Subscribe a handler
var orderHandler = new OrderEventHandler();
eventBus.Subscribe<OrderPlacedEvent>(orderHandler.HandleAsync);

// Publish an event
var order = new OrderPlacedEvent { OrderId = 1, UserId = 42, TotalAmount = 99.99m, ItemCount = 3 };
await eventBus.PublishAsync(order);

// Later, unsubscribe
// eventBus.Unsubscribe<OrderPlacedEvent>(orderHandler.HandleAsync);
```

Example 2: Using convenience extension methods
```csharp
// Setup
var services = new ServiceCollection();
services.AddLogging(logging => logging.AddConsole());
services.AddSingleton<IEventBus, EventBusImplementation>();
var provider = services.BuildServiceProvider();
var eventBus = provider.GetRequiredService<IEventBus>();

// Subscribe a handler for ProductCreatedEvent
eventBus.Subscribe<ProductCreatedEvent>(async e => 
{
    Console.WriteLine($"Product created: {e.ProductName} (${e.Price})");
});

// Raise events using extension methods
await eventBus.PublishProductCreatedAsync(1, "Laptop", 999.99m);
await eventBus.PublishOrderPlacedAsync(101, 42, 1999.98m, 2);
await eventBus.PublishUserRegisteredAsync(42, "user@example.com", "Jane Doe");
```

## Notes

- **Thread Safety**: All public methods are safe to call from multiple threads. Internally, a dedicated lock (`_subscriberLock`) protects the subscriber dictionary; handlers are copied under the lock before execution to avoid holding the lock during handler invocations.
- **Ordering**: Handlers are invoked in the order they were subscribed.
- **Error Resilience**: If a handler throws, the exception is caught and logged, and subsequent handlers are still executed. This prevents one failing handler from blocking other subscribers.
- **Memory**: Handlers are held strongly; ensure to call `Unsubscribe` when handlers are no longer needed to avoid memory leaks.
- **Testing**: The `Clear` method is provided for test scenarios where you need to reset the bus between tests.
- **Performance**: Publishing is synchronous and sequential; avoid expensive handlers in high-frequency scenarios.
- **Domain Events**: All events must derive from `DomainEvent`; the bus does not validate aggregate consistency—it simply dispatches events as they arrive.