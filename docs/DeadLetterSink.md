# DeadLetterSink

`IDeadLetterSink` defines how the event infrastructure handles a `DomainEvent` that could not be delivered after its handler exhausted all retry attempts. Implementations can decide how to process the undeliverable event, such as logging, persisting, or raising an alert.

`LoggingDeadLetterSink` is the default implementation. It writes an error log containing the failure, the event type name, and the event's `EventId`, then returns a completed task. Its constructor requires an `ILogger<LoggingDeadLetterSink>` and throws `ArgumentNullException` when the logger is `null`.

When used by `EventBusImplementation`, the sink receives an event after a handler fails three times. The exception supplied to the sink is an `AggregateException` containing the failures from those attempts.

## Public API

### `IDeadLetterSink.SendAsync<TEvent>`

```csharp
Task SendAsync<TEvent>(
    TEvent @event,
    Exception exception,
    CancellationToken cancellationToken = default)
    where TEvent : DomainEvent;
```

Handles an event that could not be delivered. `TEvent` must derive from `DomainEvent`. The method receives the failed event, the exception describing the delivery failure, and an optional cancellation token.

### `LoggingDeadLetterSink.SendAsync<TEvent>`

```csharp
public Task SendAsync<TEvent>(
    TEvent @event,
    Exception exception,
    CancellationToken cancellationToken = default)
    where TEvent : DomainEvent;
```

Logs the exception at error level with the event type and `EventId`, then returns `Task.CompletedTask`. The implementation accepts the cancellation token but does not use it.

## Usage

Supply a sink when constructing the event bus. If a subscribed handler exhausts its retry attempts, the bus calls the sink automatically.

```csharp
ILogger<LoggingDeadLetterSink> sinkLogger =
    loggerFactory.CreateLogger<LoggingDeadLetterSink>();
ILogger<EventBusImplementation> busLogger =
    loggerFactory.CreateLogger<EventBusImplementation>();

IDeadLetterSink deadLetterSink = new LoggingDeadLetterSink(sinkLogger);
var eventBus = new EventBusImplementation(busLogger, deadLetterSink);

eventBus.Subscribe<ProductCreatedEvent>(_ =>
    Task.FromException(new InvalidOperationException("Delivery failed")));

await eventBus.PublishAsync(new ProductCreatedEvent
{
    ProductId = 42,
    ProductName = "Desk lamp",
    Price = 29.99m
});
```

After the handler's final failed attempt, `LoggingDeadLetterSink` records the dead-lettered `ProductCreatedEvent`. `PublishAsync` then continues instead of propagating the handler failure to its caller.
